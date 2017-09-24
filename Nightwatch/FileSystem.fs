module Nightwatch.FileSystem

open System.IO

type Path = Path of string
type Mask = Mask of string

type FileSystem =
    { getFilesRecursively : Path -> Mask -> Async<Path seq>
      openStream : Path -> Async<Stream> }

let private getFilesRecursively (Path path) (Mask mask) =
    async {
        return Directory.GetFileSystemEntries(path, mask, SearchOption.AllDirectories)
            |> Seq.map Path
    }

let private openStream (Path path) =
    async {
        return new FileStream(path, FileMode.Open, FileAccess.Read) :> Stream
    }

let system =
    { getFilesRecursively = getFilesRecursively
      openStream = openStream }
