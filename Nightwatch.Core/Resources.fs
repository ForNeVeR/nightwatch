module Nightwatch.Core.Resources

open System
open System.Collections.Generic
open System.Threading.Tasks

type ResourceChecker = Func<Async<bool>> // TODO: Replace with Task

type ResourceFactory =
    { resourceType : string
      create : Func<IDictionary<string, string>, ResourceChecker> }

let fSharpFactory (resourceType : string) (create : IDictionary<string, string> -> unit -> Async<bool>) =
    let create = fun param -> Func<_>(create param)
    { resourceType = resourceType
      create = Func<_, _> create }
