# Xtate.Core Full-Coverage Agent Prompt

Use the following prompt for an agent working from the repository root.

---

You are responsible for completing unit-test coverage of the production `Xtate.Core` assembly.

## Authoritative inputs

Read these files before changing code:

1. `AGENTS.md` — repository architecture, commands, conventions, and investigation routes.
2. `test/Xtate.Core.Test/StateMachines/UNIT_TEST_COVERAGE_AGENT.md` — the newest method-level coverage report and primary worklist.
3. `src/Xtate.Core/UnitTestCoverage.Tracker.md` — historical class-level progress and notes about tests and known regressions.
4. The production source and existing tests for only the subsystem currently being covered.

Treat the method-level report as the authoritative starting snapshot. Do not assume a method is covered merely because the class tracker says tests were added. Coverage must be demonstrated by a refreshed report.

## Objective and completion criteria

Drive the `Xtate.Core` production assembly to full attainable unit-test coverage:

- 100% line coverage.
- 100% block/branch coverage.
- No unchecked reachable production method in `UNIT_TEST_COVERAGE_AGENT.md`.
- No test failures other than deliberately retained `[Ignore]` regression tests.
- No newly introduced compiler, analyzer, build, or test-runner warning or informational diagnostic. Existing environment diagnostics such as the already-known preview-SDK `NETSDK1057` message are baseline, not new diagnostics.

Do not declare completion from a focused test run or from the absence of obvious failures. Completion requires a freshly generated assembly coverage report proving the final line and block percentages and an audited worklist containing no reachable unchecked item.

If a branch is genuinely unreachable because of compiler/framework behavior, document exact evidence in the worklist. Do not exclude ordinary production code, add broad coverage filters, or use `[ExcludeFromCodeCoverage]` on production code to manufacture the target.

## Mandatory testing rules

- Test production behavior in `src/Xtate.Core`; do not add tests merely to cover test-project helpers.
- Search existing tests by production type and method before creating a new fixture. Extend the narrowest suitable fixture when practical.
- Prefer dedicated `[TestMethod]` methods with descriptive names. Do not replace distinct scenarios with one large sequential or dynamic runner.
- Cover applicable success, empty/default, boundary, invalid-input, exception, cancellation, disposal, repeated-disposal, concurrency, state-transition, persistence, and async completion/failure paths.
- For compiler-generated async, iterator, local-function, and lambda entries, test their source-level method behavior. Never target generated `MoveNext()` methods directly.
- Use deterministic fakes, in-memory storage, and loopback transports. Do not depend on public network services, timing races, or machine-specific state.
- Preserve compatibility patterns and existing behavior. Do not edit production code solely to make coverage easier.
- Do not remove or weaken an existing assertion to obtain a passing run.
- If a newly added test exposes a real product defect, keep the test. Add `[Ignore("detailed product-defect reason")]` and a nearby comment explaining expected behavior, actual behavior, and what production change would allow the test to be enabled.
- Correct mistakes in test setup or incorrect expectations normally; `[Ignore]` is only for genuine product behavior defects.
- Every callback passed to `Assert.ThrowsExactly...` or `Assert.ThrowsExactlyAsync...` must carry `[ExcludeFromCodeCoverage]`, for example:

  ```csharp
  Assert.ThrowsExactly<ArgumentNullException>(
      [ExcludeFromCodeCoverage] () => operation(argument: null!));
  ```

- Do not delete ignored regression tests after discovering that they fail.
- Preserve unrelated user changes and generated files. Use `apply_patch` for source edits.

## Work order

Repeat the following loop until the completion criteria are proven:

1. Parse the current report summary and unchecked worklist.
2. Select a coherent production cluster, prioritizing:
   - P0 methods and types with substantial uncovered production code.
   - P1 items.
   - P2 items.
   - P3 partial branches and final boundary cases.
3. Inspect only that subsystem's production source, DI module when relevant, and existing tests.
4. Derive concrete missing scenarios from the uncovered source branches. Do not guess solely from percentages.
5. Add as many cohesive tests as practical in the batch.
6. Run the narrowest focused test filter on `net8.0`.
7. Fix test defects. Retain genuine product defects using the required ignored-regression format.
8. Run a combined filter for every fixture changed in the batch.
9. Audit all new exact-exception callbacks for `[ExcludeFromCodeCoverage]`.
10. Check the build/test output against the diagnostic baseline and remove every newly introduced warning or informational message.
11. Regenerate coverage using the same coverage configuration that produced `UNIT_TEST_COVERAGE_AGENT.md`. If that exact generation command is unavailable, first locate the repository/user-provided coverage artifact or tooling; do not silently substitute a different metric and compare incompatible percentages.
12. Update `UNIT_TEST_COVERAGE_AGENT.md` from the refreshed production-assembly report:
    - Mark an item complete only when both its line and block coverage are 100%.
    - Keep partial items unchecked with their new exact percentages and counts.
    - Record ignored regressions or proven unreachable branches explicitly.
    - Update the assembly summary and priority totals.
13. Synchronize `src/Xtate.Core/UnitTestCoverage.Tracker.md` with verified class-level results and reconcile all summary counts with the table.
14. Continue immediately with the next cluster; do not stop merely because one batch passes.

## Verification commands

Use the repository's current target framework and start with focused runs:

```powershell
dotnet test test\Xtate.Core.Test\Xtate.Core.Test.csproj -f net8.0 --filter "FullyQualifiedName~TargetFixture" --no-restore --verbosity minimal
```

After a successful build, use `--no-build --no-restore` for fast combined verification. Before final handoff, run the complete test project and, when practical, the solution:

```powershell
dotnet test test\Xtate.Core.Test\Xtate.Core.Test.csproj -f net8.0 --no-restore --verbosity minimal
dotnet test Xtate.Core.sln --no-restore --verbosity minimal
```

Also run:

```powershell
git diff --check
```

If a test hangs, terminate only the test process created by that run, retain the scenario as an ignored regression with a detailed nontermination explanation, and verify that no `testhost` process remains before the next build.

## Progress reporting

After each batch report:

- Test files created or changed.
- Production classes and method branches exercised.
- Passing, failed, and ignored counts.
- Newly discovered retained regressions.
- Refreshed assembly line and block coverage.
- Remaining P0/P1/P2/P3 and total unchecked method counts.
- Diagnostic audit result.

Keep working autonomously while reachable coverage work remains. Ask the user only when completion requires missing external input or authority that cannot be discovered from the repository.

## Final audit

Before claiming completion, prove all of the following from current artifacts:

- The refreshed report is for `Xtate.Core.dll`, not the test assembly.
- Assembly line and block coverage both meet the target.
- Every reachable worklist item is complete.
- Every ignored test has a detailed defect reason and explanatory comment.
- Every new `Assert.ThrowsExactly...` callback is coverage-excluded.
- The full required test runs pass.
- No new warning or informational diagnostic exists.
- `git diff --check` reports no whitespace errors.
- Both coverage tracking Markdown files agree with the refreshed report.

If any evidence is missing or only inferred, the task is not complete; continue working.

---
