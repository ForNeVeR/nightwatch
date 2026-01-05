// SPDX-FileCopyrightText: 2018 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Core.Environment

open System

open Nightwatch.Core.FileSystem

type Environment =
    { currentDirectory : Path }

let fixedEnvironment(currentDirectory : Path) : Environment =
    { currentDirectory = currentDirectory }
