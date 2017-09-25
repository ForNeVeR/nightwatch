module Nightwatch.Configuration

open System
open System.IO

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

open Nightwatch.FileSystem
open Nightwatch.Resources

[<CLIMutable>]
type ResourceDescription =
    { version : Version
      id : string
      schedule : TimeSpan
      check : string }

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
    | _ when String.IsNullOrWhiteSpace resource.check -> error "Resource check is not defined"
    | valid -> Ok valid

let private deserializeResource (deserializer : Deserializer) path (reader : StreamReader) =
    let resource = deserializer.Deserialize<ResourceDescription> reader
    correctResource resource path

let private loadFile (fs : FileSystem) deserializer path =
    async {
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

let private toResource : Result<ResourceDescription, _> -> _ =
    Result.map (fun res -> { id = res.id; runEvery = res.schedule; checkCommand = res.check })

let private configFileMask = Mask "*.yml"

let read (fs : FileSystem) (configDirectory : Path) : Async<seq<Result<Resource, InvalidConfiguration>>> =
    let deserializer = buildDeserializer()
    async {
        let! fileNames = fs.getFilesRecursively configDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Async.Parallel tasks
        return Seq.map toResource checks
    }
