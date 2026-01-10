---
_disableBreadcrumb: true
---

<!--
SPDX-FileCopyrightText: 2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>

SPDX-License-Identifier: MIT
-->

Nightwatch: Monitoring Service for .NET
=======================================
Nightwatch is a monitoring service intended to monitor daily and nightly activities and notify the administrator if something goes wrong.

It can be used as:
- A standalone monitoring server (installable via `dotnet tool install -g`)
- A set of embeddable NuGet libraries for building custom monitoring solutions

## Components

Nightwatch is composed of several libraries that can be used independently:

- **Nightwatch.Core** - Core monitoring logic and scheduling
- **Nightwatch.ServiceModel** - Service model and data types
- **Nightwatch.Resources** - Resource check implementations (HTTP, Shell, etc.)
- **Nightwatch.Notifications** - Notification providers (Telegram, etc.)

## Getting Started

### As a Standalone Tool

```bash
dotnet tool install -g Nightwatch
nightwatch --config ./nightwatch.yml
```

### As a Library

```bash
dotnet add package Nightwatch.Core
```

Then use the components in your application to build custom monitoring solutions.

## Developer Guides

- [Implementing a Resource Type](implementing-a-resource-type.md) - Guide for creating custom resource checkers

## API Documentation

Proceed to the [API documentation](api/Nightwatch.Core.html) to explore the available types and methods.

## Links

- [GitHub Repository](https://github.com/ForNeVeR/nightwatch)
- [Contributor Guide](https://github.com/ForNeVeR/nightwatch/blob/master/CONTRIBUTING.md)
