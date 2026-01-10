// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.Core.FileSystem

open System
open System.IO
open System.Threading.Tasks
open TruePath
open Xunit

open Nightwatch.Core.FileSystem

let private fs = system

type private MockFileSystem =
    { root : string }
    interface IDisposable with
        member this.Dispose() =
            Directory.Delete(this.root, true)

    member this.RootPath = AbsolutePath this.root

let rec private createFileSystemEntry (path : string) =
    if path.EndsWith "/" || path.EndsWith(Path.DirectorySeparatorChar)
    then ignore <| Directory.CreateDirectory path
    else
        let parent = Path.GetDirectoryName(path) + "/"
        if not (Directory.Exists parent)
        then createFileSystemEntry parent
        use x = File.Create(path)
        ()

let private createFileSystem paths =
    let root = Path.GetTempFileName()
    File.Delete root
    ignore <| Directory.CreateDirectory root
    paths |> Seq.map (fun p -> Path.Combine(root, p)) |> Seq.iter createFileSystemEntry
    { root = root }

[<Fact>]
let ``GetFilesRecursively should return only the files corresponding to mask``(): Task =
    let fileList = [| "dir/"; "dir/file.txt"; "dir/file.yml"
                      "dir/subdir/"; "dir/subdir/file.txt"; "dir/subdir/file.yml" |]
    task {
        use dir = createFileSystem fileList
        let! files = fs.GetFilesRecursively dir.RootPath (LocalPathPattern "*.txt")
        let expected =
            fileList
            |> Seq.filter (fun p -> p.EndsWith ".txt")
            |> Seq.map (fun p -> dir.RootPath / p)
            |> Seq.sortBy _.Value
            |> Seq.toArray
        Assert.Equal<AbsolutePath>(expected, files |> Seq.sortBy _.Value)
    }

[<Fact>]
let ``OpenStream returns the file stream``(): Task =
    task {
        let path = Path.GetTempFileName()
        try
            let content = "Hello World\n123"
            do! Async.AwaitTask(File.WriteAllTextAsync(path, content))

            use! stream = fs.OpenStream (AbsolutePath path)
            use reader = new StreamReader(stream)
            let! result = Async.AwaitTask <| reader.ReadToEndAsync()
            Assert.Equal(content, result)
        finally
            File.Delete path
    }
