---
applyTo: "src/Xtate.Core/Common/Polyfills/**/*.cs"
---

# Polyfills instructions

## Purpose

This area provides compatibility shims for lower target frameworks (netstandard2.0/net462 and older runtime feature gaps).

## Follow existing patterns

- Guard each polyfill with precise `#if` target checks matching API availability.
- Keep polyfills in BCL namespaces (`System.*`) only when required to mirror missing framework APIs.
- Match existing signatures and behavior of framework APIs as closely as possible.

## Implementation rules

- Add new polyfills only when a target framework in `Xtate.Core.csproj` is missing the API.
- Prefer small, isolated files per API family to keep conditional compilation clear.
- Keep async/cancellation behavior consistent with modern API semantics.

## Testing rules

- Add/adjust tests that exercise affected behavior through public Xtate code paths.
- Validate that newer targets still compile without polyfill conflicts.

## Avoid

- Do not change behavior for frameworks where native APIs already exist.
- Do not remove or broaden preprocessor guards without target-framework evidence.
