module Nightwatch.Resources.Shell

open System
open System.Collections.Generic
open System.Diagnostics

open Nightwatch.Core.Resources

let create(param : IDictionary<string, string>) =
    let command = param.["cmd"]
    fun () -> async {
        do! Async.SwitchToThreadPool()
        let proc = Process.Start command
        proc.WaitForExit()
        return proc.ExitCode = 0
    }

let factory = fSharpFactory "shell" create
