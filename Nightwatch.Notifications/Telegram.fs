// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Notifications.Telegram

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading.Tasks
open FSharp.Control.Tasks
open Serilog

open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Types
open Funogram.Types

open Nightwatch.Core.Notifications

let private formatMessage(notification: CheckNotification) =
    let statusText =
        match notification.Status with
        | Failed -> "FAILED"
        | Recovered -> "RECOVERED"
    sprintf "[%s] Resource %s: %s"
        (notification.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))
        notification.ResourceId
        statusText

let private createBotConfig(token: string) : BotConfig =
    { IsTest = false
      Token = token
      Offset = None
      Limit = None
      Timeout = None
      AllowedUpdates = None
      OnError = fun ex -> Log.Error(ex, "Telegram API error")
      ApiEndpointUrl = Uri("https://api.telegram.org/bot")
      Client = new HttpClient()
      WebHook = None
      RequestLogger = None }

let private create (param: IDictionary<string, string>) (notification: CheckNotification): Task =
    let botToken = param.["bot-token"]
    let chatId = param.["chat-id"] |> int64
    let config = createBotConfig botToken

    task {
        let message = formatMessage notification
        let request = Req.SendMessage.Make(chatId = ChatId.Int chatId, text = message)
        let! _ = api config request
        return ()
    }

let Factory: NotificationFactory =
    NotificationFactory.Create "telegram" create
