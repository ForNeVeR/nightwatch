module Nightwatch.Core.Environment

open System

open Nightwatch.Core.FileSystem

type Environment =
    { currentDirectory : Path }

let fixedEnvironment(currentDirectory : Path) : Environment =
    { currentDirectory = currentDirectory }
