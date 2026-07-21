# Xtate.Core

[![NuGet](https://img.shields.io/nuget/v/Xtate.Core.svg)](https://www.nuget.org/packages/Xtate.Core)
[![CodeQL](https://github.com/TSSArt/Xtate.Core/actions/workflows/codeql.yml/badge.svg)](https://github.com/TSSArt/Xtate.Core/actions/workflows/codeql.yml)
[![License: AGPL-3.0-or-later](https://img.shields.io/badge/license-AGPL--3.0--or--later-blue.svg)](LICENSE)

Xtate.Core is the core .NET state-machine library for [Xtate](https://xtate.net/). It implements the [W3C SCXML specification](https://www.w3.org/TR/scxml/) and provides the model, parser, interpreter, hosting, transport, and persistence infrastructure used by the Xtate ecosystem.

## Features

- Parse and serialize SCXML documents, including optional XInclude support.
- Build state machines through object-model and fluent C# APIs.
- Execute state machines with an asynchronous SCXML interpreter.
- Select pluggable null, runtime, XPath, or external data-model handlers.
- Route events through in-process, HTTP, and named-pipe I/O processors.
- Invoke external services and optionally persist interpreter state.
- Compose features explicitly through [Xtate.IoC](https://www.nuget.org/packages/Xtate.IoC/).

## Installation

Install the package from [NuGet](https://www.nuget.org/packages/Xtate.Core):

```shell
dotnet add package Xtate.Core
```

## Quick start

The following example runs a minimal SCXML document and reads its final data:

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

Console.WriteLine(result);
```

Features are registered through module classes such as `StateMachineInterpreterModule`, `ScxmlModule`, and `PersistenceModule`.

## Supported frameworks

The library targets .NET 11, .NET 10, .NET 9, .NET 8, .NET Standard 2.0, and .NET Framework 4.6.2.

## Building from source

Install a .NET SDK capable of building the repository's target frameworks. Mono is required when building or testing the .NET Framework target on a non-Windows system.

```shell
git clone https://github.com/TSSArt/Xtate.Core.git
cd Xtate.Core
dotnet restore
dotnet build Xtate.Core.sln
dotnet test Xtate.Core.sln
```

For a faster focused test loop:

```shell
dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj \
  --framework net10.0 \
  --filter "FullyQualifiedName~InterpreterTest"
```

## Repository layout

| Path | Description |
| --- | --- |
| `src/Xtate.Core` | Library source and NuGet package project |
| `test/Xtate.Core.Test` | MSTest unit, integration, persistence, and SCXML behavior tests |
| `.github/instructions` | Path-specific guidance for coding agents |
| `.github/workflows` | Build, security analysis, and publishing workflows |
| `.agents` | Repository guide and focused test-planning documents |

## Contributing

Contributions are welcome. Read the [repository guide](.agents/AGENTS.md), follow `.editorconfig`, and add or update tests for behavioral changes. Keep generated files and unrelated work unchanged.

Use [GitHub Issues](https://github.com/TSSArt/Xtate.Core/issues) for bug reports and feature requests. For bugs, include the target framework, a minimal SCXML document or code sample, and the expected and actual behavior.

## License

Xtate.Core is licensed under the [GNU Affero General Public License v3.0 or later](LICENSE).
