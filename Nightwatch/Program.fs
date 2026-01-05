module Nightwatch.Program

open System
open System.Reflection

open Argu
open Microsoft.Extensions.Hosting
open Serilog

open Nightwatch
open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Service
open Nightwatch.ServiceModel

let private version = Assembly.GetEntryAssembly().GetName().Version

let private createLogger() =
    LoggerConfiguration().WriteTo.Console().CreateLogger()

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

[<EntryPoint>]
let main (argv : string []) : int =
    let logger = createLogger()
    Log.Logger <- logger

    let parser = ArgumentParser.Create<CliArguments>(programName = "nightwatch")
    let arguments = parser.ParseCommandLine(argv, raiseOnUsage = false)

    if arguments.IsUsageRequested then
        printfn "%s" (parser.PrintUsage())
        ExitCodes.success
    else if arguments.Contains CliArguments.Version then
        printfn "Nightwatch %A" version
        ExitCodes.success
    else
        try
            let configPath = Path(arguments.GetResult(CliArguments.Config, "nightwatch.yml"))
            let programInfo = { version = version }
            let env = Environment.fixedEnvironment (Path Environment.CurrentDirectory)
            let fs = FileSystem.system

            let builder = HostBuilder().ConfigureServices(Service.configure logger programInfo env fs configPath)

            if arguments.Contains CliArguments.Service
            then runAsService builder
            else run builder

            ExitCodes.success
        with
            | ex ->
                logger.Error(ex, "Service error")
                ExitCodes.error
