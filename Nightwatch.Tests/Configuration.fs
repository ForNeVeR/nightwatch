module Nightwatch.Tests.Configuration

open System
open System.IO
open System.Text

open Xunit

open Nightwatch
open Nightwatch.Configuration
open Nightwatch.Core.Resources
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
    let checker = fun () -> failwith "nop"
    let expected =
        { id = "test"
          runEvery = TimeSpan.FromMinutes 5.0
          checker = checker }
    let factory =
        { resourceType = "test"
          create = fun _ -> checker }
    let fileSystem = mockFileSystem [| "dir/test.yml", text |]
    async {
        let! result = Configuration.read [| factory |] fileSystem (Path "dir")
        Assert.Equal<_>([| Ok expected |], result)
    } |> Async.StartAsTask

// TODO[F]: Add test for case when there is no corresponding factory for the type

[<Fact>]
let ``Configuration should ignore non-YAML file`` () =
    let fileSystem = mockFileSystem [| "test.yml2", "" |]
    async {
        let! result = Configuration.read [| |] fileSystem (Path "dir")
        Assert.Equal<Result<_, _>>(Seq.empty, result)
    } |> Async.StartAsTask
