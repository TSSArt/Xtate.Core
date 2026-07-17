# Xtate.Core

[![NuGet](https://img.shields.io/nuget/v/Xtate.Core.svg)](https://www.nuget.org/packages/Xtate.Core)
[![CodeQL](https://github.com/TSSArt/Xtate.Core/actions/workflows/codeql.yml/badge.svg)](https://github.com/TSSArt/Xtate.Core/actions/workflows/codeql.yml)
[![License: AGPL-3.0-or-later](https://img.shields.io/badge/license-AGPL--3.0--or--later-blue.svg)](LICENSE)

Xtate.Core is a multi-targeted .NET state-machine framework centered on the [W3C SCXML specification](https://www.w3.org/TR/scxml/).

It provides SCXML parsing and serialization, programmatic and fluent state-machine builders, an asynchronous interpreter, host and event routing, pluggable data models, resource loaders, I/O processors, and optional persistence.

## Features

- Parse and serialize SCXML documents, including optional XInclude support.
- Build state machines through object-model and fluent C# APIs.
- Run state machines with an asynchronous interpreter.
- Use null, runtime, or XPath data-model handlers.
- Route events through in-process, HTTP, and named-pipe I/O processors.
- Invoke external services and persist interpreter state.
- Target .NET 11, .NET 10, .NET 9, .NET 8, .NET Standard 2.0, and .NET Framework 4.6.2.

## Installation

Install the package from [NuGet](https://www.nuget.org/packages/Xtate.Core):

```shell
dotnet add package Xtate.Core
```

## Quick start

The following example runs a small SCXML state machine and returns its final data:

```csharp
using Xtate.Class;
using Xtate.Interpreter;
using Xtate.Interpreter.DependencyInjection;
using Xtate.IoC;

const string scxml = """
    <scxml xmlns="http://www.w3.org/2005/07/scxml"
           version="1.0"
           datamodel="xpath"
           initial="done">
      <final id="done">
        <donedata>
          <content>Hello from Xtate!</content>
        </donedata>
      </final>
    </scxml>
    """;

var services = new ServiceCollection();
var stateMachine = new ScxmlStringStateMachine(scxml);

stateMachine.AddServices(services);
services.AddModule<StateMachineInterpreterModule>();

var provider = services.BuildProvider();
var interpreter = await provider.GetRequiredService<IStateMachineInterpreter>();
var result = await interpreter.Run();

Console.WriteLine(result); // Hello from Xtate!
```

Xtate.Core uses [Xtate.IoC](https://www.nuget.org/packages/Xtate.IoC/) for service composition. Features are registered through module classes such as `StateMachineInterpreterModule`, `ScxmlModule`, and `PersistenceModule`.

## Building from source

### Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0)
- Mono when building or testing the .NET Framework target on a non-Windows system

Clone the repository, then build and test the solution from its root:

```shell
git clone https://github.com/TSSArt/Xtate.Core.git
cd Xtate.Core
dotnet restore
dotnet build Xtate.Core.sln
dotnet test Xtate.Core.sln
```

To run a focused test against one framework:

```shell
dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj \
  --framework net10.0 \
  --filter "FullyQualifiedName~InterpreterTest"
```

## Repository structure

| Path | Description |
| --- | --- |
| `src/Xtate.Core` | Library source and NuGet package project |
| `test/Xtate.Core.Test` | MSTest unit, integration, persistence, and SCXML behavior tests |
| `.github/workflows` | Build, security analysis, and publishing workflows |
| `.agents/AGENTS.md` | Architecture, conventions, and maintainer guidance |

## Contributing

Contributions are welcome. Before making changes, read the [repository guide](.agents/AGENTS.md), follow the existing code style, and add or update tests for behavioral changes.

When reporting a bug, include the target framework, a minimal SCXML document or code sample, the expected behavior, and the actual behavior. Use [GitHub Issues](https://github.com/TSSArt/Xtate.Core/issues) for bug reports and feature requests.

## License

Xtate.Core is licensed under the [GNU Affero General Public License v3.0 or later](LICENSE).
