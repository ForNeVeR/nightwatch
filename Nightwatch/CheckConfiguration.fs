namespace Nightwatch

open System

type CheckConfiguration =
    { id : string
      runEvery : TimeSpan
      checkFunction : unit -> Async<bool> }
