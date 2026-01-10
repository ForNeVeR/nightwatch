// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.TestUtils.FileSystem

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Threading.Tasks

open Nightwatch.Core.FileSystem
open TruePath

let MockedRoot = AbsolutePath(if OperatingSystem.IsWindows() then @"C:\" else "/")

let mockFileSystem (paths : (string * string)[]) =
    let pathMap =
        paths
        |> Seq.map(fun(k, v) -> KeyValuePair(MockedRoot / k, v))
        |> Dictionary

    let getFiles (path: AbsolutePath) (mask: LocalPathPattern) =
        pathMap
        |> Seq.map _.Key
        |> Seq.filter (fun p -> p.StartsWith path && p.FileName.EndsWith(mask.Value.Substring 1))
        |> Seq.toArray
        |> Task.FromResult<IReadOnlyList<AbsolutePath>>

    let fileNotFound(path: IPath) =
        let allKeys = pathMap |> Seq.map _.Key |> Seq.toArray
        raise <| FileNotFoundException $"File not found: %s{path.Value}\nAvailable files: %A{allKeys}"
    let openStream(path: AbsolutePath): Task<Stream> =
        match pathMap.TryGetValue path with
        | false, _ -> fileNotFound path
        | true, content ->
            content
            |> Encoding.UTF8.GetBytes
            |> fun bytes -> new MemoryStream(bytes) :> Stream
            |> Task.FromResult

    {
        GetFilesRecursively = getFiles
        OpenStream = openStream
    }
