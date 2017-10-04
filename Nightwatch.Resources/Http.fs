module Nightwatch.Resources.Http

open System
open System.Collections.Generic
open System.Net.Http

open Nightwatch.Core.Resources

let private splitCodes (codeString : string) =
    codeString.Split ','
    |> Seq.map (fun s -> s.Trim() |> int)
    |> Set.ofSeq

let private create(param : IDictionary<string, string>) =
    let url = param.["url"]
    let okCodes = param.["ok-codes"] |> splitCodes
    fun () -> async {
        use client = new HttpClient()
        let! response = Async.AwaitTask <| client.GetAsync url
        let code = int response.StatusCode
        return Set.contains code okCodes
    }

let factory = fSharpFactory "http" create
