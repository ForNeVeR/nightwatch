module Nightwatch.Core.Process

open System
open System.Diagnostics
open System.Threading.Tasks

type Controller =
    { execute : string -> string[] -> Task<int> }

let private execute command (args : string[]) =
    let args = String.Join(" ", args)
    Task.Run(fun () ->
        use proc = Process.Start(command, args)
        proc.WaitForExit()
        proc.ExitCode)

let system : Controller =
    { execute = execute }
