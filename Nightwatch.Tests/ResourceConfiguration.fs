module Nightwatch.Tests.ResourceConfiguration

open System
open System.IO
open System.Text
open System.Threading.Tasks

open Xunit
open FSharp.Control.Tasks

open Nightwatch
open Nightwatch.ProgramConfiguration
open Nightwatch.ResourceConfiguration
open Nightwatch.Core
open Nightwatch.Core.FileSystem
open Nightwatch.Core.Resources
open Nightwatch.Resources
open Nightwatch.Tests.TestUtils.FileSystem

let private programConfiguration =
    { baseDirectory = Path "."
      resourceDirectory = Path "dir" }

[<Fact>]
let ``ResourceConfiguration should read the YAML file`` () =
    let text = @"version: 0.0.1.0
id: test
schedule: 00:05:00
type: test
param:
    check: ping localhost"
    let checker = ResourceChecker(fun () -> failwith "nop")
    let expected =
        { id = "test"
          runEvery = TimeSpan.FromMinutes 5.0
          checker = checker }
    let mutable parsedParam = None
    let factory : ResourceFactory =
        { resourceType = "test"
          create = Func<_, _>(fun param -> parsedParam <- Some param; checker) }
    let registry = Registry.create [| factory |]
    let fileSystem = mockFileSystem [| "dir/test.yml", text |]
    task {
        let! result = ResourceConfiguration.read registry fileSystem programConfiguration
        Assert.Equal<_>([| Ok expected |], result)

        let param = Option.get parsedParam
        Assert.Equal("ping localhost", param.["check"])
    }

let private emptyRegistry = Registry.create [| |]

[<Fact>]
let ``ResourceConfiguration returns error if the type is not registered in the factory``() =
    let text = @"version: 0.0.1.0
id: test
schedule: 00:05:00
type: test"
    let path = "dir/test.yml"
    let fileSystem = mockFileSystem [| path, text |]
    let expected = Error { path = (Path path)
                           id = Some "test"
                           message = "The resource factory for type \"test\" is not registered" }
    task {
        let! result = ResourceConfiguration.read emptyRegistry fileSystem programConfiguration
        Assert.Equal([| expected |], result)
    }

[<Fact>]
let ``ResourceConfiguration should ignore non-YAML file`` () =
    let fileSystem = mockFileSystem [| "test.yml2", "" |]
    task {
        let! result = ResourceConfiguration.read emptyRegistry fileSystem programConfiguration
        Assert.Equal<Result<_, _>>(Seq.empty, result)
    }
