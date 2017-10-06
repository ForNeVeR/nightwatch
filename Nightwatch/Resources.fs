namespace Nightwatch.Resources

open System
open System.Collections.Generic

open Nightwatch.Core.Resources
open Nightwatch.FileSystem

/// Description of Resource stored in the configuration file.
[<CLIMutable>]
type ResourceDescription =
    { version : Version
      id : string
      schedule : TimeSpan
      ``type`` : string
      param : Dictionary<string, string> }

type internal ResourceRegistry = Map<string, ResourceFactory>

type internal Resource =
    { id : string
      runEvery : TimeSpan
      checker : ResourceChecker }

module internal Registry =
    let create (factories : ResourceFactory seq) : ResourceRegistry =
        factories
        |> Seq.map (fun f -> f.resourceType, f)
        |> Map.ofSeq

module internal Checker =
    let create (registry : ResourceRegistry) (resourceType : string) (param : IDictionary<string, string>)
                      : ResourceChecker option =
        Map.tryFind resourceType registry
        |> Option.map (fun factory -> factory.create.Invoke param)

    let check (resource : Resource) : Async<bool> =
        async {
            printfn "Checking resource %s…" resource.id
            return! resource.checker.Invoke()
        }
