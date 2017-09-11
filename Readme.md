Nightwatch [![Build (Travis)][badge-travis]][build-travis] [![Build (Appveyor)][badge-appveyor]][build-appveyor] [![Status Zero][status-zero]][andivionian-status-classifier]
==========

Nightwatch is a monitoring service intedned to monitor daily and nightly
activities and notify the administrator if something wrong happens.

Build
-----

[.NET Core 2.0 SDK][net-core-sdk] is required to build the project.

```console
$ dotnet build 
```

Run
---

In developer mode:

```console
$ dotnet run --project Nightwatch
```

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-zero-
[build-appveyor]: https://ci.appveyor.com/project/ForNeVeR/nightwatch/branch/master
[build-travis]: https://travis-ci.org/ForNeVeR/nightwatch
[net-core-sdk]: https://www.microsoft.com/net/download/core#/sdk

[badge-appveyor]: https://ci.appveyor.com/api/projects/status/6a2fla8atl7x0nhn/branch/master?svg=true
[badge-travis]: https://travis-ci.org/ForNeVeR/nightwatch.svg?branch=master
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
