module Nightwatch.Configuration

open System
open System.IO

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

open Nightwatch.FileSystem
open Nightwatch.Resources

type ResourceDescription() =
    member val Version = Version("0.0.0.0") with get, set
    member val Id = "" with get, set
    member val Schedule = TimeSpan.Zero with get, set
    member val Check = "" with get, set

let private configFormatVersion = Version "0.0.1.0"

type InvalidConfiguration =
    { path : Path
      id : string option
      message : string }

let private correctResource (resource : ResourceDescription) path =
    let errorPath msg = Choice2Of2 { path = path; id = None; message = msg }
    let error msg = Choice2Of2 { path = path; id = Some resource.Id; message = msg }

    match resource with
    | _ when resource.Version = Version "0.0.0.0" -> errorPath "Resource version is not defined"
    | _ when resource.Version <> configFormatVersion ->
        errorPath (sprintf "Resource version %A is not supported" resource.Version)
    | _ when String.IsNullOrWhiteSpace resource.Id -> errorPath "Resource identifier is not defined"
    | _ when resource.Schedule <= TimeSpan.Zero -> error "Resource schedule is invalid"
    | _ when String.IsNullOrWhiteSpace resource.Check -> error "Resource check is not defined"
    | valid -> Choice1Of2 valid

let private deserializeResource (deserializer : Deserializer) path (reader : StreamReader) =
    let resource = deserializer.Deserialize<ResourceDescription> reader
    correctResource resource path

let private loadFile (fs : FileSystem) deserializer path =
    async {
        use! stream = fs.openStream path
        use reader = new StreamReader(stream)
        return deserializeResource deserializer path reader
    }

let versionConverter =
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
        .WithNamingConvention(CamelCaseNamingConvention())
        .WithTypeConverter(versionConverter)
        .Build()

let private toResource = function
| (Choice1Of2 (resource : ResourceDescription)) ->
    Choice1Of2 { id = resource.Id; runEvery = resource.Schedule; checkCommand = resource.Check }
| (Choice2Of2 error) -> Choice2Of2 error

let private configFileMask = Mask "*.yml"

let read (fs : FileSystem) (configDirectory : Path) : Async<seq<Choice<Resource, InvalidConfiguration>>> =
    let deserializer = buildDeserializer()
    async {
        let! fileNames = fs.getFilesRecursively configDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Async.Parallel tasks
        return Seq.map toResource checks
    }
