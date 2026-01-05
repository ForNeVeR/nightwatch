// SPDX-FileCopyrightText: 2019 Friedrich von Never <friedrich@fornever.me>
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
    override __.OnStart(_ : string []) : unit = onStart()
    override __.OnStop() : unit = onStop()

/// This lifetime starts the Windows Service when the application has been started, and requests IApplicationLifetime
/// termination on service stop.
type ServiceBaseLifetime(applicationLifetime : IApplicationLifetime) =
    let startedTask = TaskCompletionSource()
    let stoppedTask = TaskCompletionSource()
    let onStart = startedTask.SetResult
    let onStop =
        applicationLifetime.StopApplication()
        stoppedTask.SetResult

    let service = new ParametrizedServiceBase(onStart, onStop)

    interface IHostLifetime with
        member this.WaitForStartAsync(ct : CancellationToken) : Task =
            let thread = Thread(fun () ->
                try
                    ServiceBase.Run service
                with
                | ex ->
                    startedTask.TrySetException ex |> ignore
                    applicationLifetime.StopApplication())

            thread.Start()
            upcast startedTask.Task

        member __.StopAsync(ct : CancellationToken) : Task =
            service.Stop()
            upcast stoppedTask.Task

let useServiceBasedRuntime(services : IServiceCollection) : unit =
    services.AddSingleton<IHostLifetime, ServiceBaseLifetime>()
    |> ignore
