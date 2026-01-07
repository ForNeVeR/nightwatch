// SPDX-FileCopyrightText: 2017-2018 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.Scheduler

open System

open Quartz
open Xunit
open FSharp.Control.Tasks

open Nightwatch
open Nightwatch.Core.Resources
open Nightwatch.Resources
open Nightwatch.Notifications
open Nightwatch.CheckState

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

let private trueChecker = ResourceChecker(fun () -> task { return true })
let private emptyProviders: Map<string, NotificationProvider> = Map.empty
let private stateTracker = ResourceStateTracker()

[<Fact>]
let ``Schedule should be prepared`` () =
    let task =
        { id = "1"
          runEvery = TimeSpan.FromMinutes 1.0
          checker = trueChecker
          notificationIds = [||] }
    let schedule = Scheduler.prepareSchedule emptyProviders stateTracker [| task |]
    let (job, trigger) = Seq.exactlyOne schedule
    Assert.Equal (JobKey task.id, job.Key)

[<Fact>]
let ``Scheduler should be configured`` () =
    let taskResource =
        { id = "1"
          runEvery = TimeSpan.FromMinutes 1.0
          checker = trueChecker
          notificationIds = [||] }
    task {
        let! scheduler = Scheduler.create()
        let schedule = Scheduler.prepareSchedule emptyProviders stateTracker [| taskResource |]
        do! Scheduler.configure scheduler schedule
        let! job = scheduler.GetJobDetail (JobKey taskResource.id)
        Assert.NotNull job
    }
