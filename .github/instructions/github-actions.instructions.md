---
applyTo: ".github/workflows/**/*.{yml,yaml}"
---

# GitHub Actions instructions

## Preserve workflow intent

- Keep `publish.yml` tag/manual publishing, Windows legacy-framework tests, package creation, and feed publishing behavior.
- Keep `codeql.yml` security analysis for pushes, pull requests, schedule, and manual runs.
- Keep `publish-dependencies.yml` registry-package dispatch behavior and repository matrix.
- Preserve restore, build, test, and pack ordering and the existing `--no-restore` / `--no-build` chaining.

## Security and compatibility

- Keep permissions minimal and secrets scoped to the steps that require them.
- Reuse existing secret names and package feeds; never add plaintext credentials.
- Preserve .NET SDK and Mono setup needed by the multi-targeted solution.
- Pin actions to explicit major versions consistent with the repository.

## Verification

- Validate YAML syntax and expressions.
- Check that job dependencies, artifacts, package paths, triggers, and permissions still match the intended release flow.
