module Nightwatch.Tests.Configuration

open System
open System.IO
open System.Text

open Xunit

open Nightwatch
open Nightwatch.Configuration
open Nightwatch.Core.Resources
open Nightwatch.Resources
open Nightwatch.FileSystem

let private mockFileSystem (paths : (string * string)[]) =
    let pathMap = Map paths
    let getFiles (Path path) (Mask mask) =
        async {
            return
                paths
                |> Seq.map fst
                |> Seq.filter (fun p -> p.StartsWith(path + "/") && p.EndsWith(mask.Substring 1))
                |> Seq.map Path
        }
    let openStream (Path path) : Async<Stream> =
        let text = Map.find path pathMap
        let bytes = Encoding.UTF8.GetBytes text
        async {
            return upcast new MemoryStream(bytes)
        }

    { FileSystem.system with getFilesRecursively = getFiles
                             openStream = openStream }

[<Fact>]
let ``Configuration should read the YAML file`` () =
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
    let factory : ResourceFactory =
        { resourceType = "test"
          create = Func<_, _>(fun _ -> checker) }
    let registry = Registry.create [| factory |]
    let fileSystem = mockFileSystem [| "dir/test.yml", text |]
    async {
        let! result = Configuration.read registry fileSystem (Path "dir")
        Assert.Equal<_>([| Ok expected |], result)
    } |> Async.StartAsTask

let private emptyRegistry = Registry.create [| |]

[<Fact>]
let ``Confiuguration returns error if the type is not registered in the factory``() =
    let text = @"version: 0.0.1.0
id: test
schedule: 00:05:00
type: test"
    let path = "dir/test.yml"
    let fileSystem = mockFileSystem [| path, text |]
    let expected = Error { path = (Path path)
                           id = Some "test"
                           message = "The resource factory for type \"test\" is not registered" }
    async {
        let! result = Configuration.read emptyRegistry fileSystem (Path "dir")
        Assert.Equal([| expected |], result)
    } |> Async.StartAsTask

[<Fact>]
let ``Configuration should ignore non-YAML file`` () =
    let fileSystem = mockFileSystem [| "test.yml2", "" |]
    async {
        let! result = Configuration.read emptyRegistry fileSystem (Path "dir")
        Assert.Equal<Result<_, _>>(Seq.empty, result)
    } |> Async.StartAsTask
