namespace Nightwatch.Core.Network

open System
open System.Net.Http
open System.Threading.Tasks

open FSharp.Control.Tasks

module Http =
    type Client =
        { send : HttpMethod -> Uri -> Task<HttpResponseMessage> }
        with member this.get : Uri -> Task<HttpResponseMessage> = this.send HttpMethod.Get

    let private send method (uri : Uri) =
        task {
            use client = new HttpClient()
            use message = new HttpRequestMessage(method, uri)
            return! client.SendAsync message
        }

    let system : Client =
        { send = send }
