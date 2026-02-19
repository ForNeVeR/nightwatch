// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.ResourceConfiguration

open System
open System.Collections.Generic
open System.Threading.Tasks
open System.IO

open Nightwatch.Core.Resources
open TruePath
open YamlDotNet.Serialization

open Nightwatch.Core.FileSystem
open Nightwatch.ProgramConfiguration
open Nightwatch.Resources

let configFormatVersion : Version = Version "0.1.0.0"

type InvalidConfiguration = {
    Path: AbsolutePath
    Id: string option
    Message: string
}

let private correctResource (resource : ResourceDescription) path =
    let errorPath msg = Error { Path = path; Id = None; Message = msg }
    let error msg = Error { Path = path; Id = Some resource.id; Message = msg }

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
        use! stream = fs.OpenStream path
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
            Ok { Id = res.id; RunEvery = res.schedule; Checker = checker; NotificationIds = notificationIds }
        | None -> Error { Path = path
                          Id = Some res.id
                          Message = $"The resource factory for type \"%s{res.``type``}\" is not registered" })

let private configFileMask = LocalPathPattern "*.yml"

let read (registry: IReadOnlyDictionary<string, ResourceFactory>)
         (fs: FileSystem)
         (config: ProgramConfiguration)
         : Task<seq<Result<Resource, InvalidConfiguration>>> =
    let deserializer = buildDeserializer()
    task {
        let! fileNames = fs.GetFilesRecursively config.ResourceDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Task.WhenAll tasks
        return Seq.map (toResource registry) checks
    }
