// SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.Core.Network

open System
open System.Net
open System.Net.Sockets
open FSharp.Control.Tasks
open Xunit

open Nightwatch.Core

let private getUnusedPort() =
    // TcpListener will automatically select an unused port if passed 0
    use listener = new TcpListener(IPAddress.Any, 0)
    listener.Start()
    let endpoint = listener.LocalEndpoint :?> IPEndPoint
    let port = endpoint.Port
    listener.Stop()
    port

let private listenHttp() =
    let port = getUnusedPort()
    let listener = new HttpListener()
    let prefix = $"http://localhost:%d{port}/"
    listener.Prefixes.Add prefix
    listener.Start()
    listener

let http = Network.Http.system

[<Fact>]
let ``HTTP request should be sent successfully``() =
    use listener = listenHttp()
    let url = Seq.exactlyOne listener.Prefixes |> Uri
    let code = HttpStatusCode.Conflict
    let test =
        task {
            let! result = http.get url
            Assert.Equal(code, result.StatusCode)
        }
    let context = listener.GetContext()
    context.Response.StatusCode <- int code
    context.Response.Close()
    test
