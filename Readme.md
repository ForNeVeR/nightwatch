Nightwatch [![Build (Travis)][badge-travis]][build-travis] [![Build (Appveyor)][badge-appveyor]][build-appveyor] [![Status Umbra][status-umbra]][andivionian-status-classifier]
==========

Nightwatch is a monitoring service intedned to monitor daily and nightly
activities and notify the administrator if something wrong happens.

Build
-----

[.NET Core 2.0 SDK][net-core-sdk] is required to build the project.

```console
$ dotnet build
```

Configure
---------

Nightwatch uses configuration directory with multiple configuration files. At
start, it will recursively read all the `*.yml` files in the configuration
directory, and set them up as periodic tasks. Each configuration file describes
a _Resource_.

Path to the configuration directory should be explicitly passed as an argument.

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

To stop the program, press any key.

Test
----

```console
$ dotnet test Nightwatch.Tests
```

Contributor documentation
-------------------------

- [Implementing a Resource type][implementing-a-resource-type]

[implementing-a-resource-type]: docs/implementing-a-resource-type.md

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-umbra-
[build-appveyor]: https://ci.appveyor.com/project/ForNeVeR/nightwatch/branch/master
[build-travis]: https://travis-ci.org/ForNeVeR/nightwatch
[net-core-sdk]: https://www.microsoft.com/net/download/core#/sdk

[badge-appveyor]: https://ci.appveyor.com/api/projects/status/6a2fla8atl7x0nhn/branch/master?svg=true
[badge-travis]: https://travis-ci.org/ForNeVeR/nightwatch.svg?branch=master
[status-umbra]: https://img.shields.io/badge/status-umbra-red.svg
