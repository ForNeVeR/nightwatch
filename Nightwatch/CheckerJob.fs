namespace Nightwatch

open Quartz

type CheckerJob() =
    static member CheckFunction = "CheckerFunction"

    interface IJob with
        member this.Execute(context) =
            upcast (async {
                let argument =
                    context.JobDetail.JobDataMap.Get CheckerJob.CheckFunction
                    :?> unit -> Async<bool>
                let! result = argument()
                ignore result
            } |> Async.StartAsTask)
