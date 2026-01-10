// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.ResourceConfiguration

open System
open System.Threading.Tasks
open System.IO

open YamlDotNet.Serialization

open Nightwatch.Core.FileSystem
open Nightwatch.ProgramConfiguration
open Nightwatch.Resources

let configFormatVersion : Version = Version "0.0.1.0"

type InvalidConfiguration =
    { path : Path
      id : string option
      message : string }

let private correctResource (resource : ResourceDescription) path =
    let errorPath msg = Error { path = path; id = None; message = msg }
    let error msg = Error { path = path; id = Some resource.id; message = msg }

    match resource with
    | _ when resource.version = Version() -> errorPath "Resource version is not defined"
    | _ when resource.version <> configFormatVersion ->
        errorPath $"Resource version %A{resource.version} is not supported"
    | _ when String.IsNullOrWhiteSpace resource.id -> errorPath "Resource identifier is not defined"
    | _ when resource.schedule <= TimeSpan.Zero -> error "Resource schedule is invalid"
    | _ when String.IsNullOrWhiteSpace resource.``type`` -> errorPath "Resource type is not defined"
    | valid -> Ok(valid, path)

let private deserializeResource (deserializer : IDeserializer) path (reader : StreamReader) =
    let resource = deserializer.Deserialize<ResourceDescription> reader
    correctResource resource path

let private loadFile (fs : FileSystem) deserializer path =
    task {
        use! stream = fs.openStream path
        use reader = new StreamReader(stream)
        return deserializeResource deserializer path reader
    }

let private buildDeserializer() =
    DeserializerBuilder()
        .WithTypeConverter(VersionConverter())
        .Build()

let private toResource registry =
    Result.bind (fun (res, path) ->
        match Checker.create registry res.``type`` res.param with
        | Some checker ->
            let notificationIds = if isNull res.notifications then [||] else res.notifications
            Ok { id = res.id; runEvery = res.schedule; checker = checker; notificationIds = notificationIds }
        | None -> Error { path = path
                          id = Some res.id
                          message = $"The resource factory for type \"%s{res.``type``}\" is not registered" })

let private configFileMask = Mask "*.yml"

let read (registry : ResourceRegistry)
         (fs : FileSystem)
         (config : ProgramConfiguration)
         : Task<seq<Result<Resource, InvalidConfiguration>>> =
    let deserializer = buildDeserializer()
    task {
        let! fileNames = fs.getFilesRecursively config.ResourceDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Task.WhenAll tasks
        return Seq.map (toResource registry) checks
    }
