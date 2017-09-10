open System
open System.Reflection

[<EntryPoint>]
let main argv =
    let version = Assembly.GetEntryAssembly().GetName().Version
    printfn "Nightwatch v. %A" version
    0
