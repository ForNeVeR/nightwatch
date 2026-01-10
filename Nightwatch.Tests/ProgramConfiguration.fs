// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.ProgramConfiguration

open System.IO
open System.Threading.Tasks

open Xunit

open Nightwatch.Core.FileSystem
open Nightwatch.ProgramConfiguration
open Nightwatch.Tests.TestUtils.Environment
open Nightwatch.Tests.TestUtils.FileSystem

let private fullPath = Path.GetFullPath

[<Fact>]
let ``ProgramConfiguration should be read from the YAML file``() : Task =
    let text = sprintf "resource-directory: %s" (fullPath "/Temp")
    let fileSystem = mockFileSystem [| fullPath "/Users/gsomix/mytest/nightwatch.yml", text |]
    let environment = mockEnvironment(fullPath "/Users/gsomix")
    upcast (Async.StartAsTask <| async {
        let! configuration = read environment fileSystem (Path(Path.Combine("mytest", "nightwatch.yml")))
        let expected = {
            BaseDirectory = Path(fullPath "/Users/gsomix/mytest")
            ResourceDirectory = Path(fullPath "/Temp")
            NotificationDirectory = None
            LogFilePath = None
        }
        Assert.Equal(expected, configuration)
    })

[<Fact>]
let ``ProgramConfiguration's resourcePath should be interpreted from basePath``() : Task =
    let text = "resource-directory: test-relative-path"
    let fileSystem = mockFileSystem [| fullPath "/test/nightwatch.yml", text |]
    let environment = mockEnvironment(fullPath "/test")
    upcast (Async.StartAsTask <| async {
        let! configuration = read environment fileSystem (Path "nightwatch.yml")
        let expected = {
            BaseDirectory = Path(fullPath "/test")
            ResourceDirectory = Path(fullPath "/test/test-relative-path")
            NotificationDirectory = None
            LogFilePath = None
        }
        Assert.Equal(expected, configuration)
    })
