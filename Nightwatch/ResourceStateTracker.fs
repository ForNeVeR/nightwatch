// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.CheckState

open System.Collections.Concurrent

/// Represents the last known state of a resource check.
type ResourceState =
    | Passing
    | Failing

/// Tracks the state of resource checks to detect when a resource enters an erroneous state, or when it gets recovered.
type ResourceStateTracker() =
    let states = ConcurrentDictionary<string, ResourceState option>()

    /// Updates the state and returns the new state and whether this is a state change that requires notification.
    member _.UpdateState(resourceId: string, currentlyPassing: bool): ResourceState * bool =
        let newState = if currentlyPassing then Passing else Failing
        let previousState = states.GetOrAdd(resourceId, None)
        states.[resourceId] <- Some newState

        let shouldNotify =
            match previousState, newState with
            | None, Failing -> true      // First check failed
            | None, Passing -> false     // First check passed (no notification)
            | Some Passing, Failing -> true      // Resource just failed
            | Some Failing, Passing -> true      // Resource recovered
            | Some Passing, Passing -> false     // Still passing
            | Some Failing, Failing -> false     // Still failing

        newState, shouldNotify
