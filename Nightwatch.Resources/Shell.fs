module Nightwatch.Resources.Shell

open System
open System.Collections.Generic
open System.Diagnostics

open FSharp.Control.Tasks

open Nightwatch.Core.Resources

let private create(param : IDictionary<string, string>) =
    let command = param.["cmd"]
    fun () -> task {
        let proc = Process.Start command
        proc.WaitForExit()
        return proc.ExitCode = 0
    }

let factory : ResourceFactory = Factory.create "shell" create
