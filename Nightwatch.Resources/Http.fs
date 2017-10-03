module Nightwatch.Resources.Http

open System.Net.Http

open Nightwatch.Core.Resources

let private splitCodes (codeString : string) =
    codeString.Split ','
    |> Seq.map (fun s -> s.Trim() |> int)
    |> Set.ofSeq

let private create(param : Map<string, string>) =
    let url = Map.find "url" param
    let okCodes = Map.find "ok-codes" param |> splitCodes
    fun () -> async {
        use client = new HttpClient()
        let! response = Async.AwaitTask <| client.GetAsync url
        let code = int response.StatusCode
        return Set.contains code okCodes
    }

let factory =
    { resourceType = "http"
      create = create }
