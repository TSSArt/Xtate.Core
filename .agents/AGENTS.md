# Xtate.Core repository guide

Use this file as the first source of project context. Inspect only the subsystem relevant to the requested change; do not rescan the entire repository unless this guide is demonstrably stale.

## Purpose and shape

Xtate.Core is a C# state-machine framework centered on W3C SCXML. The repository contains one production library and one MSTest project:

- `src/Xtate.Core/Xtate.Core.csproj` — package/library; root namespace `Xtate`.
- `test/Xtate.Core.Test/Xtate.Core.Test.csproj` — unit, integration, hosted, persistence, and SCXML conformance-style tests.
- `Xtate.Core.sln` — the solution to build and test.

The library targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, and `net462`. Tests target all of those except `netstandard2.0`. Compatibility code for older targets is under `Common/Polyfills`; do not remove it merely because a modern target supplies the API.

## Runtime flow

The main processing path is:

1. A state-machine source is supplied by a class in `Class/DependencyInjection` (`ScxmlStringStateMachine`, `ScxmlStreamStateMachine`, `LocationStateMachine`, or `RuntimeStateMachine`).
2. SCXML input is loaded/deserialized by `Scxml` and built into the public state-machine object model in `StateMachine`.
3. `Interpreter/ModelBuilder` validates and compiles that object model into interpreter nodes.
4. `Interpreter/Services/StateMachineInterpreter.cs` executes the model and communicates through queues/controllers.
5. `StateMachineHost` manages scopes, lifecycle, event routing, scheduled events, invoked services, and security context.
6. `Persistence` optionally wraps interpreter state and collections with transactional storage.

Composition uses **Xtate.IoC**, not Microsoft.Extensions.DependencyInjection. Features are registered through `Module` subclasses and `IServiceCollection.AddModule<T>()`. Service resolution is commonly asynchronous (`GetRequiredService<T>()` returns/participates in async construction); preserve the existing lifetime choices (`SharedWithin.Scope`, `SharedWithin.Container`, etc.). Start dependency tracing at the subsystem's `DependencyInjection/*Module.cs` file.

## Subsystem map

| Area | Responsibility | Start here | Tests |
|---|---|---|---|
| `Actions` | Built-in system actions such as start/destroy | `Actions/Abstractions`, `Actions/System` | `HostedTests`, `DevTests` |
| `Class` | User-facing state-machine source wrappers and service registration | `Class/DependencyInjection/StateMachineClass.cs` | `RegisterClassTest.cs`, `StateMachineFactory` |
| `Common` | Shared helpers, logging, task monitoring, name tables, compatibility polyfills | the relevant child folder | `UnitTests/Common` |
| `DataTypes` | Dynamic SCXML values, numbers, dates, list/object adapters | `DataTypes/Model/Types/DataModelValue.cs` | root `DataModel*Test.cs`, `UnitTests/DataModel` |
| `DataModel` | Data-model contracts, action evaluators, runtime/null/XPath handlers | `DataModel/DependencyInjection/DataModelHandlersModule.cs` | `RegisterClassTest.cs`, XPath tests, `StateMachines/DataModel` |
| `ExternalServices` | HTTP/SMTP client abstractions and implementations | each handler's `DependencyInjection` module | targeted tests where present |
| `Http` | HTTP request/response helpers | `Http/DependencyInjection/HttpModule.cs` | IO processor tests |
| `Interpreter` | Executable model, queues, execution algorithm, controllers | `Interpreter/Services/StateMachineInterpreter.cs`; `Interpreter/DependencyInjection/StateMachineInterpreterModule.cs` | `Interpreter`, `StateMachines`, `DevTests/InvokeTest.cs` |
| `IoC` | Xtate-specific composition helpers, options, arrays, transforms, ancestor tracking | `IoC/DependencyInjection/IoCModule.cs` | `DI`, `RegisterClassTest.cs` |
| `IoProcessors` | Named-pipe and HTTP event transports | handler `DependencyInjection` modules | `UnitTests/IoProcessors` |
| `Persistence` | Storage abstractions, serialization, persisted interpreter/collections | `Persistence/DependencyInjection/PersistenceModule.cs` | `DevTests/*Storage*`, `*Persistence*`, `Legacy` |
| `ResourceLoaders` | File, embedded-resource, and web URI loading | `ResourceLoaders/DependencyInjection/ResourceLoadersModule.cs` | `XIncludeTest.cs`, `RegisterClassTest.cs` |
| `Scxml` | XML reader/director, serialization/deserialization, XInclude | `Scxml/DependencyInjection/ScxmlModule.cs` | `XIncludeTest.cs`, `RegisterClassTest.cs` |
| `StateMachine` | Public SCXML object model, builders, validation, visitors | `StateMachine/Abstractions`, `StateMachine/Builder`, `StateMachine/Validator` | `StateMachines`, `RegisterClassTest.cs` |
| `StateMachineFluentBuilder` | Fluent construction API over builders | `StateMachineFluentBuilder/Abstractions` | `Interpreter/FluentBuilderTest.cs`, `DevTests/StateMachineFluentBuilderTest.cs` |
| `StateMachineHost` | Lifecycle, scopes, routing, scheduling, external services, security | `StateMachineHost/DependencyInjection/StateMachineProcessorModule.cs` | `HostedTests`, `UnitTests/FinalStateTest.cs`, persistence tests |
| `StateMachineOptions` | Options model/provider/registration | all three files in the folder | `RegisterClassTest.cs` |

Important model distinction: `StateMachine` contains the source/public entity graph; `Interpreter/ModelBuilder/Model` contains the compiled executable node graph. A parsing or public-model change often needs corresponding builder, validator, interpreter-model, and persistence consideration.

## Efficient investigation routes

- Public API or source creation: inspect `Class`, then the module it registers.
- SCXML element/attribute behavior: inspect `Scxml/Services/ScxmlDirector.cs`, the matching `StateMachine` entity/abstraction, validator, and model-builder visitor.
- Execution semantics: inspect the matching node under `Interpreter/ModelBuilder/Model` and the algorithm in `Interpreter/Services/StateMachineInterpreter.cs`.
- Expression/action behavior: inspect `DataModel/Services/Evaluators`, then the handler-specific evaluator in `DataModel/Handlers/{Null|Runtime|XPath}`.
- Dependency-resolution failure: inspect the closest `DependencyInjection/*Module.cs`, then its generic base-module list and registration lifetime.
- Event routing/lifecycle: inspect `StateMachineHost/Services`, starting from `StateMachineScopeManager`, `StateMachineRuntimeController`, or the relevant dispatcher/router.
- Persistence issue: inspect the relevant `*PersistingController`, `StateMachineReader`, and storage implementation; also check `DataModelReferenceTracker` for object identity/reference behavior.
- Regression coverage: search the test project by the production type name first. The `DevTests` and `Legacy` folders still contain useful behavioral coverage despite their names.

## Build and test

From the repository root:

```powershell
dotnet build Xtate.Core.sln
dotnet test Xtate.Core.sln
```

For a faster focused loop, select one installed modern framework and a filter:

```powershell
dotnet test test\Xtate.Core.Test\Xtate.Core.Test.csproj -f net10.0 --filter "FullyQualifiedName~InterpreterTest"
```

When changing compatibility-sensitive code, build the library for `netstandard2.0` and/or `net462` as appropriate. When changing shared behavior, run the focused test first and the solution test suite before handoff when practical.

`test/Xtate.Core.Test/StateMachines/SCXML_TEST_COVERAGE.md` records the state-machine scenario coverage. Update it when adding or changing categorized SCXML behavior tests.

## Code conventions and hazards

- C# language version is `preview`; nullable reference types and .NET analyzers are enabled.
- Indentation is tabs, width 4. Follow `.editorconfig` and nearby style.
- Files carry an AGPL copyright header. Match the current year/style in adjacent files when adding production C# files.
- Async library calls generally use `ConfigureAwait(false)`; the analyzer package enforces this broadly.
- Global usings live in `src/Xtate.Core/Properties/GlobalUsings.cs` and `test/Xtate.Core.Test/GlobalUsings.cs`.
- Many services use property injection or static async factory methods because Xtate.IoC constructs them. Do not casually convert these to constructors.
- Registration order, `Option.IfNotRegistered`, sharing scope, and forwarding registrations are behavioral. Preserve them unless the task explicitly changes composition.
- `Directory.Build.props` and `Global.Packages.props` say they are autogenerated. Avoid editing them directly. Project-specific dependency versions belong in `Directory.Packages.props` unless the repository generation workflow requires otherwise.
- Package versions are centrally managed; omit versions from `PackageReference` entries.
- `Version.props` defaults to `0.0.0` and is normally overridden by release automation.
- Generated resource code is `Properties/Resources.Designer.cs`; edit `Resources.resx`, not the generated file.
- Ignore `bin`, `obj`, `TestResults`, and IDE metadata during analysis.

## Change checklist

Before finishing a code change:

1. Identify whether it affects the public entity graph, compiled interpreter graph, DI registration, and persistence format—not just the immediately edited class.
2. Add or update the narrowest matching test; search for the production type and analogous behavior.
3. Build/test a modern target, plus legacy targets if the touched API differs by framework.
4. Keep generated files and unrelated existing work untouched.
5. Update this guide only when architecture, commands, or navigation advice materially changes.
