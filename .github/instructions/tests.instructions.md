---
applyTo: "test/Xtate.Core.Test/**/*.cs"
---

# Test project instructions

## Purpose

This area contains MSTest coverage for interpreter behavior, DI wiring, hosted flows, SCXML/XInclude, and legacy compatibility scenarios.

## Follow existing patterns

- Use MSTest attributes (`[TestClass]`, `[TestMethod]`) and async tests returning `Task`.
- Build test service providers with Xtate.IoC (`new ServiceCollection()`, `AddModule<...>()`, `BuildProvider()`).
- Resolve services with `await provider.GetRequiredService<...>()`.
- Use Moq for collaborators and verify structured logging behavior where applicable.

## Implementation rules

- Keep tests close to the feature area (`DI`, `Interpreter`, `HostedTests`, `UnitTests`, `Legacy`).
- Reuse helpers like `HostedTestBase` and `ServiceCollectionExtensions` instead of duplicating setup.
- For hosted SCXML scenarios, keep resource/URI patterns consistent with existing embedded-resource tests.

## Testing rules

- Add regression tests for any production behavior change in `src/Xtate.Core`.
- Prefer deterministic assertions over console output/manual inspection.

## Avoid

- Do not switch tests to Microsoft DI APIs.
- Do not rely on fragile timing-based assertions when event/state assertions are available.
