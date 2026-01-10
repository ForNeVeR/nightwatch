// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.ProgramConfiguration

open System.Threading.Tasks

open TruePath
open Xunit

open Nightwatch.ProgramConfiguration
open Nightwatch.Tests.TestUtils.Environment
open Nightwatch.Tests.TestUtils.FileSystem

[<Fact>]
let ``ProgramConfiguration should be read from the YAML file``() : Task =
    let text = sprintf "resource-directory: %s" (MockedRoot / "/Temp").Value
    let fileSystem = mockFileSystem [| "/Users/gsomix/mytest/nightwatch.yml", text |]
    let environment = mockEnvironment(MockedRoot / "/Users/gsomix")
    upcast (Async.StartAsTask <| async {
        let! configuration = read environment fileSystem (LocalPath "mytest" / "nightwatch.yml")
        let expected = {
            BaseDirectory = MockedRoot / "/Users/gsomix/mytest"
            ResourceDirectory = MockedRoot / "/Temp"
            NotificationDirectory = None
            LogFilePath = None
        }
        Assert.Equal(expected, configuration)
    })

[<Fact>]
let ``ProgramConfiguration's resourcePath should be interpreted from basePath``() : Task =
    let text = "resource-directory: test-relative-path"
    let fileSystem = mockFileSystem [| "/test/nightwatch.yml", text |]
    let environment = mockEnvironment(MockedRoot / "/test")
    upcast (Async.StartAsTask <| async {
        let! configuration = read environment fileSystem (LocalPath "nightwatch.yml")
        let expected = {
            BaseDirectory = MockedRoot / "/test"
            ResourceDirectory = MockedRoot / "/test/test-relative-path"
            NotificationDirectory = None
            LogFilePath = None
        }
        Assert.Equal(expected, configuration)
    })
