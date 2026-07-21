---
applyTo: "src/**/*.cs"
---

# C# source instructions

## Style and compatibility

- Follow `.editorconfig`: tabs, nullable annotations, analyzer rules, using order, and existing naming conventions.
- Match the AGPL header and current style in adjacent source files.
- Preserve every target framework. Guard compatibility code precisely and do not assume APIs available only on modern .NET.
- Preserve `ValueTask` and `ConfigureAwait(false)` patterns used by async library code.

## Architecture

- Use Xtate.IoC modules and Xtate service APIs, not Microsoft DI conventions.
- Keep abstractions, implementations, composition modules, and persistence responsibilities separated.
- Preserve registration order, lifetime, ownership, forwarding, and `Option.IfNotRegistered` behavior.
- Keep the public state-machine graph distinct from the compiled interpreter graph.

## Generated and dependency files

- Edit `.resx` sources instead of generated resource designer files.
- Keep dependency versions in `Directory.Packages.props` and omit versions from `PackageReference` items.
- Do not edit generated build-property files or build output.

## Verification

- Add or update the narrowest matching test for behavior changes.
- Build the affected modern target and relevant compatibility targets.
