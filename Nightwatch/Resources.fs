module internal Nightwatch.Resources

open System
open System.Collections.Generic

open Nightwatch.Core.Resources
open Nightwatch.FileSystem

[<CLIMutable>]
type ResourceDescription =
    { version : Version
      id : string
      schedule : TimeSpan
      ``type`` : string
      param : Map<string, string> }

type ResourceRegistry = Map<string, ResourceFactory>

let createRegistry (factories : ResourceFactory seq) : ResourceRegistry =
    factories
    |> Seq.map (fun f -> f.resourceType, f)
    |> Map.ofSeq

let createResourceChecker (registry : ResourceRegistry) (resourceType : string) (param : IDictionary<string, string>)
                          : ResourceChecker option =
    Map.tryFind resourceType registry
    |> Option.map (fun factory -> factory.create.Invoke param)

type Resource =
    { id : string
      runEvery : TimeSpan
      checker : ResourceChecker }

let check (resource : Resource) : Async<bool> =
    async {
        printfn "Checking resource %s…" resource.id
        return! resource.checker.Invoke()
    }
