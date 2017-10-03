module Nightwatch.Resources.Shell

open System.Diagnostics

open Nightwatch.Core.Resources

let create(param : Map<string, string>) =
    let command = Map.find "cmd" param
    fun () -> async {
        do! Async.SwitchToThreadPool()
        let proc = Process.Start command
        proc.WaitForExit()
        return proc.ExitCode = 0
    }

let factory =
    { resourceType = "shell"
      create = create }
