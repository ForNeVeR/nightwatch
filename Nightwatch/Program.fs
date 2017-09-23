module Nightwatch.Program

open System
open System.Reflection

open Nightwatch.Resources

let private printVersion() =
    let version = Assembly.GetEntryAssembly().GetName().Version
    printfn "Nightwatch v. %A" version

let private runScheduler() =
    let healthCheck =
        { id = "health-check"
          runEvery = TimeSpan.FromSeconds 25.0
          checkCommand = "Health check" }
    let schedule = Scheduler.prepareSchedule [| healthCheck |]

    async {
        let! scheduler = Scheduler.create()
        do! Scheduler.configure scheduler schedule
        do! Scheduler.start scheduler
        printfn "Scheduler started"
        printfn "Press any key to stop"
        ignore <| Console.ReadKey()
        printfn "Stopping"
        do! Scheduler.stop scheduler
        printfn "Bye"
    } |> Async.RunSynchronously

[<EntryPoint>]
let main argv =
    printVersion()
    runScheduler()
    0
