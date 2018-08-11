module Nightwatch.Tests.TestUtils.Environment

open Nightwatch.Core
open Nightwatch.Core.Environment
open Nightwatch.Core.FileSystem

let mockEnvironment(currentDirectory : string) : Environment =
    Environment.fixedEnvironment(Path currentDirectory)
