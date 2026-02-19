// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.ResourceStateTrackerTests

open Xunit

open Nightwatch.CheckState

[<Fact>]
let ``StateTracker should notify on first failure`` () =
    let tracker = ResourceStateTracker()
    let state, shouldNotify = tracker.UpdateState("resource1", Error "x")
    Assert.Equal(Failing "x", state)
    Assert.True(shouldNotify)

[<Fact>]
let ``StateTracker should not notify on first success`` () =
    let tracker = ResourceStateTracker()
    let state, shouldNotify = tracker.UpdateState("resource1", Ok())
    Assert.Equal(Passing, state)
    Assert.False(shouldNotify)

[<Fact>]
let ``StateTracker should notify on recovery`` () =
    let tracker = ResourceStateTracker()
    tracker.UpdateState("resource1", Error "x") |> ignore
    let state, shouldNotify = tracker.UpdateState("resource1", Ok())
    Assert.Equal(Passing, state)
    Assert.True(shouldNotify)

[<Fact>]
let ``StateTracker should notify on failure after success`` () =
    let tracker = ResourceStateTracker()
    tracker.UpdateState("resource1", Ok()) |> ignore
    let state, shouldNotify = tracker.UpdateState("resource1", Error "x")
    Assert.Equal(Failing "x", state)
    Assert.True(shouldNotify)

[<Fact>]
let ``StateTracker should not notify on continued failure`` () =
    let tracker = ResourceStateTracker()
    tracker.UpdateState("resource1", Error "x") |> ignore
    let _, shouldNotify = tracker.UpdateState("resource1", Error "x")
    Assert.False(shouldNotify)

[<Fact>]
let ``StateTracker should not notify on continued success`` () =
    let tracker = ResourceStateTracker()
    tracker.UpdateState("resource1", Ok()) |> ignore
    let _, shouldNotify = tracker.UpdateState("resource1", Ok())
    Assert.False(shouldNotify)

[<Fact>]
let ``StateTracker tracks multiple resources independently`` () =
    let tracker = ResourceStateTracker()

    // Resource 1 fails
    let state1, notify1 = tracker.UpdateState("resource1", Error "x")
    Assert.Equal(Failing "x", state1)
    Assert.True(notify1)

    // Resource 2 passes (should not notify)
    let state2, notify2 = tracker.UpdateState("resource2", Ok())
    Assert.Equal(Passing, state2)
    Assert.False(notify2)

    // Resource 1 recovers
    let state1b, notify1b = tracker.UpdateState("resource1", Ok())
    Assert.Equal(Passing, state1b)
    Assert.True(notify1b)

    // Resource 2 fails
    let state2b, notify2b = tracker.UpdateState("resource2", Error "x")
    Assert.Equal(Failing "x", state2b)
    Assert.True(notify2b)
