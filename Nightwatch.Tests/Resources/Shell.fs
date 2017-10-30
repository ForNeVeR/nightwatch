module Nightwatch.Tests.Resources.Shell

open System
open System.Threading.Tasks

open FSharp.Control.Tasks
open Xunit

open Nightwatch.Core.Process
open Nightwatch.Resources

let private getChecker controller (command: string) (args: string[]) =
    let factory = Shell.factory controller
    let args = String.Join(' ', args)
    let param = [| "cmd", command; "args", args |] |> Map.ofArray
    factory.create.Invoke param

[<Fact>]
let ``Shell Resource starts a process``() =
    let command = "any.exe"
    let args = [|"-a"; "--b"; "test"|]

    let mutable startedCommand = None
    let mutable startedArgs = None
    let controller = { execute = fun name args ->
        startedCommand <- Some name
        startedArgs <- Some args
        Task.FromResult 0 }
    let checker = getChecker controller command args
    task {
        let! result = checker.Invoke()
        Assert.Equal(command, Option.get startedCommand)
        Assert.Equal(Some args, startedArgs)
    }

[<Fact>]
let ``Shell Resource returns success if process returns zero exit code``() =
    let controller = { execute = fun _ _ -> Task.FromResult 0 }
    let checker = getChecker controller "" Array.empty<_>
    task {
        let! result = checker.Invoke()
        Assert.True result
    }

[<Fact>]
let ``Shell Resource returns success if process returns nonzero exit code``() =
    let controller = { execute = fun _ _ -> Task.FromResult 1 }
    let checker = getChecker controller "" Array.empty<_>
    task {
        let! result = checker.Invoke()
        Assert.False result
    }
