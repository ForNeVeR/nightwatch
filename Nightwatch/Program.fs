module Nightwatch.Program

open System
open System.Reflection
open System.Threading.Tasks

open Argu
open Serilog
open FSharp.Control.Tasks

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

let private synchronize (t:Task<'T>) = t.Result

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

let private runScheduler resources =
    let schedule = Scheduler.prepareSchedule resources
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        do! Scheduler.start scheduler
        Log.Information("Scheduler started")
        printfn "Press any key to stop..."
        ignore <| Console.ReadKey()
        Log.Information("Stopping...")
        do! Scheduler.stop scheduler
        Log.Information("Bye")
    }

module private ExitCodes =
    let success = 0
    let invalidArguments = 1
    let configurationError = 2

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

let private run = function
| Ok resources ->
    logFullVersion()
    runScheduler resources |> ignore
    ExitCodes.success
| Error errors ->
    Log.Error(errorsToString errors)
    ExitCodes.configurationError

[<RequireQualifiedAccess>]
type CliArguments =
    | Version
    | [<MainCommand>] Arguments of configPath:string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Version -> "display nightwatch's version."
            | Arguments _ -> "path to config directory."

[<EntryPoint>]
let main argv =
    Log.Logger <- LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger()

    let parser = ArgumentParser.Create<CliArguments>(programName = "nightwatch")
    let results = parser.ParseCommandLine(argv, raiseOnUsage = false)

    if results.Contains CliArguments.Version then
        printfn "%A" version
        ExitCodes.success
    else if results.Contains CliArguments.Arguments then
        let configPath = results.GetResult CliArguments.Arguments
        configureResourceRegistry()
        |> readConfiguration (Path configPath)
        |> synchronize
        |> run
    else
        printfn "%s" (parser.PrintUsage())
        ExitCodes.invalidArguments
