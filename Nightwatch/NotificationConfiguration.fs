// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module internal Nightwatch.NotificationConfiguration

open System
open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks
open YamlDotNet.Serialization

open Nightwatch.Core.FileSystem
open Nightwatch.Notifications

let ConfigFormatVersion: Version = Version "0.0.1.0"

type NotificationConfigurationError = {
    Path: Path
    Id: string option
    Message: string
}

let private assertCorrect (notification: NotificationDescription) path =
    let errorPath msg = Error { Path = path; Id = None; Message = msg }
    let error msg = Error { Path = path; Id = Some notification.id; Message = msg }

    match notification with
    | _ when notification.version = Version() -> errorPath "Notification version is not defined"
    | _ when notification.version <> ConfigFormatVersion ->
        errorPath $"Notification version %A{notification.version} is not supported"
    | _ when String.IsNullOrWhiteSpace notification.id -> errorPath "Notification identifier is not defined"
    | _ when String.IsNullOrWhiteSpace notification.``type`` -> error "Notification type is not defined"
    | valid -> Ok(valid, path)

let private deserializeNotification (deserializer: IDeserializer) path (reader: StreamReader) =
    let notification = deserializer.Deserialize<NotificationDescription> reader
    assertCorrect notification path

let private loadFile (fs: FileSystem) deserializer path =
    task {
        use! stream = fs.openStream path
        use reader = new StreamReader(stream)
        return deserializeNotification deserializer path reader
    }

let private buildDeserializer() =
    DeserializerBuilder()
        .WithTypeConverter(VersionConverter())
        .Build()

let private toProvider registry =
    Result.bind (fun (desc: NotificationDescription, path) ->
        match NotificationSender.create registry desc.``type`` desc.param with
        | Some sender -> Ok { id = desc.id; sender = sender }
        | None -> Error { Path = path
                          Id = Some desc.id
                          Message = $"The notification factory for type \"%s{desc.``type``}\" is not registered" })

let private configFileMask = Mask "*.yml"

let read (registry: NotificationRegistry)
         (fs: FileSystem)
         (notificationDirectory: Path)
         : Task<seq<Result<NotificationProvider, NotificationConfigurationError>>> =
    let deserializer = buildDeserializer()
    task {
        let! fileNames = fs.getFilesRecursively notificationDirectory configFileMask
        let tasks = fileNames |> Seq.map (loadFile fs deserializer)
        let! checks = Task.WhenAll tasks
        return Seq.map (toProvider registry) checks
    }
