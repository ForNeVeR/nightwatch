// SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Core.Resources

open System
open System.Collections.Generic
open System.Threading.Tasks

type ResourceChecker = Func<Task<Result<unit, string>>>

type ResourceFactory =
    { resourceType : string
      create : Func<IDictionary<string, string>, ResourceChecker> }

module Factory =
    let create (resourceType : string) (create : IDictionary<string, string> -> unit -> Task<Result<unit, string>>) =
        let create = fun param -> ResourceChecker(create param)
        { resourceType = resourceType
          create = Func<_, _> create }
