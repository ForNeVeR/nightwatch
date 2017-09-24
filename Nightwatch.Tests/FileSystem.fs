module Nightwatch.Tests.FileSystem

open System
open System.IO

open Xunit

open Nightwatch
open Nightwatch.FileSystem

let private fs = FileSystem.system

type private MockFileSystem =
    { root : string }
    interface IDisposable with
        member this.Dispose() =
            Directory.Delete(this.root, true)

let private createFileSystemEntry (path : string) =
    if path.EndsWith "/"
    then ignore <| Directory.CreateDirectory path
    else
        use x = File.Create(path)
        ()

let private createFileSystem paths =
    let root = Path.GetTempFileName()
    File.Delete root
    ignore <| Directory.CreateDirectory root
    paths |> Seq.map (fun p -> Path.Combine(root, p)) |> Seq.iter createFileSystemEntry
    { root = root }

let ``getFilesRecursively should return only the files corresponding to mask`` () =
    let fileList = [| "dir/"; "dir/file.txt"; "dir/file.yml"
                      "dir/subdir"; "dir/subdir/file.txt"; "dir/subdir/file.yml" |]
    use dir = createFileSystem fileList
    async {
        let! files = fs.getFilesRecursively (Path dir.root) (Mask "*.txt")
        let expected =
            fileList
            |> Seq.filter (fun p -> p.EndsWith ".txt")
            |> Seq.map Path
            |> Seq.sort
            |> Seq.toArray
        Assert.Equal<Path>(expected, files)
    } |> Async.StartAsTask

let ``openStream returns the file stream`` () =
    async {
        let path = Path.GetTempFileName()
        try
            let content = "Hello World\n123"
            do! Async.AwaitTask(File.WriteAllTextAsync(path, content))

            use! stream = fs.openStream (Path path)
            use reader = new StreamReader(stream)
            let! result = Async.AwaitTask <| reader.ReadToEndAsync()
            Assert.Equal(content, result)
        finally
            File.Delete path
    } |> Async.StartAsTask
