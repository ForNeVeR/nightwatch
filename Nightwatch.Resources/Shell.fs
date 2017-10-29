module Nightwatch.Resources.Shell

open System
open System.Collections.Generic

open FSharp.Control.Tasks

open Nightwatch.Core
open Nightwatch.Core.Resources

let private create (processController : Process.Controller)
                   (param : IDictionary<string, string>) =
    let commandParams = param.["cmd"].Split(' ')
    let len = commandParams.Length
    let command = commandParams.[0]
    let args = Array.sub commandParams 1 (len - 1)
    fun () -> task {
        let! code = processController.execute command args
        return code = 0
    }

let factory(processController : Process.Controller) : ResourceFactory =
    Factory.create "shell" (create processController)
