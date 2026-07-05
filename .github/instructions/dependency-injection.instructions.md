---
applyTo: "src/Xtate.Core/**/DependencyInjection/**/*.cs"
---

# Dependency injection module instructions

## Purpose

This area wires feature modules and service registrations for Xtate.IoC.

## Follow existing patterns

- Register dependencies through `Module` subclasses and `Services.AddModule<...>()` composition.
- Keep DI types container-instantiated with `[InstantiatedByIoC]` when they are service entry points.
- Prefer `required` init-properties with `[SetByIoC]` over constructor-based injection.
- Keep async/sync factory registrations aligned with existing `AddTypeSync`, `AddImplementationSync`, and forwarding patterns.

## Implementation rules

- Preserve module dependency ordering when adding new module references.
- Use Xtate.IoC APIs (`BuildProvider`, forwarding, options modules), not Microsoft DI helpers.
- Keep registration files focused on wiring; move business logic to service classes.

## Testing rules

- Add/adjust tests in `test/Xtate.Core.Test/DI` when registration behavior changes.
- Resolve services in tests via `await provider.GetRequiredService<...>()`.

## Avoid

- Do not introduce `BuildServiceProvider()` or constructor injection conventions from MS.DI.
- Do not mix unrelated runtime behavior into DI registration modules.
