// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module Nightwatch.EntryPoint

open System.Reflection

open Argu
open Microsoft.Extensions.Hosting
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

/// Main Nightwatch entry point. Returns zero if success or a non-zero exit code in case of errors.
let Main(args: string[]): int =
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

        try
            let programInfo = { version = version }

            let builder = HostBuilder().ConfigureServices(configure logger programInfo fs config)

            if arguments.Contains CliArguments.Service
            then runAsService builder
            else run builder

            ExitCodes.success
        with
            | ex ->
                logger.Error(ex, "Service error")
                ExitCodes.error

let FsiMain(args: string[]): int =
    let args =
        if args.Length > 0 && args[0].EndsWith ".fsx"
        then Array.skip 1 args
        else args
    Main args
