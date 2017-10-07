module Nightwatch.Core.FileSystem

open System.IO
open System.Threading.Tasks

type Path = Path of string
type Mask = Mask of string

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
