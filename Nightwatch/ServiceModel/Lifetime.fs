// SPDX-FileCopyrightText: 2019-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.ServiceModel.Lifetime

open System.ServiceProcess
open System.Threading
open System.Threading.Tasks

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type private ParametrizedServiceBase(onStart : unit -> unit, onStop : unit -> unit) =
    inherit ServiceBase()
    override _.OnStart(_ : string []) : unit = onStart()
    override _.OnStop() : unit = onStop()

/// This lifetime starts the Windows Service when the application has been started, and requests IApplicationLifetime
/// termination on service stop.
type ServiceBaseLifetime(applicationLifetime : IHostApplicationLifetime) =
    let startedTask = TaskCompletionSource()
    let stoppedTask = TaskCompletionSource()
    let onStart = startedTask.SetResult
    let onStop() =
        applicationLifetime.StopApplication()
        stoppedTask.SetResult()

    let service = new ParametrizedServiceBase(onStart, onStop)

    interface IHostLifetime with
        member this.WaitForStartAsync _ =
            let thread = Thread(fun () ->
                try
                    ServiceBase.Run service
                with
                | ex ->
                    startedTask.TrySetException ex |> ignore
                    applicationLifetime.StopApplication())

            thread.Start()
            startedTask.Task

        member _.StopAsync _ =
            service.Stop()
            stoppedTask.Task

let useServiceBasedRuntime(services : IServiceCollection) : unit =
    services.AddSingleton<IHostLifetime, ServiceBaseLifetime>()
    |> ignore
