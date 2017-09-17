module Nightwatch.Scheduler

open System.Threading.Tasks

open Quartz
open Quartz.Impl

open Nightwatch

type Schedule = (IJobDetail * ITrigger) seq

let prepareSchedule : CheckConfiguration seq -> Schedule =
    Seq.map (fun { id = id; runEvery = runEvery; checkFunction = checkFunction } ->
        let trigger =
            TriggerBuilder.Create()
                .WithIdentity(id)
                .WithSimpleSchedule(fun x -> ignore <| x.WithInterval(runEvery).RepeatForever())
                .Build()
        let jobData = Map.ofArray [| CheckerJob.CheckFunction, box checkFunction |]
        let job =
            JobBuilder.CreateForAsync<CheckerJob>()
                .WithIdentity(JobKey id)
                .UsingJobData(JobDataMap jobData)
                .Build()
        job, trigger
    )


let create() : Async<IScheduler> =
    async {
        let factory = StdSchedulerFactory()
        return! factory.GetScheduler()
    }

let private configureTask (scheduler : IScheduler) (job : IJobDetail, trigger) : Task =
    upcast scheduler.ScheduleJob(job, trigger)

let configure (schedule : Schedule) (scheduler: IScheduler) : Async<unit> =
    async {
        let tasks =
            schedule
            |> Seq.map (configureTask scheduler)
            |> Seq.toArray
        do! Task.WhenAll tasks
    }

let start (scheduler : IScheduler) : Async<unit> =
    async {
        do! scheduler.Start()
    }

let stop (scheduler : IScheduler) : Async<unit> =
    async {
        do! scheduler.Shutdown true
    }
