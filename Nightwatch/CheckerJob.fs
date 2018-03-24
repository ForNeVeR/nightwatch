namespace Nightwatch

open Quartz
open FSharp.Control.Tasks

open Nightwatch.Resources

type CheckerJob() =
    static member Resource = "Resource"

    interface IJob with
        member this.Execute(context) =
            upcast task {
                let argument =
                    context.JobDetail.JobDataMap.Get CheckerJob.Resource
                    :?> Resource
                let! result = Checker.check argument
                ignore result
            }
