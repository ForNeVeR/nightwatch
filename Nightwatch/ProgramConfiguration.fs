// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.ProgramConfiguration

open System
open System.IO

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
    BaseDirectory: Path
    ResourceDirectory: Path
    NotificationDirectory: Path option
    LogFilePath: Path option
}

let read (env : Environment) (fs : FileSystem) (configFilePath : Path) : Async<ProgramConfiguration> =
    let deserializer = Deserializer()
    async {
        let configFilePath = env.currentDirectory / configFilePath
        use! stream = Async.AwaitTask <| fs.openStream configFilePath
        use reader = new StreamReader(stream)
        let config = deserializer.Deserialize<ProgramConfigurationDescription> reader
        let baseDirectory = Path.parent configFilePath
        let relResourceDirectory = Path config.``resource-directory``
        let notificationDirectory =
            if String.IsNullOrWhiteSpace config.``notification-directory``
            then None
            else Some (baseDirectory / Path config.``notification-directory``)
        let logFilePath =
            if String.IsNullOrWhiteSpace config.``log-file``
            then None
            else Some (baseDirectory / Path config.``log-file``)
        return { BaseDirectory = baseDirectory
                 ResourceDirectory = baseDirectory / relResourceDirectory
                 NotificationDirectory = notificationDirectory
                 LogFilePath = logFilePath }
    }
