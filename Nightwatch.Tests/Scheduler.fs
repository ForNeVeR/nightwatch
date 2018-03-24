module Nightwatch.Tests.Scheduler

open System

open Quartz
open Xunit
open FSharp.Control.Tasks

open Nightwatch
open Nightwatch.Core.Resources
open Nightwatch.Resources

[<Fact>]
let ``Scheduler should be created`` () =
    task {
        let! scheduler = Scheduler.create()
        Assert.NotNull scheduler
    }

[<Fact>]
let ``Scheduler should be started`` () =
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.start scheduler
        Assert.True scheduler.IsStarted
    }

[<Fact>]
let ``Scheduler should be stopped`` () =
    task {
        let! scheduler = Scheduler.create()
        do! Scheduler.start scheduler
        do! Scheduler.stop scheduler
        Assert.True scheduler.IsShutdown
    }

let trueChecker = ResourceChecker(fun () -> task { return true })

[<Fact>]
let ``Schedule should be prepared`` () =
    let task =
        { id = "1"
          runEvery = TimeSpan.FromMinutes 1.0
          checker = trueChecker }
    let schedule = Scheduler.prepareSchedule [| task |]
    let (job, trigger) = Seq.exactlyOne schedule
    Assert.Equal (JobKey task.id, job.Key)

[<Fact>]
let ``Scheduler should be configured`` () =
    let taskResource =
        { id = "1"
          runEvery = TimeSpan.FromMinutes 1.0
          checker = trueChecker }
    task {
        let! scheduler = Scheduler.create()
        let schedule = Scheduler.prepareSchedule [| taskResource |]
        do! Scheduler.configure scheduler schedule
        let! job = scheduler.GetJobDetail (JobKey taskResource.id)
        Assert.NotNull job
    }

