# Xtate.Core

Xtate.Core is a multi-targeted .NET state-machine framework centered on W3C SCXML. It provides SCXML parsing and serialization, programmatic and fluent state-machine builders, an asynchronous interpreter, host/event routing, pluggable data models (including XPath), resource loaders, I/O processors, and optional persistence.

The repository contains:

- `src/Xtate.Core` — the `Xtate.Core` library/package.
- `test/Xtate.Core.Test` — MSTest unit, integration, persistence, and SCXML behavior coverage.
- `AGENTS.md` — the architecture map, subsystem entry points, conventions, and efficient investigation routes for maintainers and coding agents.

## Build and test

```powershell
dotnet build Xtate.Core.sln
dotnet test Xtate.Core.sln
```

For focused development, target one installed framework and filter by test name:

```powershell
dotnet test test\Xtate.Core.Test\Xtate.Core.Test.csproj -f net10.0 --filter "FullyQualifiedName~InterpreterTest"
```

See [AGENTS.md](AGENTS.md) before making changes; it is designed to identify the smallest production and test file set needed for a task.

## License

Xtate.Core is licensed under the GNU Affero General Public License v3.0 or later. See [LICENSE](LICENSE).
