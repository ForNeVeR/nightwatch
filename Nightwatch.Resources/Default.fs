module Nightwatch.Resources.Default

open Nightwatch.Core.Resources

let factories : ResourceFactory[] =
    [| Http.factory
       Shell.factory |]
