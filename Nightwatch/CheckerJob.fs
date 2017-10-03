namespace Nightwatch

open Quartz

open Nightwatch.Core
open Nightwatch.Core.Resources

type CheckerJob() =
    static member Resource = "Resource"

    interface IJob with
        member this.Execute(context) =
            upcast (async {
                let argument =
                    context.JobDetail.JobDataMap.Get CheckerJob.Resource
                    :?> Resource
                let! result = Resources.check argument
                ignore result
            } |> Async.StartAsTask)
