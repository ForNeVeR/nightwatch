module Nightwatch.Program

open System
open System.IO
open System.Reflection
open System.Threading.Tasks

open Argu
open FSharp.Control.Tasks
open Serilog
open Topshelf

open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Core.Network
open Nightwatch.Core.Resources
open Nightwatch.Configuration
open Nightwatch.Resources

let private version = Assembly.GetEntryAssembly().GetName().Version

let private logFullVersion() =
    Log.Information("Nightwatch v. {0}", version)
    Log.Information("Config file format v. {0}", Configuration.configFormatVersion)

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
    Seq.choose chooseOk results, Seq.choose chooseError errors

let private createScheduler resources =
    let schedule = Scheduler.prepareSchedule resources
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        return scheduler
    }

module private ExitCodes =
    let success = 0
    let configurationError = 1

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

let private readConfiguration path factories : Task<Result<Resource seq, InvalidConfiguration seq>> =
    task {
        let! resources = Configuration.read factories FileSystem.system path
        let (results, errors) = splitResults resources
        return
            if Seq.isEmpty errors
            then Ok results
            else Error errors
    }

[<RequireQualifiedAccess>]
type CliArguments =
    | Version
    | [<MainCommand>] Arguments of configPath:string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Version -> "display nightwatch's version."
            | Arguments _ -> "path to config directory."

let private defaultConfigPath = Path.Combine(Environment.CurrentDirectory, "samples") // TODO[F]: Remove the "samples" part

let private runService scheduler =
    let doSync f _ =
        synchronize f
        true

    // TODO[F]: It will throw an exception if it will find any additional command-line arguments, e.g. `./config/`

    Service.Default
    |> with_start (doSync <| Scheduler.start scheduler)
    |> with_stop (doSync <| Scheduler.stop scheduler)
    |> service_name "nightwatch"
    |> display_name "Nightwatch"
    |> description "Nightwatch service"
    |> run

[<EntryPoint>]
let main (argv : string[]) : int =
    let initializeLogger() =
        Log.Logger <-
            LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger()

    initializeLogger()

    let parser = ArgumentParser.Create<CliArguments>(programName = "nightwatch")
    let arguments = parser.ParseCommandLine(argv, raiseOnUsage = false)

    let getConfigPath() =
    // TODO[F]: This messes up with `install` argument, fix please
//         if arguments.Contains CliArguments.Arguments then
//             arguments.GetResult CliArguments.Arguments
//         else
         defaultConfigPath

    if arguments.IsUsageRequested then
        printfn "%s" (parser.PrintUsage())
        ExitCodes.success
    else if arguments.Contains CliArguments.Version then
        printfn "%A" version
        ExitCodes.success
    else
        let configPath = getConfigPath()
        let registry = configureResourceRegistry()
        let config = synchronize <| readConfiguration (Path configPath) registry
        match config with
        | Ok resources ->
            logFullVersion()
            let scheduler = synchronize <| createScheduler resources
            runService scheduler
        | Error errors ->
            Log.Error(errorsToString errors)
            ExitCodes.configurationError
