// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.CheckState

open System.Collections.Concurrent

/// Represents the last known state of a resource check.
type ResourceState =
    | Passing
    | Failing of string

/// Tracks the state of resource checks to detect when a resource enters an erroneous state, or when it gets recovered.
type ResourceStateTracker() =
    let states = ConcurrentDictionary<string, ResourceState option>()

    /// Updates the state and returns the new state and whether this is a state change that requires notification.
    member _.UpdateState(resourceId: string, currentResult: Result<unit, string>): ResourceState * bool =
        let newState =
            match currentResult with
            | Ok() -> Passing
            | Error message -> Failing message
        let previousState = states.GetOrAdd(resourceId, None)
        states[resourceId] <- Some newState

        let shouldNotify =
            match previousState, newState with
            | None, Failing _ -> true      // First check failed
            | None, Passing -> false     // First check passed (no notification)
            | Some Passing, Failing _ -> true      // Resource just failed
            | Some(Failing _), Passing -> true      // Resource recovered
            | Some Passing, Passing -> false     // Still passing
            | Some(Failing _), Failing _ -> false     // Still failing

        newState, shouldNotify
