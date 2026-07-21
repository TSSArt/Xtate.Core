---
applyTo: "test/**/*.cs"
---

# Test source instructions

## Test style

- Use MSTest attributes and the assertion style already used by nearby tests.
- Keep tests focused, deterministic, independent, and safe under parallel execution.
- Reuse existing fixtures, `HostedTestBase`, service-collection helpers, and embedded resources.
- Compose services with Xtate.IoC and resolve asynchronous services with the existing async APIs.

## Coverage

- Place tests in the closest feature area: `DI`, `Interpreter`, `HostedTests`, `Scxml`, `StateMachines`, `UnitTests`, `Persistence`, or `Legacy`.
- Cover the public model, compiled model, runtime behavior, registration, and persistence dimensions affected by the change.
- Prefer state, event, result, and structured-log assertions over sleeps, timing assumptions, console output, or manual inspection.
- Keep SCXML fixtures minimal and name them for the behavior under test.

## Verification

- Run the narrowest matching test filter on one modern framework first.
- Run broader solution tests and legacy targets when shared or compatibility-sensitive behavior changes.
