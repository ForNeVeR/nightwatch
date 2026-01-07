// SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Resources.Http

open System
open System.Collections.Generic
open Nightwatch.Core.Network
open Nightwatch.Core.Resources

let private splitCodes(codeString : string) =
    codeString.Split ','
    |> Seq.map (fun s -> s.Trim() |> int)
    |> Set.ofSeq

let private create (http : Http.Client) (param : IDictionary<string, string>) =
    let url = param.["url"] |> Uri
    let okCodes = param.["ok-codes"] |> splitCodes
    fun () -> task {
        let! response = http.get url
        let code = int response.StatusCode
        return Set.contains code okCodes
    }

let factory (http : Http.Client) : ResourceFactory = Factory.create "http" (create http)
