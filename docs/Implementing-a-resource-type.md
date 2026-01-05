<!--
SPDX-FileCopyrightText: 2017 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Implementing a Resource type
============================

In Nightwatch, a _Resource_ is an entity combined of

- a way to check if resource's parameters are nominal
- a check schedule

There're some predefined resources listed in [the Readme file][readme], but if
they aren't enough, custom Resource may be implemented in any .NET-compatible
language. User defines Resources in the configuration file, and there could be
multiple instances of each Resource type at any moment of time.

Assembly references
-------------------

Any custom Resource should reference `Nightwatch.Core` assembly.

Defining a factory
------------------

Resource factory is an entity that will be used by Nightwatch to instantiate
the Resource.

Resource author should provide a
[`Nightwatch.Core.Resources.ResourceFactory`][resourcefactory] instance that has
the following properties:

- `resourceType : string` — the name of the Resource type. Nightwatch will
  instantiate a Resource if the configuration file names that type.
- `create : Func<IDictionary<string, string>, ResourceChecker>` — the checker
  factory. It takes the user-defined parameters from the configuration file and
  returns a function that will be called each time Nightwatch need to perform
  check for that particular Resource instance.

There's a helper method `Nightwatch.Core.Resources.Factory.create` to
instantiate a factory from F#. If using other languages, just call the
`ResourceFactory` constructor.

For a sample Resource implementation in C#, see
[Nightwatch.Samples.CSharp][nightwatch-samples-csharp] project.

Registering a factory
---------------------

Currently, to register a Resource factory, you need to add it to the
`resourceFactories` collection in [`Nightwatch.Program`][nightwatch-program]
module and recompile Nightwatch.

In future, there'll be a way to register a Resource factory without recompiling
the code. See [the corresponding issue][issue-14].

[readme]: ../Readme.md
[resourcefactory]: ../Nightwatch.Core/Resources.fs
[nightwatch-program]: ../Nightwatch/Program.fs
[nightwatch-samples-csharp]: ../Nightwatch.Samples.CSharp

[issue-14]: https://github.com/ForNeVeR/nightwatch/issues/14
