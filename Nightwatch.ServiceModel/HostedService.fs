namespace Nightwatch.ServiceModel

open System.Threading
open System.Threading.Tasks

open FSharp.Control.Tasks
open Microsoft.Extensions.Hosting

type HostedService<'a>(start : CancellationToken -> Task<'a>, stop : CancellationToken -> 'a -> Task) =
    let mutable service = None

    interface IHostedService with
        member __.StartAsync(cancellationToken : CancellationToken) : Task =
            upcast task {
                let! startedService = start cancellationToken
                service <- Some startedService
            }

        member __.StopAsync(cancellationToken : CancellationToken) : Task =
            service
            |> Option.map (stop cancellationToken)
            |> Option.defaultValue Task.CompletedTask
