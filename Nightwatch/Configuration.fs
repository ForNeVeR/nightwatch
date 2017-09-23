module Nightwatch.Configuration

open System
open System.IO

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

open Nightwatch.FileSystem
open Nightwatch.Resources

let private deserializeResource (deserializer : Deserializer) (reader : StreamReader) =
    deserializer.Deserialize<Resource> reader

let private loadFile (fs : FileSystem) deserializer path =
    async {
        use! stream = fs.openStream path
        use reader = new StreamReader(stream)
        return deserializeResource deserializer reader
    }

let private buildDeserializer() =
    DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention())
        .Build()

let private configFileMask = Mask "*.yml"

let read (fs : FileSystem) (configDirectory : Path) : Async<Resource seq> =
    let deserializer = buildDeserializer()
    async {
        let! fileNames = fs.getFilesRecursively configDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Async.Parallel tasks
        return upcast checks
    }
