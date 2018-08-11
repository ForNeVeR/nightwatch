module Nightwatch.ProgramConfiguration

open System.IO

open YamlDotNet.Serialization

open Nightwatch.Core.Environment
open Nightwatch.Core.FileSystem

[<CLIMutable>]
type ProgramConfigurationDescription =
    { ``resource-directory`` : string }

type ProgramConfiguration =
    { baseDirectory : Path
      resourceDirectory : Path }

let read (env : Environment) (fs : FileSystem) (configFilePath : Path) : Async<ProgramConfiguration> =
    let deserializer = Deserializer()
    async {
        let configFilePath = env.currentDirectory / configFilePath
        use! stream = Async.AwaitTask <| fs.openStream configFilePath
        use reader = new StreamReader(stream)
        let config = deserializer.Deserialize<ProgramConfigurationDescription> reader
        let baseDirectory = Path.parent configFilePath
        let relResourceDirectory = Path config.``resource-directory``
        return { baseDirectory = baseDirectory
                 resourceDirectory = baseDirectory / relResourceDirectory }
    }
