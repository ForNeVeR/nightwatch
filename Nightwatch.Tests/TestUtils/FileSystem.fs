module Nightwatch.Tests.TestUtils.FileSystem

open System.IO
open System.Text
open System.Threading.Tasks

open Nightwatch.Core
open Nightwatch.Core.FileSystem

let mockFileSystem (paths : (string * string)[]) =
    let pathMap = Map paths
    let getFiles (Path path) (Mask mask) =
        paths
        |> Seq.map fst
        |> Seq.filter (fun p -> p.StartsWith(path + "/") && p.EndsWith(mask.Substring 1))
        |> Seq.map Path
        |> Task.FromResult
    let fileNotFound path =
        let allKeys = pathMap |> Map.toSeq |> Seq.map fst |> Seq.toArray
        raise <| FileNotFoundException(sprintf "File not found: %s\nAvailable files: %A" path allKeys)
    let openStream (Path path) : Task<Stream> =
        Map.tryFind path pathMap
        |> Option.defaultWith(fun () -> fileNotFound path)
        |> Encoding.UTF8.GetBytes
        |> fun bytes -> new MemoryStream(bytes) :> Stream
        |> Task.FromResult

    { FileSystem.system with getFilesRecursively = getFiles
                             openStream = openStream }
