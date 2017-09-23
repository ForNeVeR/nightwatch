module Nightwatch.Resources

open System

type Resource =
    { id : string
      runEvery : TimeSpan
      checkCommand : string }

let check (resource : Resource) : Async<bool> =
    printfn "Checking resource %s: %s" resource.id resource.checkCommand
    async { return true }
