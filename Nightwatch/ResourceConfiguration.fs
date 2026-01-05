// SPDX-FileCopyrightText: 2017-2018 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.ResourceConfiguration

open System
open System.Collections.Generic
open System.Threading.Tasks
open System.IO

open FSharp.Control.Tasks
open YamlDotNet.Serialization

open Nightwatch.Core.FileSystem
open Nightwatch.Core.Resources
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
        errorPath (sprintf "Resource version %A is not supported" resource.version)
    | _ when String.IsNullOrWhiteSpace resource.id -> errorPath "Resource identifier is not defined"
    | _ when resource.schedule <= TimeSpan.Zero -> error "Resource schedule is invalid"
    | _ when String.IsNullOrWhiteSpace resource.``type`` -> errorPath "Resource type is not defined"
    | valid -> Ok(valid, path)

let private deserializeResource (deserializer : Deserializer) path (reader : StreamReader) =
    let resource = deserializer.Deserialize<ResourceDescription> reader
    correctResource resource path

let private loadFile (fs : FileSystem) deserializer path =
    task {
        use! stream = fs.openStream path
        use reader = new StreamReader(stream)
        return deserializeResource deserializer path reader
    }

let private versionConverter =
    { new IYamlTypeConverter with
        member __.Accepts(t) = t = typeof<Version>
        member __.ReadYaml(parser, _) : obj =
             let scalar = parser.Current :?> YamlDotNet.Core.Events.Scalar
             ignore <| parser.MoveNext()
             let version = scalar.Value
             box <| Version version
        member __.WriteYaml(_, _, _) = failwithf "Not supported" }

let private buildDeserializer() =
    DeserializerBuilder()
        .WithTypeConverter(versionConverter)
        .Build()

let private toResource registry =
    Result.bind (fun (res, path) ->
        match Checker.create registry res.``type`` res.param with
        | Some checker -> Ok { id = res.id; runEvery = res.schedule; checker = checker }
        | None -> Error { path = path
                          id = Some res.id
                          message = sprintf "The resource factory for type \"%s\" is not registered" res.``type`` })

let private configFileMask = Mask "*.yml"

let read (registry : ResourceRegistry)
         (fs : FileSystem)
         (config : ProgramConfiguration)
         : Task<seq<Result<Resource, InvalidConfiguration>>> =
    let deserializer = buildDeserializer()
    task {
        let! fileNames = fs.getFilesRecursively config.resourceDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Task.WhenAll tasks
        return Seq.map (toResource registry) checks
    }
