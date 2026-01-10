// SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Core.FileSystem

open System.Collections.Generic
open System.IO
open System.Threading.Tasks
open TruePath

type FileSystem = {
    GetFilesRecursively: AbsolutePath -> LocalPathPattern -> Task<IReadOnlyList<AbsolutePath>>
    OpenStream: AbsolutePath -> Task<Stream>
}

let private getFilesRecursively (path: AbsolutePath) (pattern: LocalPathPattern): Task<IReadOnlyList<AbsolutePath>> =
    Task.Run(fun () ->
        let result =
            Directory.GetFileSystemEntries(path.Value, pattern.Value, SearchOption.AllDirectories)
            |> Seq.map AbsolutePath
            |> Seq.toArray
        result: IReadOnlyList<AbsolutePath>
    )

let private openStream (path: AbsolutePath) =
    Task.Run(fun () -> new FileStream(path.Value, FileMode.Open, FileAccess.Read) :> Stream)

let system = {
    GetFilesRecursively = getFilesRecursively
    OpenStream = openStream
}
