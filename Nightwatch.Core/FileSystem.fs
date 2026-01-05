// SPDX-FileCopyrightText: 2017-2018 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Core.FileSystem

open System
open System.IO
open System.Threading.Tasks

type Path = Path of string
    with
        static member parent(Path path) : Path = Path(Path.GetDirectoryName path)
type Mask = Mask of string

let (/) (Path p1) (Path p2) : Path = Path(Path.Combine(p1, p2))

type FileSystem =
    { getFilesRecursively : Path -> Mask -> Task<Path seq>
      openStream : Path -> Task<Stream> }

let private getFilesRecursively (Path path) (Mask mask) =
    Task.Run(fun () ->
        Directory.GetFileSystemEntries(path, mask, SearchOption.AllDirectories)
        |> Seq.map Path)

let private openStream (Path path) =
    Task.Run(fun () -> new FileStream(path, FileMode.Open, FileAccess.Read) :> Stream)

let system =
    { getFilesRecursively = getFilesRecursively
      openStream = openStream }
