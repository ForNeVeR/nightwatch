// SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.Resources.Http

open System.Net
open System.Net.Http
open System.Threading.Tasks

open Xunit

open Nightwatch.Core.Network.Http
open Nightwatch.Resources

let private testMessage = new HttpResponseMessage(HttpStatusCode.Conflict)

let private http =
    { send = fun _ _ -> Task.FromResult(testMessage) }

let private test okCodes =
    let param = [| "url", "http://example.org"
                   "ok-codes", okCodes |] |> Map.ofArray
    let factory = Http.factory http
    let check = factory.create.Invoke param
    check.Invoke()

[<Fact>]
let ``Http Resource returns true on corresponding code``() =
    task {
        let! checkResult = test "200, 409"
        Assert.True checkResult
    }

[<Fact>]
let ``Http Resource returns false on non-corresponding code``() =
    task {
        let! checkResult = test "200, 408"
        Assert.False checkResult
    }
