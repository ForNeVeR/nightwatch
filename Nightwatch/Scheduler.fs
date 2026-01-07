// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.Scheduler

open System.Threading.Tasks

open Quartz
open Quartz.Impl
open FSharp.Control.Tasks

open Nightwatch.Resources
open Nightwatch.Notifications
open Nightwatch.CheckState

type Schedule = (IJobDetail * ITrigger) seq

let prepareSchedule (providers: Map<string, NotificationProvider>)
                    (stateTracker: ResourceStateTracker)
                    (resources: Resource seq)
                    : Schedule =
    Seq.map (fun resource ->
        let { id = id; runEvery = runEvery } = resource
        let trigger =
            TriggerBuilder.Create()
                .WithIdentity(id)
                .WithSimpleSchedule(fun x -> ignore <| x.WithInterval(runEvery).RepeatForever())
                .Build()
        let jobData = Map.ofArray [|
            CheckerJob.Resource, box resource
            CheckerJob.NotificationProviders, box providers
            CheckerJob.StateTracker, box stateTracker
        |]
        let job =
            JobBuilder.Create<CheckerJob>()
                .WithIdentity(JobKey id)
                .UsingJobData(JobDataMap jobData)
                .Build()
        job, trigger
    ) resources


let create() : Task<IScheduler> =
    task {
        let factory = StdSchedulerFactory()
        return! factory.GetScheduler()
    }

let private configureTask (scheduler : IScheduler) (job : IJobDetail, trigger) : Task =
    upcast scheduler.ScheduleJob(job, trigger)

let configure (scheduler: IScheduler) (schedule : Schedule) : Task<unit> =
    task {
        let tasks =
            schedule
            |> Seq.map (configureTask scheduler)
            |> Seq.toArray
        do! Task.WhenAll tasks
    }

let start (scheduler : IScheduler) : Async<unit> =
    async {
        do! Async.AwaitTask(scheduler.Start())
    }

let stop (scheduler : IScheduler) : Async<unit> =
    async {
        do! Async.AwaitTask(scheduler.Shutdown true)
    }
