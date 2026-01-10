// SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Core.Environment

open TruePath

type Environment = {
    CurrentDirectory : AbsolutePath
}

let fixedEnvironment(currentDirectory: AbsolutePath): Environment =
    { CurrentDirectory = currentDirectory }
