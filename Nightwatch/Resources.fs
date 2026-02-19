// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

namespace Nightwatch.Resources

open System
open System.Collections.Generic
open System.Threading.Tasks
open Serilog

open Nightwatch.Core.Resources

/// Description of Resource stored in the configuration file.
[<CLIMutable>]
type ResourceDescription =
    { version : Version
      id : string
      schedule : TimeSpan
      ``type`` : string
      param : Dictionary<string, string>
      notifications : string[] }

type internal Resource = {
      Id: string
      RunEvery: TimeSpan
      Checker: ResourceChecker
      NotificationIds: string[]
}

module ResourceRegistry =
    let Create (factories : ResourceFactory seq) : IReadOnlyDictionary<string, ResourceFactory> =
        upcast (
            factories
            |> Seq.map(fun f -> f.resourceType, f)
            |> Map.ofSeq
        )

module internal Checker =
    let create (registry: IReadOnlyDictionary<string, ResourceFactory>) (resourceType: string) (param: IDictionary<string, string>)
                      : ResourceChecker option =
        match registry.TryGetValue resourceType with
        | false, _ -> None
        | true, factory -> Some <| factory.create.Invoke param

    let check (resource : Resource) : Task<Result<unit, string>> =
        task {
            Log.Information("Checking resource {0}...", resource.Id)
            return! resource.Checker.Invoke()
        }
