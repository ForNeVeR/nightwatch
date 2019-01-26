module Nightwatch.Program

open System
open System.Reflection

open Microsoft.Extensions.Hosting
open Serilog

open Nightwatch
open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Service

let private version = Assembly.GetEntryAssembly().GetName().Version

let private createLogger() =
    LoggerConfiguration().WriteTo.Console().CreateLogger()

[<EntryPoint>]
let main (argv : string []) : int =
    let logger = createLogger()
    Log.Logger <- logger

    try
        let programInfo = { version = version }
        let env = Environment.fixedEnvironment (Path Environment.CurrentDirectory)
        let fs = FileSystem.system

        let configPath =
            if argv.Length >= 2 && argv.[0] = "--config"
            then Path argv.[1]
            else Path "nightwatch.yml"
        let mutable scheduler = None

        let service = HostedService(logger, programInfo, env, fs, configPath)
        let host = HostBuilder()
                       .ConfigureServices(Service.configure service)
                       .Build()
        host.Run()

        0
    with
        | ex ->
            logger.Error(ex, "Service error")
            1
