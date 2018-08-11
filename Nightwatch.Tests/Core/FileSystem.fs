module Nightwatch.Tests.Core.FileSystem

open System
open System.IO
open System.Threading.Tasks

open Xunit
open FSharp.Control.Tasks

open Nightwatch.Core
open Nightwatch.Core.FileSystem

let private fs = FileSystem.system

type private MockFileSystem =
    { root : string }
    interface IDisposable with
        member this.Dispose() =
            Directory.Delete(this.root, true)

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
let ``getFilesRecursively should return only the files corresponding to mask`` () =
    let fileList = [| "dir/"; "dir/file.txt"; "dir/file.yml"
                      "dir/subdir/"; "dir/subdir/file.txt"; "dir/subdir/file.yml" |]
    task {
        use dir = createFileSystem fileList
        let! files = fs.getFilesRecursively (Path dir.root) (Mask "*.txt")
        let expected =
            fileList
            |> Seq.filter (fun p -> p.EndsWith ".txt")
            |> Seq.map (fun p -> Path(Path.Combine(dir.root, p.Replace("/", string Path.DirectorySeparatorChar))))
            |> Seq.sort
            |> Seq.toArray
        Assert.Equal<Path>(expected, files |> Seq.sort)
    }

[<Fact>]
let ``openStream returns the file stream`` () =
    task {
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
    }

[<Fact>]
let ``/ operator combines paths``() : unit =
    let p1 = Path "a"
    let p2 = Path "b"
    Assert.Equal(Path(Path.Combine("a", "b")), p1 / p2)

[<Fact>]
let ``Path.parent returns full parent path``() : unit =
    let path = "/Documents and Settings/Vasily"
    let parent = Path.parent(Path path)
    Assert.Equal(Path(Path.GetDirectoryName path), parent)
