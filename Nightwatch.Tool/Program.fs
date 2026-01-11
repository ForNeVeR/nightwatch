// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tool.Program

open Nightwatch

[<EntryPoint>]
let main(argv: string[]): int =
    EntryPoint.Main(argv, None, None)
