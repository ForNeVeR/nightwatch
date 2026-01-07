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
type ProgramConfigurationDescription =
    { ``resource-directory`` : string
      ``notification-directory`` : string }

type ProgramConfiguration =
    { baseDirectory : Path
      resourceDirectory : Path
      notificationDirectory : Path option }

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
        return { baseDirectory = baseDirectory
                 resourceDirectory = baseDirectory / relResourceDirectory
                 notificationDirectory = notificationDirectory }
    }
