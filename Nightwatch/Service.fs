// SPDX-FileCopyrightText: 2019-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Service

open System

open System.Collections.Generic
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Nightwatch.Core.Notifications
open Nightwatch.Core.Resources
open Nightwatch.NotificationConfiguration
open Nightwatch.ProgramConfiguration
open Serilog

open Nightwatch
open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Core.Network
open Nightwatch.ResourceConfiguration
open Nightwatch.Resources
open Nightwatch.Notifications
open Nightwatch.CheckState
open Nightwatch.ServiceModel

type ProgramInfo =
    { version : Version }

let private logFullVersion (logger : ILogger) programInfo =
    logger.Information("Nightwatch v. {0}", programInfo.version)
    logger.Information("Config file format v. {0}", configFormatVersion)

let resourceFactories = [| Http.factory Http.system; Shell.factory Process.system |]
let notificationFactories = [| Telegram.Factory |]

/// <remarks>Uses <see cref="P:Nightwatch.Service.resourceFactories"/>.</remarks>
let ConfigureResourceRegistry (logger: ILogger) =
    let names = resourceFactories |> Seq.map _.resourceType
    logger.Information("Available resources: {0}", String.Join(", ", names))
    ResourceRegistry.Create resourceFactories

/// <remarks>Uses <see cref="P:Nightwatch.Service.notificationFactories"/>.</remarks>
let ConfigureNotificationRegistry (logger: ILogger) =
    let names = notificationFactories |> Seq.map _.NotificationType
    logger.Information("Available notification providers: {0}", String.Join(", ", names))
    NotificationRegistry.Create notificationFactories

let private splitResults seq =
    let chooseOk = function
    | Ok x -> Some x
    | Error _ -> None

    let chooseError = function
    | Ok _ -> None
    | Error x -> Some x

    let isOk = chooseOk >> Option.isSome

    let results, errors =
        seq
        |> Seq.toArray
        |> Array.partition isOk
    Array.choose chooseOk results, Array.choose chooseError errors

let private readResources fs config factories : Async<Result<Resource [], InvalidConfiguration []>> =
    async {
        let! resources = Async.AwaitTask <| read factories fs config
        let results, errors = splitResults resources
        return
            if Seq.isEmpty errors
            then Ok results
            else Error errors
    }

let private readNotifications fs notificationDirectory registry : Async<Result<NotificationProvider [],
                                                                            NotificationConfigurationError[]>> =
    async {
        let! notifications = Async.AwaitTask <| NotificationConfiguration.read registry fs notificationDirectory
        let results, errors = splitResults notifications
        return
            if Seq.isEmpty errors
            then Ok results
            else Error errors
    }

let private createScheduler providers stateTracker resources =
    let schedule = Scheduler.prepareSchedule providers stateTracker resources
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        return scheduler
    }

let private startService logger resourceRegistry notificationRegistry programInfo fs (config: ProgramConfiguration) =
    let resourceErrorsToString (errors: InvalidConfiguration[]) =
        let printError (error: InvalidConfiguration) =
            sprintf "Path: %s\nId: %s\nMessage: %s"
                error.Path.Value
                (defaultArg error.Id "N/A")
                error.Message

        String.Join("\n", Seq.map printError errors)

    let notificationErrorsToString (errors: NotificationConfigurationError[]) =
        let printError({ Path = path; Id = id; Message = message }: NotificationConfigurationError) =
            sprintf "Path: %s\nId: %s\nMessage: %s"
                path.Value
                (defaultArg id "N/A")
                message

        String.Join("\n", Seq.map printError errors)

    async {
        logFullVersion logger programInfo
        logger.Information("Resource directory location: {0}", config.ResourceDirectory)

        // Load notification providers if directory is configured
        let! notificationProviders =
            match config.NotificationDirectory with
            | Some dir ->
                logger.Information("Notification directory location: {0}", dir)
                async {
                    let! result = readNotifications fs dir notificationRegistry
                    match result with
                    | Ok providers ->
                        logger.Information("{0} notification providers loaded", providers.Length)
                        return providers
                    | Error errors ->
                        logger.Error("Failed to load notifications:\n{0}", notificationErrorsToString errors)
                        return failwithf
                                   $"Cannot start Nightwatch service: failed to load notification configuration:\n%s{notificationErrorsToString errors}"
                }
            | None ->
                logger.Information("No notification directory configured")
                async { return [||] }

        let providersMap =
            notificationProviders
            |> Array.map (fun p -> p.id, p)
            |> Map.ofArray

        let stateTracker = ResourceStateTracker()

        let! resources = readResources fs config resourceRegistry
        match resources with
        | Ok resources ->
            logger.Information("{0} resources loaded", resources.Length)
            let! scheduler = Async.AwaitTask <| createScheduler providersMap stateTracker resources
            do! Scheduler.start scheduler
            return Some scheduler
        | Error errors ->
            logger.Error(resourceErrorsToString errors)
            return None
    }

let internal Configure
    (
        logger: ILogger,
        programInfo: ProgramInfo,
        resources: IReadOnlyDictionary<string, ResourceFactory>,
        notifications: NotificationRegistry,
        fs: FileSystem,
        config: ProgramConfiguration
    )
    (services: IServiceCollection)
    : unit =
    let startService =
        async {
            let! service = startService logger resources notifications programInfo fs config
            let startedService =
                match service with
                | None -> failwith "Cannot start Nightwatch service"
                | Some s -> s
            return startedService
        }
    let service =
        HostedService(
            (fun ct -> Async.StartAsTask(startService, cancellationToken = ct)),
            fun ct scheduler -> upcast Async.StartAsTask(Scheduler.stop scheduler, cancellationToken = ct))
    services.AddSingleton<IHostedService> service
    |> ignore
