module Nightwatch.Core.Resources

open System
open System.Collections.Generic
open System.Threading.Tasks

type ResourceChecker = Func<Task<bool>>

type ResourceFactory =
    { resourceType : string
      create : Func<IDictionary<string, string>, ResourceChecker> }

module Factory =
    let create (resourceType : string) (create : IDictionary<string, string> -> unit -> Task<bool>) =
        let create = fun param -> ResourceChecker(create param)
        { resourceType = resourceType
          create = Func<_, _> create }
