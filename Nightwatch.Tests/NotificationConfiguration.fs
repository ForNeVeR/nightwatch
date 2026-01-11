// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.NotificationConfiguration

open System
open System.Threading.Tasks

open TruePath
open Xunit

open Nightwatch.Core.Notifications
open Nightwatch
open Nightwatch.Notifications
open Nightwatch.Tests.TestUtils.FileSystem

[<Fact>]
let ``NotificationConfiguration should read the YAML file``(): Task =
    let text = @"version: 0.0.1.0
id: test-notification
type: test
param:
    token: abc123"
    let sender = NotificationSender(fun _ -> Task.CompletedTask)
    let mutable parsedParam = None
    let factory: NotificationFactory =
        { NotificationType = "test"
          Create = Func<_, _>(fun param -> parsedParam <- Some param; sender) }
    let registry = NotificationRegistry.Create [| factory |]
    let fileSystem = mockFileSystem [| "notifications/test.yml", text |]
    task {
        let path = MockedRoot / LocalPath "notifications"
        let! result = NotificationConfiguration.read registry fileSystem path
        let results = result |> Seq.toArray
        match Assert.Single(results) with
        | Ok provider -> Assert.Equal("test-notification", provider.id)
        | Error e -> Assert.True(false, $"Expected Ok but got Error: %s{e.Message}")

        let param = Option.get parsedParam
        Assert.Equal("abc123", param["token"])
    }

let private emptyRegistry = NotificationRegistry.Create [| |]

[<Fact>]
let ``NotificationConfiguration returns error if type is not registered``(): Task =
    let text = @"version: 0.0.1.0
id: test
type: unknown"
    let path = "notifications/test.yml"
    let fileSystem = mockFileSystem [| path, text |]
    let expected: Result<NotificationProvider, NotificationConfiguration.NotificationConfigurationError> =
        Error { NotificationConfiguration.NotificationConfigurationError.Path = MockedRoot / path
                NotificationConfiguration.NotificationConfigurationError.Id = Some "test"
                NotificationConfiguration.NotificationConfigurationError.Message = "The notification factory for type \"unknown\" is not registered" }
    task {
        let! result = NotificationConfiguration.read emptyRegistry fileSystem (MockedRoot / "notifications")
        Assert.Equal<Result<NotificationProvider, NotificationConfiguration.NotificationConfigurationError>>([| expected |], result |> Seq.toArray)
    }

[<Fact>]
let ``NotificationConfiguration should ignore non-YAML file``(): Task =
    let fileSystem = mockFileSystem [| "notifications/test.yml2", "" |]
    task {
        let! result = NotificationConfiguration.read emptyRegistry fileSystem (MockedRoot / "notifications")
        Assert.Empty(result)
    }

[<Fact>]
let ``NotificationConfiguration returns error if version is missing``(): Task =
    let text = @"id: test
type: test"
    let path = "notifications/test.yml"
    let fileSystem = mockFileSystem [| path, text |]
    task {
        let! result = NotificationConfiguration.read emptyRegistry fileSystem (MockedRoot / "notifications")
        let results = result |> Seq.toArray
        Assert.Single(results) |> ignore
        match results[0] with
        | Error e -> Assert.Contains("version", e.Message)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")
    }

[<Fact>]
let ``NotificationConfiguration returns error if id is missing``(): Task =
    let text = @"version: 0.0.1.0
type: test"
    let path = "notifications/test.yml"
    let fileSystem = mockFileSystem [| path, text |]
    task {
        let! result = NotificationConfiguration.read emptyRegistry fileSystem (MockedRoot / "notifications")
        let results = result |> Seq.toArray
        Assert.Single(results) |> ignore
        match results[0] with
        | Error e -> Assert.Equal("Notification identifier is not defined", e.Message)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")
    }
