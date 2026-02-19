// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Core.Notifications

open System
open System.Collections.Generic
open System.Threading.Tasks

/// The status of a resource check for notification purposes.
type CheckStatus =
    | Failed of string
    | Recovered

/// Information about a check result to be sent in notifications.
type CheckNotification = {
    ResourceId: string
    Status: CheckStatus
    Timestamp: DateTimeOffset
}

/// A notification sender that can send check notifications.
type NotificationSender = Func<CheckNotification, Task>

/// Factory for creating notification senders based on configuration parameters.
type NotificationFactory = {
    NotificationType: string
    Create: Func<IDictionary<string, string>, NotificationSender>
}

module NotificationFactory =
    let Create (notificationType: string)
               (create: IDictionary<string, string> -> CheckNotification -> Task): NotificationFactory =
        let create = fun param -> NotificationSender(create param)
        { NotificationType = notificationType
          Create = Func<_, _> create }
