module Nightwatch.Program

open System
open System.IO
open System.Reflection
open System.Threading.Tasks

open FSharp.Control.Tasks
open Serilog
open Topshelf

open Nightwatch
open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Core.Network
open Nightwatch.Core.Resources
open Nightwatch.ProgramConfiguration
open Nightwatch.ResourceConfiguration
open Nightwatch.Resources

let private version = Assembly.GetEntryAssembly().GetName().Version

let private logFullVersion() =
    Log.Information("Nightwatch v. {0}", version)
    Log.Information("Config file format v. {0}", ResourceConfiguration.configFormatVersion)

let private synchronize(t : Task<'T>) = t.GetAwaiter().GetResult()

let private splitResults seq =
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

let private createScheduler resources =
    let schedule = Scheduler.prepareSchedule resources
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        return scheduler
    }

let private errorsToString errors =
    let printError { path = (Path path); id = id; message = message } =
        sprintf "Path: %s\nId: %s\nMessage: %s"
            path
            (defaultArg id "N/A")
            message

    String.Join("\n", Seq.map printError errors)

let resourceFactories = [| Http.factory Http.system; Shell.factory Process.system |]

let private configureResourceRegistry() =
    let names = resourceFactories |> Seq.map (fun f -> f.resourceType)
    Log.Information("Available resources: {0}", String.Join(", ", names))
    Registry.create resourceFactories

let private readResources fs config factories : Async<Result<Resource[], InvalidConfiguration[]>> =
    async {
        let! resources = Async.AwaitTask <| ResourceConfiguration.read factories fs config
        let (results, errors) = splitResults resources
        return
            if Seq.isEmpty errors
            then Ok results
            else Error errors
    }

let private initializeLogger() =
    Log.Logger <-
        LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger()

let private startService env fs (configFilePath : Path) =
    async {
        logFullVersion()
        Log.Logger.Information("Configuration file location: {0}", configFilePath)
        let! config = ProgramConfiguration.read env fs configFilePath
        Log.Logger.Information("Resource directory location: {0}", config.resourceDirectory)
        let registry = configureResourceRegistry()
        let! resources = readResources fs config registry
        match resources with
        | Ok resources ->
            Log.Logger.Information("{0} resources loaded", resources.Length)
            let! scheduler = Async.AwaitTask <| createScheduler resources
            do! Scheduler.start scheduler
            return Some scheduler
        | Error errors ->
            Log.Error(errorsToString errors)
            return None
    }

let private stopService scheduler = async {
    do! Scheduler.stop scheduler
    return true
}

[<EntryPoint>]
let main (argv : string[]) : int =
    initializeLogger()

    let env = Environment.fixedEnvironment(Path Environment.CurrentDirectory)
    let fs = FileSystem.system

    let mutable configPath = Path "nightwatch.yml"
    let mutable scheduler = None

    Service.Default
    |> add_command_line_definition "config" (fun newPath -> configPath <- Path newPath)
    |> with_start(fun _ ->
        scheduler <- Async.RunSynchronously(startService env fs configPath)
        Option.isSome scheduler)
    |> with_stop(fun _ ->
        let scheduler = Option.get scheduler
        Async.RunSynchronously(stopService scheduler))
    |> service_name "nightwatch"
    |> display_name "Nightwatch"
    |> description "Nightwatch service"
    |> run
