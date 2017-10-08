namespace Nightwatch.Core

open System
open System.Diagnostics
open System.Threading.Tasks

open FSharp.Control.Tasks

type ProcessController =
    { execute : string -> string[] -> Task<int> }

module Process =
    let system : ProcessController =
        { execute = fun (command) (args) ->
            let args = String.Join(" ", args)
            task {
                use proc = Process.Start(command, args)
                proc.WaitForExit()
                return proc.ExitCode
            } }
