# Xtate.Core Copilot instructions

## Repository at a glance

Xtate.Core is the central .NET implementation of the Xtate SCXML state-machine framework. The production project is `src/Xtate.Core/Xtate.Core.csproj`; tests are in `test/Xtate.Core.Test`.

Read [`.agents/AGENTS.md`](../.agents/AGENTS.md) for the repository map and investigation routes. Apply every matching file in [`.github/instructions`](instructions); those rules are more specific than this guide.

## Working approach

1. Inspect the closest production type, its abstraction, its `DependencyInjection/*Module.cs`, and tests that mention the type.
2. Trace model changes across the public `StateMachine` graph, validator, `Interpreter/ModelBuilder` graph, runtime execution, and persistence as applicable.
3. Make the smallest coherent change and preserve generated files and unrelated work.
4. Add or update a focused regression test for behavior changes.
5. Run the narrowest useful test before broader build/test commands.

## Build and test

```powershell
dotnet restore
dotnet build Xtate.Core.sln
dotnet test Xtate.Core.sln
```

Focused example:

```powershell
dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj -f net10.0 --filter "FullyQualifiedName~InterpreterTest"
```

The library targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, and `net462`. Tests target `net10.0`, `net9.0`, `net8.0`, and optionally `net462`. Use `-p:SkipNetFrameworkTests=true` when the environment cannot run .NET Framework tests.

## Shared coding rules

- Follow `.editorconfig`; C# uses tabs, nullable annotations, analyzers, and preview language features.
- Match the AGPL header and current style in adjacent source files.
- Use Xtate.IoC, `Module` composition, and Xtate service-resolution APIs; do not substitute Microsoft DI conventions.
- Preserve registration order, lifetimes, forwarding, `Option.IfNotRegistered`, property injection, and async factory behavior.
- Preserve `ValueTask` and `ConfigureAwait(false)` patterns used by library code.
- Keep compatibility shims precisely guarded for older targets.
- Keep package versions in `Directory.Packages.props` and omit versions from project `PackageReference` items.
- Treat `Directory.Build.props`, `Global.Packages.props`, and `Resources.Designer.cs` as generated. Edit `Resources.resx` instead.
- Ignore `bin`, `obj`, `TestResults`, and IDE metadata.

## Architecture guardrails

- `StateMachine` is the public/source model; `Interpreter/ModelBuilder/Model` is the compiled runtime model.
- `Scxml` parses and serializes; it delegates object construction and validation to the state-machine layer.
- `StateMachineHost` owns runtime orchestration above the interpreter.
- `Persistence` mirrors interpreter concepts and may require coordinated updates when executable state changes.
- Resource loading must continue through `IResourceLoader` abstractions.
- Async runtime paths must not introduce blocking I/O or synchronous waits.

## Tests and documentation

- Use MSTest and existing helpers, fixtures, embedded resources, and Xtate.IoC setup patterns.
- Prefer deterministic state/event assertions over timing or console output.
- Update `.agents` coverage trackers only when their recorded plan or coverage actually changes.
- Update README or repository guidance when public usage, supported targets, commands, or architecture changes.

## Before finishing

Confirm that relevant focused tests pass, generated files were not hand-edited, compatibility targets were considered, and only files required by the task changed.
