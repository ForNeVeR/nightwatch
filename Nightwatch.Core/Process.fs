namespace Nightwatch.Core

open System
open System.Diagnostics
open System.Threading.Tasks

type ProcessController =
    { execute : string -> string[] -> Task<int> }

module Process =
    let system : ProcessController =
        { execute = fun (command) (args) ->
            let args = String.Join(" ", args)
            Task.Run(fun () ->
                use proc = Process.Start(command, args)
                proc.WaitForExit()
                proc.ExitCode) }
