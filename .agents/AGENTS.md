# Xtate.Core repository guide

Use this guide as the first source of repository context. Inspect the subsystem relevant to the requested change and expand the search only when dependencies cross that boundary.

## Project purpose

Xtate.Core is the core C# implementation of the Xtate SCXML state-machine framework. It contains one production library and one MSTest project:

| Path | Purpose |
| --- | --- |
| `src/Xtate.Core/Xtate.Core.csproj` | Multi-targeted library and NuGet package; root namespace `Xtate` |
| `test/Xtate.Core.Test/Xtate.Core.Test.csproj` | Unit, integration, hosted, persistence, and SCXML behavior tests |
| `Xtate.Core.sln` | Repository solution |

The library targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, and `net462`. The test project targets `net10.0`, `net9.0`, `net8.0`, and, unless `SkipNetFrameworkTests=true`, `net462`.

## Architecture

The normal runtime flow is:

1. A class in `Class/DependencyInjection` supplies an SCXML document, URI, stream, or runtime model.
2. `Scxml` loads and deserializes XML into the public object model under `StateMachine`.
3. `Interpreter/ModelBuilder` validates and compiles the public model into executable nodes.
4. `Interpreter/Services/StateMachineInterpreter.cs` executes the model.
5. `StateMachineHost` manages scopes, lifecycle, event routing, scheduling, invoked services, and security context.
6. `Persistence` optionally persists interpreter state and data-model values.

Composition uses Xtate.IoC, not Microsoft.Extensions.DependencyInjection. Trace composition from the nearest `DependencyInjection/*Module.cs` file. Registration order, `Option.IfNotRegistered`, forwarding, and `SharedWithin` choices are behavioral.

The public graph in `StateMachine` and the compiled graph in `Interpreter/ModelBuilder/Model` have different responsibilities. A parsing or public-model change may require corresponding validator, compiled-model, interpreter, and persistence updates.

## Subsystem map

| Area | Responsibility | Typical tests |
| --- | --- | --- |
| `Actions` | Built-in system actions | `HostedTests`, `DevTests` |
| `Class` | User-facing state-machine source wrappers | `RegisterClassTest`, factory tests |
| `Common` | Helpers, logging, task monitoring, and polyfills | `UnitTests/Common` |
| `DataModel` | Data-model contracts and built-in handlers | XPath and state-machine tests |
| `DataTypes` | Dynamic SCXML values and conversions | `DataModel*Test`, `UnitTests/DataModel` |
| `Interpreter` | Executable model and SCXML algorithm | `Interpreter`, `StateMachines` |
| `IoC` | Xtate-specific composition helpers | `DI` |
| `IoProcessors` | HTTP and named-pipe event transports | `UnitTests/IoProcessors` |
| `Persistence` | Storage and persisted runtime state | persistence and legacy tests |
| `ResourceLoaders` | File, resource, and web URI loading | XInclude and registration tests |
| `Scxml` | SCXML parsing, serialization, and XInclude | SCXML and XInclude tests |
| `StateMachine` | Public model, builders, visitors, and validation | state-machine tests |
| `StateMachineFluentBuilder` | Fluent C# construction API | fluent-builder tests |
| `StateMachineHost` | Runtime orchestration and event routing | hosted and persistence tests |

For a detailed repository catalog, see [`.github/instructions/repo-catalog.instructions.md`](../.github/instructions/repo-catalog.instructions.md).

## Code conventions and hazards

- Follow `.editorconfig`: tabs, nullable annotations, analyzer rules, and existing naming/style.
- Match the AGPL header and current year/style in adjacent C# files.
- Use `ConfigureAwait(false)` where required by the analyzer and nearby library code.
- Preserve compatibility code under `Common/Polyfills`; guard additions with precise target checks.
- Keep package versions in `Directory.Packages.props`; omit versions from `PackageReference` entries.
- Treat `Directory.Build.props`, `Global.Packages.props`, and generated resource designer files as generated.
- Edit `Properties/Resources.resx`, not `Properties/Resources.Designer.cs`.
- Preserve IoC property injection and factory patterns unless a task explicitly changes composition.
- Ignore `bin`, `obj`, `TestResults`, and IDE metadata.

Path-specific rules in `.github/instructions` take precedence for matching files.

## Build and test

From the repository root:

```powershell
dotnet restore
dotnet build Xtate.Core.sln
dotnet test Xtate.Core.sln
```

For a focused loop:

```powershell
dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj -f net10.0 --filter "FullyQualifiedName~InterpreterTest"
```

Use `-p:SkipNetFrameworkTests=true` when the local environment cannot run `net462`. Build legacy targets when changing polyfills or compatibility-sensitive APIs.

The other Markdown files in `.agents` are focused coverage plans and trackers. Update them only when the associated test-planning task or recorded coverage changes.

## Change checklist

1. Trace effects across the public model, compiled model, DI registration, and persistence as applicable.
2. Add or update the narrowest matching regression test for behavior changes.
3. Run a focused test first, then the relevant solution build/test command.
4. Keep generated files and unrelated existing work untouched.
5. Update documentation only when public usage, architecture, commands, or navigation guidance changes.
