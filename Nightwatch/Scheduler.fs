module Nightwatch.Scheduler

open System.Threading.Tasks

open Quartz
open Quartz.Impl

open Nightwatch.Resources

type Schedule = (IJobDetail * ITrigger) seq

let prepareSchedule : Resource seq -> Schedule =
    Seq.map (fun resource ->
        let { id = id; runEvery = runEvery } = resource
        let trigger =
            TriggerBuilder.Create()
                .WithIdentity(id)
                .WithSimpleSchedule(fun x -> ignore <| x.WithInterval(runEvery).RepeatForever())
                .Build()
        let jobData = Map.ofArray [| CheckerJob.Resource, box resource |]
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

let configure (scheduler: IScheduler) (schedule : Schedule) : Async<unit> =
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
