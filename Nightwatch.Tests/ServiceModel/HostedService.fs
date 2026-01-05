// SPDX-FileCopyrightText: 2019 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.ServiceModel.HostedService

open System.Threading.Tasks

open FSharp.Control.Tasks

open Microsoft.Extensions.Hosting
open Xunit

open Nightwatch.ServiceModel

[<Fact>]
let ``HostedService should call start action on start``() : Task =
    let mutable started = false
    let service : IHostedService =
        upcast HostedService<obj>(
            (fun _ ->
                started <- true
                Task.FromResult null),
            fun _ _ -> Task.CompletedTask)
    upcast task {
        do! service.StartAsync(Unchecked.defaultof<_>)
        Assert.True started
    }

[<Fact>]
let ``HostedService should call stop action on stop``() : Task =
    let mutable stopped = false
    let service : IHostedService =
        upcast HostedService<obj>((fun _ -> Task.FromResult null), fun _ _ -> stopped <- true; Task.CompletedTask)
    upcast task {
        do! service.StartAsync(Unchecked.defaultof<_>)
        Assert.False stopped
        do! service.StopAsync(Unchecked.defaultof<_>)
        Assert.True stopped
    }

[<Fact>]
let ``HostedService does nothing if stop called without start``() : Task =
    let mutable stopped = false
    let service : IHostedService =
        upcast HostedService<obj>((fun _ -> Task.FromResult null), fun _ _ -> stopped <- true; Task.CompletedTask)
    upcast task {
        do! service.StopAsync(Unchecked.defaultof<_>)
        Assert.False stopped
    }
