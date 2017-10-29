module Nightwatch.Tests.Resources.Shell

open System
open System.Threading.Tasks

open FSharp.Control.Tasks
open Xunit

open Nightwatch.Core.Process
open Nightwatch.Resources

let private getChecker controller command =
    let factory = Shell.factory controller

    let param = [| "cmd", command |] |> Map.ofArray
    factory.create.Invoke param

[<Fact>]
let ``Shell Resource starts a process``() =
    let processName = "any.exe"
    let args = [|"-a"; "--b"; "test"|]
    let command = processName + " " + String.Join(' ', args)

    let mutable startedCommand = None
    let mutable startedArgs = None
    let controller = { execute = fun name args ->
        startedCommand <- Some name
        startedArgs <- Some args
        Task.FromResult 0 }
    let checker = getChecker controller command
    task {
        let! result = checker.Invoke()
        Assert.Equal(processName, Option.get startedCommand)
        Assert.Equal<string []>(args, Option.get startedArgs)
    }

[<Fact>]
let ``Shell Resource returns success if process returns zero exit code``() =
    let controller = { execute = fun _ _ -> Task.FromResult 0 }
    let checker = getChecker controller ""
    task {
        let! result = checker.Invoke()
        Assert.True result
    }

[<Fact>]
let ``Shell Resource returns success if process returns nonzero exit code``() =
    let controller = { execute = fun _ _ -> Task.FromResult 1 }
    let checker = getChecker controller ""
    task {
        let! result = checker.Invoke()
        Assert.False result
    }
