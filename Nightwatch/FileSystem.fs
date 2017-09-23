module Nightwatch.FileSystem

open System.IO

type Path = Path of string
type Mask = Mask of string

type FileSystem =
    { getFilesRecursively : Path -> Mask -> Async<Path seq>
      openStream : Path -> Async<Stream> }

let system =
    { getFilesRecursively = failwithf "Not implemented"
      openStream = failwithf "Not implemented" }
