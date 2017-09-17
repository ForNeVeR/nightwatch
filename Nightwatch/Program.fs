﻿module Nightwatch.Program

open System
open System.Reflection

let private printVersion() =
    let version = Assembly.GetEntryAssembly().GetName().Version
    printfn "Nightwatch v. %A" version

let private runScheduler() =
    let healthCheck =
        { id = "health-check"
          runEvery = TimeSpan.FromSeconds 25.0
          checkFunction = fun () -> async { printfn "Health check succeed"; return true } }
    let schedule = Scheduler.prepareSchedule [| healthCheck |]

    async {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure schedule scheduler
        do! Scheduler.start scheduler
        do! Async.Ignore <| Async.AwaitEvent(Console.CancelKeyPress)
    } |> Async.RunSynchronously

[<EntryPoint>]
let main argv =
    printVersion()
    runScheduler()
    0
