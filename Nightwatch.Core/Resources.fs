module Nightwatch.Core.Resources

open System

type ResourceChecker = unit -> Async<bool>

type ResourceFactory =
    { resourceType : string
      create : Map<string, string> -> ResourceChecker }

type Resource =
    { id : string
      runEvery : TimeSpan
      checker : ResourceChecker }

let check (resource : Resource) : Async<bool> =
    printfn "Checking resource %s…" resource.id
    resource.checker()
