<!--
SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Nightwatch [![Build (Travis)][badge-travis]][build-travis] [![Build (Appveyor)][badge-appveyor]][build-appveyor] [![Status Umbra][status-umbra]][andivionian-status-classifier]
==========

Nightwatch is a monitoring service intedned to monitor daily and nightly
activities and notify the administrator if something wrong happens.

Build
-----

[.NET Core 2.1 SDK][net-core-sdk] is required to build the project.

```console
$ dotnet build
```

If you need a standalone executable (useful for service deployment), then add
the following options:

```console
$ cd Nightwatch
$ dotnet build --configuration Release --runtime win-x64 --output out
```

Configure
---------

Nightwatch could be configured by placing the `nightwatch.yml` file in the
current directory. You may also override the configuration file path by using
the command-line arguments, see the Run section of this document.

`nightwatch.yml` has the following form:

```yaml
resource-directory: "some/path"
```

Nightwatch searches the resource directory for the configuration files. At
start, it will recursively read all the `*.yml` files in the resource
directory, and set them up as periodic tasks. Each configuration file describes
a _Resource_.

Currently supported resources are documented below.

### Shell Resource

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: test # task identifier
schedule: 00:05:00 # run every 5 minutes
type: shell
param:
    cmd: ping localhost # check command
```

### HTTP Resource

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: test # task identifier
schedule: 00:05:00 # run every 5 minutes
type: shell
param:
    url: http://localhost:8080/ # URL to visit
    ok-codes: 200, 304 # the list of the codes considered as a success
```

Run
---

In developer mode:

```console
$ dotnet run --project Nightwatch
```

To stop the program, press `Ctrl-C`.

Add `--config ./some/path.yml` option to set the configuration file path
(`nightwatch.yml` in the current directory is the default).

Add `--service` to run in a Windows service mode.

To install the service on Windows, execute the following commands in your shell:

```pwsh
$ sc.exe Nightwatch binpath= "D:\Path\To\Nightwatch.exe --config D:\Path\To\nightwatch.yml --service" start= auto
$ sc.exe start Nightwatch
```

_(note the space and quote placement, that's important)_

Test
----

```console
$ dotnet test Nightwatch.Tests
```

Documentation
-------------
- [Implementing a Resource type][implementing-a-resource-type]
- [Contributor Guide][docs.contributing]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec].

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-umbra-
[badge-appveyor]: https://ci.appveyor.com/api/projects/status/6a2fla8atl7x0nhn/branch/master?svg=true
[badge-travis]: https://travis-ci.org/ForNeVeR/nightwatch.svg?branch=master
[build-appveyor]: https://ci.appveyor.com/project/ForNeVeR/nightwatch/branch/master
[build-travis]: https://travis-ci.org/ForNeVeR/nightwatch
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[implementing-a-resource-type]: docs/implementing-a-resource-type.md
[net-core-sdk]: https://www.microsoft.com/net/download/core#/sdk
[reuse.spec]: https://reuse.software/spec-3.3/
[status-umbra]: https://img.shields.io/badge/status-umbra-red.svg
