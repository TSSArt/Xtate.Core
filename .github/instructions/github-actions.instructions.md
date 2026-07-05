---
applyTo: ".github/workflows/*.{yml,yaml}"
---

# GitHub Actions workflow instructions

## Purpose

This area defines publish automation and dependency-triggered cross-repository dispatch.

## Follow existing patterns

- Keep release publishing triggered by version tags (`v*`) and manual dispatch.
- Preserve the restore → build → test → pack pipeline with `--no-restore`/`--no-build` chaining after restore/build.
- Keep .NET setup and Mono installation aligned with multi-target support (including `net462`).
- Reuse existing secret names (`XTATE_GITHUB_TOKEN`, `NUGET_ORG_TOKEN`) and package feeds.

## Implementation rules

- Keep workflow permissions minimal (`contents: read` unless broader access is required).
- Preserve matrix-based dependency dispatch structure in `publish-dependencies.yml`.
- Pin third-party actions to explicit major versions as currently done.

## Testing rules

- Ensure workflow edits remain syntactically valid YAML and consistent with current job names/inputs.
- Keep references to existing repository names and workflow names when dispatching.

## Avoid

- Do not introduce new secrets, tokens, or external endpoints without a repository requirement.
- Do not remove steps required for version calculation, test execution, or package publishing.
