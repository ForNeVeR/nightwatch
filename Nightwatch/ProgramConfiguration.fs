// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.ProgramConfiguration

open System
open System.IO

open TruePath
open YamlDotNet.Serialization

open Nightwatch.Core.Environment
open Nightwatch.Core.FileSystem

[<CLIMutable>]
type ProgramConfigurationDescription = {
    ``resource-directory``: string
    ``notification-directory``: string
    ``log-file``: string
}

type ProgramConfiguration = {
    BaseDirectory: AbsolutePath
    ResourceDirectory: AbsolutePath
    NotificationDirectory: AbsolutePath option
    LogFilePath: AbsolutePath option
}

let read (env: Environment) (fs: FileSystem) (configFilePath: LocalPath) : Async<ProgramConfiguration> =
    let deserializer = Deserializer()
    async {
        let configFilePath = env.CurrentDirectory / configFilePath
        use! stream = Async.AwaitTask <| fs.OpenStream configFilePath
        use reader = new StreamReader(stream)
        let config = deserializer.Deserialize<ProgramConfigurationDescription> reader
        let baseDirectory = configFilePath.Parent |> nonNullV
        let relResourceDirectory = LocalPath config.``resource-directory``
        let notificationDirectory =
            if String.IsNullOrWhiteSpace config.``notification-directory``
            then None
            else Some (baseDirectory / config.``notification-directory``)
        let logFilePath =
            if String.IsNullOrWhiteSpace config.``log-file``
            then None
            else Some (baseDirectory / config.``log-file``)
        return {
            BaseDirectory = baseDirectory
            ResourceDirectory = baseDirectory / relResourceDirectory
            NotificationDirectory = notificationDirectory
            LogFilePath = logFilePath
        }
    }
