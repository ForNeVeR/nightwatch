// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.TestUtils.Environment

open Nightwatch.Core.Environment
open TruePath

let mockEnvironment(currentDirectory: AbsolutePath) : Environment =
    fixedEnvironment currentDirectory
