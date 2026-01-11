// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.ServiceModel.LifetimeTests

open System.Threading
open Microsoft.Extensions.Hosting
open Xunit

open Nightwatch.ServiceModel

type MockHostApplicationLifetime() =
    let mutable stopCalled = false
    member _.StopApplicationCalled = stopCalled
    interface IHostApplicationLifetime with
        member _.ApplicationStarted = CancellationToken.None
        member _.ApplicationStopping = CancellationToken.None
        member _.ApplicationStopped = CancellationToken.None
        member _.StopApplication() = stopCalled <- true

[<Fact>]
let ``ServiceBaseLifetime should not call StopApplication on construction``(): unit =
    let lifetime = MockHostApplicationLifetime()
    let _ = Lifetime.ServiceBaseLifetime(lifetime)
    Assert.False(lifetime.StopApplicationCalled, "StopApplication should not be called during construction")
