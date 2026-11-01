# Coverage toward 99%

Measured 2026-11-01 with coverlet.collector, Debug, no coverage exclusions added.
The 99% objective remains active and has not yet been reached.

| Metric (.NET 10, Xtate.Core only) | Baseline | Current |
| --- | ---: | ---: |
| Covered lines | 20,311 / 21,661 | 21,238 / 21,661 |
| Line coverage | 93.76% | 98.04% |
| Branch coverage | 88.85% | 92.11% |
| Passing tests | 4,081 | 4,574 |
| Ignored regression cases added | 0 | 9 |

207 additional covered lines are required for 99% line coverage with the current denominator. Branch coverage is tracked separately and also remains below 99%. Ignored tests do not count as executed coverage. Tests include focused unit checks, interpreter integration checks, and loopback HTTP integration checks.

## Verification

Full suites passed on net8.0, net9.0, net10.0, and net11.0: 4,574 passed and 9 skipped per framework. net462: 4,572 passed and 9 skipped. Earlier builds emitted an existing nullable warning in StateMachineDestroyOnIdle; transient testhost file locks resolved on retry.

Command (PowerShell):

```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj --no-restore --collect:'XPlat Code Coverage' --results-directory TestResults/goal-latest2 --logger 'trx;LogFilePrefix=full'
```

The .NET 10 report is `TestResults/goal-latest2/ser_apex_2026-11-01_15_59_12/In/apex/coverage.cobertura[3].xml`, also present in one of the collector GUID directories. Its TRX is `TestResults/goal-latest2/full_net10.0_20261101155917.trx`. The original baseline is `TestResults/goal-baseline/192e22fa-60d5-4939-8773-4cbd5badddb6/coverage.cobertura.xml`. TestResults is ignored by Git.

## Retained failing regressions

Each was run and failed before receiving `[Ignore]`. Production logic was left unchanged.

| Test | Observed defect | Reproduction TRX |
| --- | --- | --- |
| PersistenceCollectionControllerCoverageTest.KeyListControllerRestoresListItemsInOrder | Stored items restore as an empty list; restore loop uses the new list's zero Count. | goal-batch1/batch1.trx |
| XPathObjectCoverageTest.MixedElementAndTextSelectionPreservesTextEntries | XmlConverter.FromXml discards accumulated child elements when encountering mixed text. | goal-xpath-object/object.trx |
| XPathNavigatorMutationCoverageTest.DeletingLastNestedElementNormalizesParentToUndefined | Deleting the last nested child leaves it present after normalization reaches the root. | goal-mutation2/mutation.trx |
| NamedPipeFramingCoverageTest.FragmentedFrameIsReadUntilRequestedLength | Four bytes delivered in two-byte chunks return only two; remaining length and offset are both subtracted. | goal-pipe/pipe.trx |
| XmlDirectorCoverageTest.IncompatibleOptionalElementAndRawContentPoliciesAreRejected (two cases) | OptionalElement omits UseRawContent(false), allowing incompatible policies in either registration order. | goal-xml-policy/policy.trx |
| XPathNavigatorMutationCoverageTest.InheritedNamespaceIsVisibleOnDescendantAndReturnsToThatDescendant | MoveToParent after an inherited namespace returns the declaring ancestor instead of the original descendant. | goal-xpath-root/root.trx |
| XPathNavigatorMutationCoverageTest.NamespaceLookupFindsAncestorBindingWithoutMovingOriginalNavigator | GetNamespace changes the original navigator's position while resolving an inherited namespace. | goal-xpath-root/root-verified.trx |
| XIncludeReaderCoverageTest.AsyncXmlAsTextPreservesMarkupAndIgnoresNamespacedExtensionAttributes | XML-as-text inclusion creates a reader without Async=true and then calls ReadAsync. | goal-xinclude-async/async.trx |

TRX paths above are relative to TestResults. Preserve these tests and their expected behavior until the corresponding logic is repaired.

## Coverage added

- Resource catalog presence and numeric-placeholder formatting (326 resources).
- XPath expression compilation, validation, typed conversions, deep navigation, mutation, ordering, copied insertion, and scalar normalization.
- HTTP receive decoding, metadata routing, malformed content, size limits, and dispatch failure responses.
- Memory stream span/memory operations, cancellation, disposal, and persisted content.
- Interpreter error event classification, logger failure recursion bounds, invoke filtering, and forwarding failures.
- Persisted operation replay, transition node lookup, checkpoint policy, corrupt execution state, and duplicate side-effect prevention.
- External-service add checkpoint rollback and retry; corrupt operation rejection.
- Persistent scheduler add/cancel rollback and channel read checkpoint rollback.
- Named-pipe complete/truncated frame reads and XInclude resolver fallback/error handling.
- Invalid empty SCXML expression attributes.
- Parallel transition conflict precedence in both region orders and default entry of untargeted parallel regions.
- Data, done-data, and global-script evaluation failure handling; parsed expression namespace shadowing.
- XML callback failure reporting and incompatible policy registration guards.
- Persisted service start failures, suspension retention, compaction, removal rollback, and cleanup failures.
- XPath unnamed-root mutations and multi-node assignment; scalar object graph conversion with shared references.
- List removal around locked entries and metadata-preserving read-only insertion.
- Scheduler cancellation failure aggregation, concurrency/timeout limits, and lazy-task late failure observation after cancellation.

## Next gaps

Prioritize remaining interpreter lifecycle/error paths, XPath handler assignment validation, collection edge cases and concurrency retries, and resource/transport error paths. Scheduler infrastructure failures and concurrency retry loops need reachability analysis and deterministic tests; avoid timing-dependent coverage attempts. Some unmatched enum/type defaults and invariant guards are unreachable through valid construction. Key-list restoration loop coverage and async XML-as-text inclusion coverage depend on retained ignored logic defects.

See `remaining.json` for source line candidates. A line can have both covered and uncovered sequence points; inspect the XML and source before treating each candidate as an entirely unexecuted line. Do not change the denominator or exclude production code to satisfy the target.
