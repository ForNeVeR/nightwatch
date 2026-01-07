// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

namespace Nightwatch

open System
open Quartz
open FSharp.Control.Tasks
open Serilog

open Nightwatch.Core.Notifications
open Nightwatch.Resources
open Nightwatch.Notifications
open Nightwatch.CheckState

type CheckerJob() =
    static member Resource = "Resource"
    static member NotificationProviders = "NotificationProviders"
    static member StateTracker = "StateTracker"

    interface IJob with
        member _.Execute(context) =
            upcast task {
                let argument =
                    context.JobDetail.JobDataMap.Get CheckerJob.Resource
                    :?> Resource
                let providers =
                    context.JobDetail.JobDataMap.Get CheckerJob.NotificationProviders
                    :?> Map<string, NotificationProvider>
                let stateTracker =
                    context.JobDetail.JobDataMap.Get CheckerJob.StateTracker
                    :?> ResourceStateTracker

                let! result = Checker.check argument
                let newState, shouldNotify = stateTracker.UpdateState(argument.id, result)

                if shouldNotify then
                    let status =
                        match newState with
                        | Passing -> Recovered
                        | Failing -> Failed

                    let notification =
                        { ResourceId = argument.id
                          Status = status
                          Timestamp = DateTimeOffset.UtcNow }

                    for notificationId in argument.notificationIds do
                        match Map.tryFind notificationId providers with
                        | Some provider ->
                            try
                                do! NotificationSender.send provider notification
                            with ex ->
                                Log.Error(ex, "Failed to send notification via {0} for resource {1}",
                                          notificationId, argument.id)
                        | None ->
                            Log.Warning("Notification provider {0} not found for resource {1}",
                                       notificationId, argument.id)
            }
