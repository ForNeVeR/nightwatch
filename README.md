<!--
SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Nightwatch [![Status Ventis][status-ventis]][andivionian-status-classifier]
==========

Nightwatch is a monitoring service intended to monitor daily and nightly activities and notify the administrator if something wrong happens.

Packages
--------
| Package                      | NuGet                                                                                        |
|------------------------------|----------------------------------------------------------------------------------------------|
| **Nightwatch**               | [![Nightwatch on nuget.org][badge.nightwatch]][nuget.nightwatch]                             |
| **Nightwatch.Core**          | [![Nightwatch on nuget.org][badge.nightwatch.core]][nuget.nightwatch.core]                   |
| **Nightwatch.Notifications** | [![Nightwatch on nuget.org][badge.nightwatch.notifications]][nuget.nightwatch.notifications] |
| **Nightwatch.Resources**     | [![Nightwatch on nuget.org][badge.nightwatch.resources]][nuget.nightwatch.resources]         |
| **Nightwatch.Tool**          | [![Nightwatch on nuget.org][badge.nightwatch.tool]][nuget.nightwatch.tool]                   |

Configure
---------

Nightwatch could be configured by placing the `nightwatch.yml` file in the
current directory. You may also override the configuration file path by using
the command-line arguments, see the Run section of this document.

`nightwatch.yml` has the following form:

```yaml
resource-directory: "some/path"
notification-directory: "notifications/path"
log-file: "logs/nightwatch.log"  # optional
```

Nightwatch searches the resource directory for the configuration files. At
start, it will recursively read all the `*.yml` files in the resource
directory, and set them up as periodic tasks. Each configuration file describes
a _Resource_.

Similarly, Nightwatch reads all `*.yml` files from the notification directory to configure _Notification_ providers. When a resource check fails or recovers, Nightwatch sends notifications via the providers listed in that resource's `notifications` section, not via all configured providers.

See [Resource Documentation](docs/resources.md) for details on available resource types (Shell, HTTP, HTTPS Certificate).

See [Notification Documentation](docs/notifications.md) for details on available notification providers (Telegram).

### Logging Configuration

By default, Nightwatch logs to the console. When running as a Windows service or in environments where console output is not accessible, you can redirect logs to a file:

```yaml
log-file: "logs/nightwatch.log"
```

The path can be relative (resolved from the configuration file's directory) or absolute. When this option is not set or empty, logs are written to the console.

Usage
-----
There are two ways to use Nightwatch: install it and run it in service mode, or install it as a package into an F# project and build your own monitoring service based on its API.

### Installation in Service Mode
Install [.NET SDK][dotnet] 10.0 or later.

Then, install Nightwatch as a global tool:
```console
$ dotnet tool install --global FVNever.Nightwatch.Tool
```

After installation:
```console
$ nightwatch [--config ./some/path.yml] [--service]
```

To stop the program, press `Ctrl-C`.

Add `--config ./some/path.yml` option to set the configuration file path
(`nightwatch.yml` in the current directory is the default).

Add `--service` to run in a Windows service mode.

To install the service on Windows, execute the following commands in your shell:

```pwsh
$ sc.exe Nightwatch binpath= "D:\Path\To\nightwatch.exe --config D:\Path\To\nightwatch.yml --service" start= auto
$ sc.exe start Nightwatch
```

_(note the space and quote placement, that's important)_

**Note:** When running as a Windows service, logs are not visible in a console. It is recommended to configure file logging in your `nightwatch.yml`:

```yaml
log-file: "D:\\Path\\To\\logs\\nightwatch.log"
```

Make sure the service account has write permissions to the log directory.

### Installation as Package
Install the package `FVNever.Nightwatch` into a .NET project or an `fsx` file:
```fsharp
#r "nuget: FVNever.Nightwatch"
exit <| Nightwatch.EntryPoint.FsiMain(fsi.CommandLineArgs, None, None)
// you may override the resources or notification registries via the last two parameters
```
Read more in the [API reference][docs.api].

Documentation
-------------
- [Resource Types](docs/resources.md)
- [Notification Providers](docs/notifications.md)
- [Implementing a Resource Type][implementing-a-resource-type]
- [API Reference][docs.api]
- [Contributor Guide][docs.contributing]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec].

[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-ventis-
[badge.nightwatch.core]: https://img.shields.io/nuget/v/FVNever.Nightwatch.Core?label=FVNever.Nightwatch.Core
[badge.nightwatch.notifications]: https://img.shields.io/nuget/v/FVNever.Nightwatch.Notifications?label=FVNever.Nightwatch.Notifications
[badge.nightwatch.resources]: https://img.shields.io/nuget/v/FVNever.Nightwatch.Resources?label=FVNever.Nightwatch.Resources
[badge.nightwatch.tool]: https://img.shields.io/nuget/v/FVNever.Nightwatch.Tool?label=FVNever.Nightwatch.Tool
[badge.nightwatch]: https://img.shields.io/nuget/v/FVNever.Nightwatch?label=FVNever.Nightwatch
[docs.api]: https://fornever.github.io/nightwatch/
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[dotnet]: https://dotnet.microsoft.com
[implementing-a-resource-type]: docs/implementing-a-resource-type.md
[net-core-sdk]: https://www.microsoft.com/net/download/core#/sdk
[nuget.nightwatch.core]: https://www.nuget.org/packages/FVNever.Nightwatch.Core/
[nuget.nightwatch.notifications]: https://www.nuget.org/packages/FVNever.Nightwatch.Notifications/
[nuget.nightwatch.resources]: https://www.nuget.org/packages/FVNever.Nightwatch.Resources/
[nuget.nightwatch.tool]: https://www.nuget.org/packages/FVNever.Nightwatch.Tool/
[nuget.nightwatch]: https://www.nuget.org/packages/FVNever.Nightwatch/
[reuse.spec]: https://reuse.software/spec-3.3/
[status-ventis]: https://img.shields.io/badge/status-ventis-yellow.svg
