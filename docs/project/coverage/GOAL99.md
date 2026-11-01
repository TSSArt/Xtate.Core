# 99% line coverage verified

Measured 2026-11-01 with coverlet.collector, Debug, Xtate.Core only. Every tested target framework exceeds 99% line coverage using the exact covered/valid line counts. No coverage exclusions were added. Branch coverage is reported separately and remains below 99%.

| Framework | Covered / valid lines | Line coverage | Branch coverage | Passing tests | Failed / ignored |
| --- | ---: | ---: | ---: | ---: | ---: |
| net10.0 (primary) | 21,521 / 21,736 | 99.01% | 93.73% | 4,724 | 0 / 0 |
| net11.0 | 21,529 / 21,736 | 99.04% | 93.78% | 4,724 | 0 / 0 |
| net9.0 | 21,521 / 21,736 | 99.01% | 93.73% | 4,724 | 0 / 0 |
| net8.0 | 21,520 / 21,735 | 99.01% | 93.73% | 4,724 | 0 / 0 |
| net462 | 21,789 / 22,008 | 99.00% | 93.44% | 4,723 | 0 / 0 |

The original .NET 10 baseline was 20,311 / 21,661 lines (93.76%), 88.85% branches, and 4,081 passing tests. Current coverage adds 1,210 covered lines and 643 passing cases. Production defect fixes made during this work changed the denominator; no production code was excluded to reach the target.

## Reproduce

```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj --no-restore -p:TestTfmsInParallel=false --collect:'XPlat Code Coverage' --results-directory TestResults/goal-final17 --logger 'trx;LogFilePrefix=full'
```

All five full suites passed. Run frameworks sequentially to avoid concurrent testhost startup load and competition for loopback listener ports. The HTTP receive fixture also retries HTTP.sys prefix conflicts when a TCP-selected port is claimed before listener registration.

The primary report is `TestResults/goal-final17/ser_apex_2026-11-01_19_19_15/In/apex/coverage.cobertura.xml`; its test result is `TestResults/goal-final17/full_net10.0_20261101191919.trx`. Exact paths and results for every framework are recorded in `remaining.json`. TestResults is ignored by Git. The original baseline remains in `TestResults/goal-baseline/192e22fa-60d5-4939-8773-4cbd5badddb6/coverage.cobertura.xml`.

## Regression retention

Defect-revealing tests were retained and marked `[Ignore]` after reproducing their failures. Subsequent production fixes, including commits `fe248c1e` and `9c5dd395`, repaired the defects and restored these tests. The final suite has no ignored tests.

Retained cases include key-list restoration, mixed XML text, XPath mutation and namespace traversal, fragmented named-pipe reads, incompatible XML policies, async XInclude, late binding, nested parallel completion, persistent-channel FIFO ordering, absent multivalue removal, and replacement of a sole top-level XPath element. Their expected behavior was preserved through the fixes.

## Added coverage

- Resource catalog presence, formatting, culture fallback, and public value conversions.
- XPath compilation, validation, typed conversion, navigation, namespace scope, mutation, normalization, and assignment.
- Interpreter lifecycle, transition conflicts, invocation failures, error classification, finalization, cleanup order, and large internal-queue progress.
- Persisted operation replay, storage rollback and compaction, malformed records, model replacement, shared references, scheduler failure recovery, and channel handoff ordering.
- HTTP receive formats, routing and malformed requests; named-pipe framing, size limits, response timestamps, invalid wire codes, and host shutdown.
- SCXML validation, namespace caching, inline serialization, XML callback failures, and XInclude error/fallback behavior.
- Legacy span formatting, cancellation token preservation, suppressed execution-context waits, and HTTP handler expiration and disposal.

## Remaining gaps

`remaining.json` lists uncovered sequence-point candidates for the primary framework. Some lines contain both covered and uncovered sequence points. Remaining gaps include defensive invariant/type defaults and scheduler concurrency or infrastructure failure paths. Inspect the XML and source before treating a candidate as an entirely unexecuted line. The 99% line target is verified; this report does not claim 99% branch coverage or execution of netstandard assemblies independently of the tested targets.
