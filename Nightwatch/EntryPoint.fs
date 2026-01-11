// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module Nightwatch.EntryPoint

open System.Collections.Generic
open System.Reflection

open Argu
open Microsoft.Extensions.Hosting
open Nightwatch.Core.Notifications
open Nightwatch.Core.Resources
open Serilog

open Nightwatch
open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Service
open Nightwatch.ServiceModel
open TruePath

let private version = Assembly.GetEntryAssembly().GetName().Version

let private createLogger(logFilePath: AbsolutePath option) =
    let config = LoggerConfiguration()
    match logFilePath with
    | Some path -> config.WriteTo.File(path.Value)
    | None -> config.WriteTo.Console()
    |> _.CreateLogger()

module ExitCodes =
    let success = 0
    let error = 1

[<RequireQualifiedAccess>]
type CliArguments =
    |  [<First; Last>] Version
    |  [<Unique>] Config of configPath : string
    |  [<Unique>] Service
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Version -> "print the program version."
            | Config _ -> "path to the YML configuration file."
            | Service -> "start the application as a Windows service."

let private run(builder : IHostBuilder) =
    builder.Build().Run()

let private runAsService(builder : IHostBuilder) =
    builder.ConfigureServices Lifetime.useServiceBasedRuntime
    |> run

/// <summary>Main Nightwatch entry point.</summary>
/// <param name="args">Command-line arguments used to start the service <see cref="CliArguments"/> for details.</param>
/// <param name="resourceRegistry">
///     <para>
///         A mapping from a resource type name to a resource factory. If not passed,
///         <see cref="Service.ConfigureResourceFactory"/> will be used.
///     </para>
///     <para>Use <see cref="Resources.ResourceRegistry.Create"/> to create a custom registry if needed.</para>
/// </param>
/// <param name="notificationRegistry">
///     <para>
///         A mapping from a notification type name to a notification factory. If not passed,
///         <see cref="Service.ConfigureNotificationFactory"/> will be used.
///     </para>
///     <para>
///         Use <see cref="Notifications.NotificationRegistryModule.Create"/> to create a custom registry if needed.
///     </para>
/// </param>
/// <returns>See <see cref="ExitCodes"/>.</returns>
let Main(
    args: string[],
    resourceRegistry: IReadOnlyDictionary<string, ResourceFactory> option,
    notificationRegistry: IReadOnlyDictionary<string, NotificationFactory> option
): int =
    let parser = ArgumentParser.Create<CliArguments>(programName = "nightwatch")
    let arguments = parser.ParseCommandLine(args, raiseOnUsage = false)

    if arguments.IsUsageRequested then
        printfn $"%s{parser.PrintUsage()}"
        ExitCodes.success
    else if arguments.Contains CliArguments.Version then
        printfn $"Nightwatch %A{version}"
        ExitCodes.success
    else
        let configPath = LocalPath(arguments.GetResult(CliArguments.Config, "nightwatch.yml"))
        let env = Environment.fixedEnvironment AbsolutePath.CurrentWorkingDirectory
        let fs = system

        let config = ProgramConfiguration.read env fs configPath |> Async.RunSynchronously

        let logger = createLogger config.LogFilePath
        Log.Logger <- logger

        let resourceRegistry = resourceRegistry |> Option.defaultWith(fun() -> ConfigureResourceRegistry logger)
        let notificationRegistry =
            notificationRegistry |> Option.defaultWith(fun() -> ConfigureNotificationRegistry logger)

        try
            let programInfo = { version = version }

            let builder = HostBuilder().ConfigureServices(Configure(logger, programInfo, resourceRegistry, notificationRegistry, fs, config))

            if arguments.Contains CliArguments.Service
            then runAsService builder
            else run builder

            ExitCodes.success
        with
            | ex ->
                logger.Error(ex, "Service error")
                ExitCodes.error

/// <inheritdoc cref="Main"/>
let FsiMain(
    args: string[],
    resourceRegistry: IReadOnlyDictionary<string, ResourceFactory> option,
    notificationRegistry: IReadOnlyDictionary<string, NotificationFactory> option
): int =
    let args =
        if args.Length > 0 && args[0].EndsWith ".fsx"
        then Array.skip 1 args
        else args
    Main(args, resourceRegistry, notificationRegistry)
