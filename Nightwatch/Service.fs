module Nightwatch.Service

open System

open FSharp.Control.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog

open Nightwatch
open Nightwatch.Core
open Nightwatch.Core.Environment
open Nightwatch.Core.FileSystem
open Nightwatch.Core.Network
open Nightwatch.ResourceConfiguration
open Nightwatch.Resources
open Nightwatch.ServiceModel

type ProgramInfo =
    { version : Version }

let private logFullVersion (logger : ILogger) programInfo =
    logger.Information("Nightwatch v. {0}", programInfo.version)
    logger.Information("Config file format v. {0}", ResourceConfiguration.configFormatVersion)

let private resourceFactories = [| Http.factory Http.system; Shell.factory Process.system |]

let private configureResourceRegistry (logger : ILogger) =
    let names = resourceFactories |> Seq.map (fun f -> f.resourceType)
    logger.Information("Available resources: {0}", String.Join(", ", names))
    Registry.create resourceFactories

let private readResources fs config factories : Async<Result<Resource [], InvalidConfiguration []>> =
    let splitResults seq =
        let chooseOk = function
        | Ok x -> Some x
        | Error _ -> None

        let chooseError = function
        | Ok _ -> None
        | Error x -> Some x

        let isOk = chooseOk >> Option.isSome

        let (results, errors) =
            seq
            |> Seq.toArray
            |> Array.partition isOk
        Array.choose chooseOk results, Array.choose chooseError errors

    async {
        let! resources = Async.AwaitTask <| ResourceConfiguration.read factories fs config
        let (results, errors) = splitResults resources
        return
            if Seq.isEmpty errors
            then Ok results
            else Error errors
    }

let private createScheduler resources =
    let schedule = Scheduler.prepareSchedule resources
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        return scheduler
    }

let private startService logger programInfo env fs (configFilePath : Path) =
    let errorsToString errors =
        let printError { path = (Path path); id = id; message = message } =
            sprintf "Path: %s\nId: %s\nMessage: %s"
                path
                (defaultArg id "N/A")
                message

        String.Join("\n", Seq.map printError errors)

    async {
        logFullVersion logger programInfo
        logger.Information("Configuration file location: {0}", configFilePath)
        let! config = ProgramConfiguration.read env fs configFilePath
        logger.Information("Resource directory location: {0}", config.resourceDirectory)
        let registry = configureResourceRegistry logger
        let! resources = readResources fs config registry
        match resources with
        | Ok resources ->
            logger.Information("{0} resources loaded", resources.Length)
            let! scheduler = Async.AwaitTask <| createScheduler resources
            do! Scheduler.start scheduler
            return Some scheduler
        | Error errors ->
            logger.Error(errorsToString errors)
            return None
    }

let configure (logger : ILogger)
              (programInfo: ProgramInfo)
              (environment : Environment)
              (fs : FileSystem)
              (configFilePath : Path)
              (services : IServiceCollection) : unit =
    let startService =
        async {
            let! service = startService logger programInfo environment fs configFilePath
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
