namespace Nightwatch

open Quartz

open Nightwatch.Resources

type CheckerJob() =
    static member Resource = "Resource"

    interface IJob with
        member this.Execute(context) =
            upcast (async {
                let argument =
                    context.JobDetail.JobDataMap.Get CheckerJob.Resource
                    :?> Resource
                let! result = Checker.check argument
                ignore result
            } |> Async.StartAsTask)
