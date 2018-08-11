module Nightwatch.Tests.ProgramConfiguration

open System.IO
open System.Threading.Tasks

open Xunit

open Nightwatch
open Nightwatch.Core.FileSystem
open Nightwatch.ProgramConfiguration
open Nightwatch.Tests.TestUtils.Environment
open Nightwatch.Tests.TestUtils.FileSystem

let private fullPath = Path.GetFullPath

[<Fact>]
let ``ProgramConfiguration should be read from the YAML file``() : Task =
    let text = sprintf "resource-directory: %s" (fullPath @"C:\Temp")
    let fileSystem = mockFileSystem [| fullPath @"C:\Users\gsomix\mytest\nightwatch.yml", text |]
    let environment = mockEnvironment(fullPath @"C:\Users\gsomix")
    upcast (Async.StartAsTask <| async {
        let! configuration = ProgramConfiguration.read environment fileSystem (Path(Path.Combine("mytest", "nightwatch.yml")))
        let expected = { baseDirectory = Path(fullPath @"C:\Users\gsomix\mytest")
                         resourceDirectory = Path(fullPath @"C:\Temp") }
        Assert.Equal(expected, configuration)
    })

[<Fact>]
let ``ProgramConfiguration's resourcePath should be interpreted from basePath``() : Task =
    let text = "resource-directory: test-relative-path"
    let fileSystem = mockFileSystem [| fullPath "/test/nightwatch.yml", text |]
    let environment = mockEnvironment(fullPath "/test")
    upcast (Async.StartAsTask <| async {
        let! configuration = ProgramConfiguration.read environment fileSystem (Path "nightwatch.yml")
        let expected = { baseDirectory = Path(fullPath "/test")
                         resourceDirectory = Path(fullPath @"/test/test-relative-path") }
        Assert.Equal(expected, configuration)
    })
