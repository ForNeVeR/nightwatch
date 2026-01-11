// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace Nightwatch.Notifications

open System
open System.Collections.Generic
open System.Threading.Tasks
open Serilog

open Nightwatch.Core.Notifications

/// Description of a Notification Provider stored in the configuration file.
[<CLIMutable>]
type NotificationDescription =
    { version: Version
      id: string
      ``type``: string
      param: Dictionary<string, string> }

type internal NotificationRegistry = IReadOnlyDictionary<string, NotificationFactory>

type internal NotificationProvider =
    { id: string
      sender: NotificationSender }

module NotificationRegistry =
    let Create (factories: NotificationFactory seq): NotificationRegistry =
        upcast (
            factories
            |> Seq.map (fun f -> f.NotificationType, f)
            |> Map.ofSeq
        )

module internal NotificationSender =
    let create (registry: NotificationRegistry) (notificationType: string) (param: IDictionary<string, string>)
               : NotificationSender option =
        match registry.TryGetValue notificationType with
        | false, _ -> None
        | true, factory -> Some <| factory.Create.Invoke param

    let send (provider: NotificationProvider) (notification: CheckNotification): Task =
        task {
            Log.Information("Sending notification via {0} for resource {1}...", provider.id, notification.ResourceId)
            return! provider.sender.Invoke(notification)
        }
