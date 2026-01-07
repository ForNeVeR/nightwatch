// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.CheckerJobTests

open System
open System.Threading.Tasks
open Quartz
open Xunit

open Nightwatch
open Nightwatch.CheckState
open Nightwatch.Core.Notifications
open Nightwatch.Core.Resources
open Nightwatch.Notifications
open Nightwatch.Resources

// Test helpers to create fake resource checkers and notification senders
module private TestHelpers =
    let createResource id notificationIds checkResult =
        let checker = ResourceChecker(fun () -> Task.FromResult(checkResult))
        { id = id
          runEvery = TimeSpan.FromMinutes(5.0)
          checker = checker
          notificationIds = notificationIds }

    let createNotificationProvider id (capturedNotifications: CheckNotification list ref) =
        let sender = NotificationSender(fun notification ->
            capturedNotifications.Value <- notification :: capturedNotifications.Value
            Task.CompletedTask)
        { id = id; sender = sender }

    let createFailingNotificationProvider id =
        let sender = NotificationSender(fun _ ->
            Task.FromException<unit>(InvalidOperationException("Notification sending failed")))
        { id = id; sender = sender }

    let executeJob resource providers stateTracker =
        task {
            // Create JobDataMap the same way Scheduler does
            let jobData = Map.ofArray [|
                CheckerJob.Resource, box resource
                CheckerJob.NotificationProviders, box providers
                CheckerJob.StateTracker, box stateTracker
            |]
            let jobDetail =
                JobBuilder.Create<CheckerJob>()
                    .WithIdentity(JobKey resource.id)
                    .UsingJobData(JobDataMap jobData)
                    .Build()

            // Create minimal context
            let context =
                { new IJobExecutionContext with
                    member _.JobDetail = jobDetail
                    member _.Trigger = null
                    member _.Calendar = null
                    member _.RefireCount = 0
                    member _.MergedJobDataMap = jobDetail.JobDataMap
                    member _.FireTimeUtc = DateTimeOffset.UtcNow
                    member _.ScheduledFireTimeUtc = Nullable()
                    member _.PreviousFireTimeUtc = Nullable()
                    member _.NextFireTimeUtc = Nullable()
                    member _.JobRunTime = TimeSpan.Zero
                    member _.Result with get() = null and set _ = ()
                    member _.Scheduler = null
                    member _.FireInstanceId = ""
                    member _.RecoveringTriggerKey = null
                    member _.CancellationToken = System.Threading.CancellationToken.None
                    member _.Recovering = false
                    member _.JobInstance = null
                    member _.Put(_, _) = ()
                    member _.Get _ = null }

            let job = CheckerJob() :> IJob
            do! job.Execute(context)
        }

[<Fact>]
let ``CheckerJob should send notification when resource fails`` () =
    task {
        // Arrange
        let capturedNotifications = ref []
        let resource = TestHelpers.createResource "test-resource" [|"provider1"|] false
        let provider = TestHelpers.createNotificationProvider "provider1" capturedNotifications
        let providers = Map.ofList [("provider1", provider)]
        let stateTracker = ResourceStateTracker()

        // Act
        do! TestHelpers.executeJob resource providers stateTracker

        // Assert
        Assert.Single(!capturedNotifications) |> ignore
        let notification = List.head !capturedNotifications
        Assert.Equal("test-resource", notification.ResourceId)
        Assert.Equal(Failed, notification.Status)
    }

[<Fact>]
let ``CheckerJob should send notification when resource recovers`` () =
    task {
        // Arrange
        let capturedNotifications = ref []
        let stateTracker = ResourceStateTracker()

        // First, fail the resource
        let failingResource = TestHelpers.createResource "test-resource" [||] false
        let emptyProviders: Map<string, NotificationProvider> = Map.empty
        do! TestHelpers.executeJob failingResource emptyProviders stateTracker

        // Now, recover the resource
        let recoveringResource = TestHelpers.createResource "test-resource" [|"provider1"|] true
        let provider = TestHelpers.createNotificationProvider "provider1" capturedNotifications
        let providers = Map.ofList [("provider1", provider)]

        // Act
        do! TestHelpers.executeJob recoveringResource providers stateTracker

        // Assert
        Assert.Single(!capturedNotifications) |> ignore
        let notification = List.head !capturedNotifications
        Assert.Equal("test-resource", notification.ResourceId)
        Assert.Equal(Recovered, notification.Status)
    }

[<Fact>]
let ``CheckerJob should handle notification sending errors gracefully`` () =
    task {
        // Arrange
        let resource = TestHelpers.createResource "test-resource" [|"failing-provider"|] false
        let failingProvider = TestHelpers.createFailingNotificationProvider "failing-provider"
        let providers = Map.ofList [("failing-provider", failingProvider)]
        let stateTracker = ResourceStateTracker()

        // Act & Assert - should not throw, errors are logged internally
        do! TestHelpers.executeJob resource providers stateTracker
    }

[<Fact>]
let ``CheckerJob should handle missing notification provider`` () =
    task {
        // Arrange
        let resource = TestHelpers.createResource "test-resource" [|"missing-provider"|] false
        let providers: Map<string, NotificationProvider> = Map.empty  // No providers configured
        let stateTracker = ResourceStateTracker()

        // Act & Assert - should not throw, missing provider is logged as warning
        do! TestHelpers.executeJob resource providers stateTracker
    }

[<Fact>]
let ``CheckerJob should not send notifications when shouldNotify is false`` () =
    task {
        // Arrange
        let capturedNotifications = ref []
        let stateTracker = ResourceStateTracker()

        // First check passes - should not notify
        let passingResource1 = TestHelpers.createResource "test-resource" [|"provider1"|] true
        let provider = TestHelpers.createNotificationProvider "provider1" capturedNotifications
        let providers = Map.ofList [("provider1", provider)]
        do! TestHelpers.executeJob passingResource1 providers stateTracker

        // Second check still passes - should not notify
        let passingResource2 = TestHelpers.createResource "test-resource" [|"provider1"|] true

        // Act
        do! TestHelpers.executeJob passingResource2 providers stateTracker

        // Assert - no notifications should have been sent
        Assert.Empty(!capturedNotifications)
    }

[<Fact>]
let ``CheckerJob should send notifications to multiple providers`` () =
    task {
        // Arrange
        let capturedNotifications1 = ref []
        let capturedNotifications2 = ref []
        let resource = TestHelpers.createResource "test-resource" [|"provider1"; "provider2"|] false
        let provider1 = TestHelpers.createNotificationProvider "provider1" capturedNotifications1
        let provider2 = TestHelpers.createNotificationProvider "provider2" capturedNotifications2
        let providers = Map.ofList [("provider1", provider1); ("provider2", provider2)]
        let stateTracker = ResourceStateTracker()

        // Act
        do! TestHelpers.executeJob resource providers stateTracker

        // Assert
        Assert.Single(!capturedNotifications1) |> ignore
        Assert.Single(!capturedNotifications2) |> ignore
        Assert.Equal("test-resource", (List.head !capturedNotifications1).ResourceId)
        Assert.Equal("test-resource", (List.head !capturedNotifications2).ResourceId)
    }

[<Fact>]
let ``CheckerJob should continue sending to other providers when one fails`` () =
    task {
        // Arrange
        let capturedNotifications = ref []
        let resource = TestHelpers.createResource "test-resource" [|"failing-provider"; "working-provider"|] false
        let failingProvider = TestHelpers.createFailingNotificationProvider "failing-provider"
        let workingProvider = TestHelpers.createNotificationProvider "working-provider" capturedNotifications
        let providers = Map.ofList [("failing-provider", failingProvider); ("working-provider", workingProvider)]
        let stateTracker = ResourceStateTracker()

        // Act
        do! TestHelpers.executeJob resource providers stateTracker

        // Assert - working provider should have received notification despite failing provider
        Assert.Single(!capturedNotifications) |> ignore
        Assert.Equal("test-resource", (List.head !capturedNotifications).ResourceId)
    }
