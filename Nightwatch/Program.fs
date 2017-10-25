module Nightwatch.Program

open System
open System.Reflection
open Argu

open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Core.Network
open Nightwatch.Core.Resources
open Nightwatch.Configuration
open Nightwatch.Resources

let private version = Assembly.GetEntryAssembly().GetName().Version

let private fullVersion =
    sprintf "Nightwatch v. %A\nConfig file format v. %A"
        version
        Configuration.configFormatVersion

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
    async {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        do! Scheduler.start scheduler
        printfn "Scheduler started"
        printfn "Press any key to stop"
        ignore <| Console.ReadKey()
        printfn "Stopping"
        do! Scheduler.stop scheduler
        printfn "Bye"
    } |> Async.RunSynchronously

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

let private printUsage() =
    printfn "Arguments: <path to config directory>"

let resourceFactories = [| Http.factory Http.system; Shell.factory Process.system |]

let private configureResourceRegistry() =
    let names = resourceFactories |> Seq.map (fun f -> f.resourceType)
    printfn "Available resources: %s" (String.Join(", ", names))
    Registry.create resourceFactories

let private readConfiguration path factories : Result<Resource seq, InvalidConfiguration seq> =
    async {
        let! resources = Configuration.read factories FileSystem.system path
        let (results, errors) = splitResults resources
        return
            if Seq.isEmpty errors
            then Ok results
            else Error errors
    } |> Async.RunSynchronously

let private run = function
| Ok resources ->
    runScheduler resources
    ExitCodes.success
| Error errors ->
    printfn "%s" (errorsToString errors)
    ExitCodes.configurationError

[<RequireQualifiedAccess>]
type CLIArguments =
    | Version
    | [<MainCommand>] Arguments of configPath:string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Version -> "display nightwatch's version"
            | Arguments _ -> "path to config directory"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "nightwatch")
    let results = parser.ParseCommandLine(argv, raiseOnUsage = false)

    if results.Contains <@ CLIArguments.Version @> then
        printfn "%A" version
        ExitCodes.success
    else if results.Contains <@ CLIArguments.Arguments @> then
        let configPath = results.GetResult <@ CLIArguments.Arguments @>

        printfn "%s" fullVersion
        configureResourceRegistry()
        |> readConfiguration (Path configPath)
        |> run
    else
        printfn "%s" (parser.PrintUsage())
        ExitCodes.invalidArguments
