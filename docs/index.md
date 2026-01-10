---
_disableBreadcrumb: true
---

<!--
SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Nightwatch: Monitoring Service for .NET
=======================================
Nightwatch is a monitoring service intended to monitor a set of specified _resources_ and notify the administrator if something goes wrong with them.

It can be used as:
- a standalone monitoring server (installable as a .NET tool via `dotnet tool install -g`);
- a set of embeddable NuGet libraries for building custom monitoring solutions.

## Components

Nightwatch is composed of several libraries that can be used independently:

- **Nightwatch.Core** — core monitoring logic and scheduling;
- **Nightwatch.Resources** — resource check implementations (HTTP, Shell, etc.);
- **Nightwatch.Notifications** — notification providers (Telegram, etc.);
- **Nightwatch** — the main package wiring everything else together.

## Getting Started

### As a Standalone Tool

```console
$ dotnet tool install -g FVNever.Nightwatch.Tool
$ nightwatch --config ./nightwatch.yml
```

### As a Library

```console
$ dotnet add package FVNever.Nightwatch.Core
```

Then use the components in your application to build custom monitoring solutions.

## Developer Guides

- [Implementing a Resource Type](implementing-a-resource-type.md) - Guide for creating custom resource checkers

## API Documentation

Proceed to the [API documentation](xref:Nightwatch) to explore the available types and methods.

## Links

- [GitHub Repository](https://github.com/ForNeVeR/nightwatch)
- [Contributor Guide](https://github.com/ForNeVeR/nightwatch/blob/master/CONTRIBUTING.md)
