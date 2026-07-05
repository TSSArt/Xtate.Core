---
applyTo: "src/Xtate.Core/Scxml/**/*.cs"
---

# SCXML instructions

## Purpose

This area parses, validates, and serializes SCXML and handles XInclude processing.

## Follow existing patterns

- Keep XML operations async-capable (`ReadAsync`, async `ValueTask`, async `XmlReader`/`XmlWriter` settings).
- Continue using IoC-injected collaborators (`IXIncludeOptions`, `XmlResolver`, directors/readers, validators).
- Validate parsed state machines through `IStateMachineValidator` after construction.
- Use existing SCXML exceptions (for example `XIncludeException`) for reader-context-aware failures.

## Implementation rules

- Respect `IXIncludeOptions` gates before enabling include processing.
- Preserve namespace/name table based comparisons and existing reader delegation flow.
- Keep resource loading through existing `Resource`/`IResourceLoader` abstractions.

## Testing rules

- Extend `test/Xtate.Core.Test/XIncludeTest.cs` or nearby SCXML tests when parser/reader behavior changes.
- Cover both include-enabled and include-disabled paths where relevant.

## Avoid

- Do not bypass validator checks in deserialization flows.
- Do not introduce blocking XML APIs in async paths.
