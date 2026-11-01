# Solution consistency audit coverage

Current full audit: **in progress**, pinned to convention revision **61**. All nine policy decisions were approved and documented. The exact current eligible sets and individual dispositions are in [AUDIT.coverage.json](AUDIT.coverage.json); declaration/call evidence is in [AUDIT.structure.json](AUDIT.structure.json).

Inventory: 1,090 maintained inputs, including 783 library C# files (one generated designer) and 258 test C# files. Both modern and legacy syntax variants parsed without errors; library net10 symbol compilation also has zero errors. Parsing, symbol binding and inventory alone do not count as semantic audit completion.

| Criterion | Compliant | Deviation | Exception | Unreviewed | N/A | Eligible files |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| [C01](CONVENTIONS.md#c01) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C02](CONVENTIONS.md#c02) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C03](CONVENTIONS.md#c03) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C04](CONVENTIONS.md#c04) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C05](CONVENTIONS.md#c05) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C06](CONVENTIONS.md#c06) | 0 | 0 | 0 | 1080 | 0 | 1080 |
| [C07](CONVENTIONS.md#c07) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C08](CONVENTIONS.md#c08) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C09](CONVENTIONS.md#c09) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C10](CONVENTIONS.md#c10) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C11](CONVENTIONS.md#c11) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C12](CONVENTIONS.md#c12) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C13](CONVENTIONS.md#c13) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C14](CONVENTIONS.md#c14) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C15](CONVENTIONS.md#c15) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C16](CONVENTIONS.md#c16) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C17](CONVENTIONS.md#c17) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C18](CONVENTIONS.md#c18) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C19](CONVENTIONS.md#c19) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C20](CONVENTIONS.md#c20) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C21](CONVENTIONS.md#c21) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C22](CONVENTIONS.md#c22) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C23](CONVENTIONS.md#c23) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C24](CONVENTIONS.md#c24) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C25](CONVENTIONS.md#c25) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C26](CONVENTIONS.md#c26) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C27](CONVENTIONS.md#c27) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C28](CONVENTIONS.md#c28) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C29](CONVENTIONS.md#c29) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C30](CONVENTIONS.md#c30) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C31](CONVENTIONS.md#c31) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C32](CONVENTIONS.md#c32) | 0 | 0 | 0 | 266 | 0 | 266 |
| [C33](CONVENTIONS.md#c33) | 0 | 0 | 0 | 266 | 0 | 266 |
| [C34](CONVENTIONS.md#c34) | 0 | 0 | 0 | 266 | 0 | 266 |
| [C35](CONVENTIONS.md#c35) | 0 | 0 | 0 | 266 | 0 | 266 |
| [C36](CONVENTIONS.md#c36) | 0 | 0 | 0 | 822 | 0 | 822 |
| [C37](CONVENTIONS.md#c37) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C38](CONVENTIONS.md#c38) | 0 | 0 | 0 | 40 | 0 | 40 |
| [C39](CONVENTIONS.md#c39) | 0 | 0 | 0 | 1087 | 0 | 1087 |
| [C40](CONVENTIONS.md#c40) | 0 | 0 | 0 | 1087 | 0 | 1087 |
| [C41](CONVENTIONS.md#c41) | 0 | 0 | 0 | 1087 | 0 | 1087 |
| [C42](CONVENTIONS.md#c42) | 0 | 0 | 0 | 822 | 0 | 822 |
| [C43](CONVENTIONS.md#c43) | 0 | 0 | 0 | 822 | 0 | 822 |
| [C44](CONVENTIONS.md#c44) | 0 | 0 | 0 | 822 | 0 | 822 |
| [C45](CONVENTIONS.md#c45) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C46](CONVENTIONS.md#c46) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C47](CONVENTIONS.md#c47) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C48](CONVENTIONS.md#c48) | 0 | 0 | 0 | 782 | 0 | 782 |
| [C49](CONVENTIONS.md#c49) | 0 | 0 | 0 | 1040 | 0 | 1040 |
| [C50](CONVENTIONS.md#c50) | 0 | 0 | 0 | 782 | 0 | 782 |

Completed criteria: **0/50**. File units overlap across criteria and must not be summed as distinct files. Unreviewed units are explicit and prevent a complete-audit status. The eight original implementation/policy tasks and nine adopted-policy decisions retain their history in [TODO.md](TODO.md).

<!-- current-audit-end -->

## Historical C47 audit and implementation record

The following original matrix/reasons are retained for provenance; they are superseded by current per-criterion dispositions above as those units are revalidated.

**Freshness:** This is the historical revision-51 audit. The subsequent CONS-007 policy selection changes C15/C16/C47 targets; affected C47 file checks are marked stale in `AUDIT.coverage.json` and `TODO.progress.json` until revalidated. At that policy checkpoint, CONS-007 was complete and seven runtime repairs were pending; subsequent implementation is recorded below. Counts and source-linked reproductions describe the original audit, not conformance to the newly selected policy. The original `AUDIT.validate.ps1` enforces its old convention pin and unchecked-task baseline and is not a current-policy validation command.

**Targeted implementation:** All seven runtime repairs and the CONS-007 policy decision are complete. CONS-001's earlier scope-manager verification is retained. On 2026-10-28, CONS-002/003/004/005/006/008 were implemented and verified by all 4,090 tests on net10.0, 446 Common/StateMachineHost/Persistence unit tests on net462 and all six library target builds (zero warnings/errors). The implementation evidence in `TODO.progress.json` supersedes the original findings for these specific repairs; it does not refresh this historical audit or its source/member-span inventory. The added terminal sink is recorded in implementation evidence and was not part of the original 76-file audit inventory.

The user selected **Async execution (C47) across hosting, scheduling, and task monitoring**. This is the entire approved audit scope, recorded before analysis in [TODO.progress.json](TODO.progress.json). The canonical workspace remains `docs/project/consistency/`; [TODO.md](TODO.md) is the single remediation/decision list.

The audit is pinned to convention revision **51**, digest `2701F65161D3762E23EEFC83DA28FB487901CBD9EE62D9C20E7A4F608BC3F382`, and parent Git revision `88d74702ce1685dd4544cfe68abbd776f6cc44a1`. `Global.Packages.props` was already modified. Current working-tree source bytes and supporting evidence are fingerprinted in [AUDIT.inventory.json](AUDIT.inventory.json). Conventions and production/test source are unchanged by the audit.

## Reconciliation

| Scope | Compliant | Deviation | Justified exception | Unreviewed | Applicable total | N/A separately | Files inspected |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| C47 / Xtate.Core hosting, scheduling and monitoring | 44 | 9 | 1 | 0 | 54 | 22 | 76 |

`44 + 9 + 1 + 0 = 54` applicable file units; `54 + 22 = 76` enumerated source files. A file with several findings is counted once as a deviation and links all its tasks. Seven remediation tasks address nine files; CONS-007 is the eighth task and supplies a remediation policy decision, not additional audited coverage. These are original audit counts; all eight TODO tasks are now checked, with subsequent implementation verification recorded above.

## Scope and method

Every maintained C# file under `StateMachineHost`, `Common/TaskMonitor`, and `Common/IoBoundTask` was inspected, plus `TaskExtensions.cs`, `ValueTaskExtensions.cs`, `LazyTask.cs`, `PersistentEventScheduler.cs`, and `PersistedScheduledEvent.cs`. These exact 76 paths are the eligible file set. No generated production source occurs in that set. Small contracts, inherited implementations and synchronous support files were retained and given an explicit disposition rather than silently excluded.

The comparison unit is one file with its contained execution boundaries: task-returning members, all await/async-enumeration/disposal paths, factory callbacks, completion sources, timer/thread callbacks, forwarding and detached observation. Complete source/control-flow inspection determined dispositions. [AUDIT.structure.json](AUDIT.structure.json) provides 389 method/constructor/property/local-function declaration spans, invocation expressions and 94 await-expression spans; counts are structural aids, not a substitute for inspection or a count of runtime states. Field initializers and implicit callbacks were also read. Both branches of the sole conditional declaration in scope (`StateMachineDestroyOnIdle` Timer disposal) were inspected; the structural inventory unions legacy and modern-symbol parses. There were no parser errors.

All 94 await expressions use `ConfigureAwait(false)`; asynchronous enumeration and configured disposal were also inspected directly. Returning a Task/ValueTask to a caller is observation transfer, so a direct forwarding member is not required to add a redundant async state machine. No incomplete-operation synchronous wait was found on the normal hosting async call paths. The dedicated worker scheduler's semaphore waits have an explicit role-based exception below. Task-monitor `GetAwaiter().GetResult()` calls are guarded by completed fault/cancellation states, so they are synchronous exception delivery, not blocking waits; existing tests explicitly expect that behavior.

Supporting tests/configuration/logger contracts are enumerated separately in the input inventory, with hashes. Test source is evidence about the cases it actually asserts; tests were not treated as a second test-style audit, nor as proof that absent throwing-cleanup/source-backed-ValueTask scenarios pass. A candidate search across production source located the sole parameterless-Forget caller in `LazyTask`; only that consumer and the in-scope helpers are counted here.

Five [source-linked reproductions](AUDIT.probes.json) use the current TaskMonitor, task helpers, LazyTask and controller/status source with minimal audit-only collaborators. They establish pre-cancelled monitoring abandonment, skipped successful ValueTask consumption, a faulting observer when reporting fails, an already-cancelled lazy result, and startup failure with separate faulted proxies and no readiness wait. The lazy late-error observation gap also relies on direct source tracing. IoC container startup is not executed by the harness. Scope-manager cleanup completion and idle-timer logging findings are direct control-flow findings. The reproduction project is an isolated audit utility; it is not added to the product solution and does not prove a full library build, passing product regression tests, GC behavior, or a process crash.

Antigravity's already authorized sanitized review was used as a second opinion. [AUDIT.antigravity.json](AUDIT.antigravity.json) records the success response, CLI warning, context boundary, rejected claims and independently useful test suggestion. Its output is not authority for any disposition.

Other criteria and subsystems were outside the agreed audit scope. C12 caller-token semantics, C13 cleanup-draining guarantees, broader C16 diagnostic/privacy rules and C46 concurrent disposal remain unaudited. CONS-007 subsequently selected the scoped C15/C16/C47 repair policy and resolved that decision prerequisite. The original audit had no unreviewed C47 files; the newly affected policy checks are now stale, as recorded in the tracker, and this historical result must not be presented as their completed revalidation.

## Per-file dispositions

Each file's complete inspected declaration list and source spans are in [AUDIT.coverage.json](AUDIT.coverage.json), keyed by the same path. N/A means no new asynchronous execution or completion/wait contract is implemented by that file; relevant inherited/base code is separately inspected at its declaring file. Compliant means no deviation from the adopted C47 target was found within the stated static audit, not that all behavior/security/lifecycle contracts are certified.

### C47-host-contracts-model-composition

| File | Disposition | Tasks | Evidence / rationale |
| --- | --- | --- | --- |
| [src/Xtate.Core/StateMachineHost/Abstractions/IDeadLetterQueue.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IDeadLetterQueue.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IEventRouter.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IEventRouter.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IEventScheduler.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IEventScheduler.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalEventDispatcher.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalEventDispatcher.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalService.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalService.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceActivator.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceActivator.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceCollection.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceCollection.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceController.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceController.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceGlobalCollection.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceGlobalCollection.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceInvokeId.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceInvokeId.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceParameters.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceParameters.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceProvider.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceProvider.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceScopeManager.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceScopeManager.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceSource.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceSource.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceType.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IExternalServiceType.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IInternalEventDispatcher.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IInternalEventDispatcher.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IInvokedStateMachine.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IInvokedStateMachine.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IIoProcessorHost.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IIoProcessorHost.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IParentEventDispatcher.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IParentEventDispatcher.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IParentStateMachineSessionId.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IParentStateMachineSessionId.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IRouterEvent.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IRouterEvent.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineCollection.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineCollection.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineController.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineController.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineHost.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineHost.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineScopeManager.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineScopeManager.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineStatus.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/IStateMachineStatus.cs) | Compliant | — | Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately. |
| [src/Xtate.Core/StateMachineHost/Abstractions/SecurityContextPermissions.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/SecurityContextPermissions.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/SecurityContextType.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/SecurityContextType.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Abstractions/StateMachineStatus.cs](../../../src/Xtate.Core/StateMachineHost/Abstractions/StateMachineStatus.cs) | Deviation | [CONS-008](TODO.md#cons-008) | ForceFailed can fault readiness before WhenAccepted has an observer; a synchronous production Forget failure in controller Start bypasses the await. The default status implementation and controller must reconcile startup-proxy ownership together. |
| [src/Xtate.Core/StateMachineHost/DependencyInjection/EventSchedulerModule.cs](../../../src/Xtate.Core/StateMachineHost/DependencyInjection/EventSchedulerModule.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/DependencyInjection/ExternalServiceModule.cs](../../../src/Xtate.Core/StateMachineHost/DependencyInjection/ExternalServiceModule.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/DependencyInjection/StateMachineProcessorModule.cs](../../../src/Xtate.Core/StateMachineHost/DependencyInjection/StateMachineProcessorModule.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Exceptions/StateMachineSecurityException.cs](../../../src/Xtate.Core/StateMachineHost/Exceptions/StateMachineSecurityException.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Model/RouterEvent.cs](../../../src/Xtate.Core/StateMachineHost/Model/RouterEvent.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs) | Compliant | — | CancelAsync returns the owned cancellation-source Task to its caller. Synchronous Cancel/Dispose remain separate synchronous contracts. |

### C47-host-services

| File | Disposition | Tasks | Evidence / rationale |
| --- | --- | --- | --- |
| [src/Xtate.Core/StateMachineHost/Services/DeadLetterQueue.cs](../../../src/Xtate.Core/StateMachineHost/Services/DeadLetterQueue.cs) | Compliant | — | Enqueue returns Logger.Write ValueTask to the caller without discarding completion. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalCommunication.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalCommunication.cs) | Compliant | — | TrySend returns or awaits routing/scheduling work; Cancel forwards the ValueTask. All await expressions suppress context capture. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalEventDispatcher.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalEventDispatcher.cs) | Compliant | — | Every selected dispatch/dead-letter operation is awaited with ConfigureAwait(false); no detached branch. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceClass.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceClass.cs) | Compliant | — | Explicit parent-event Dispatch returns the dispatcher ValueTask; other members synchronously supply configuration/forwarding. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceCollection.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceCollection.cs) | Compliant | — | Dispatch awaits asynchronous controller lookup and every fallback dispatch/dead-letter path; no locally detached task. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceController.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceController.cs) | Compliant | — | WaitForCompletion returns AsyncInit; execution and error event/log dispatch are awaited. Reporting failures propagate through the returned execution operation rather than being silently discarded here. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceEventRouter.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceEventRouter.cs) | Compliant | — | GetRouterEvent completes synchronously; Dispatch returns collection dispatch directly; provider selection is synchronous. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceFactory.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceFactory.cs) | Compliant | — | CreateService awaits selection/creation; provider enumeration and enumerator disposal are awaited with false context capture. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceGlobalCollection.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceGlobalCollection.cs) | Compliant | — | TryDispatch awaits controller lookup and dispatch or returns false; no owned background operation is dropped. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceManager.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceManager.cs) | Compliant | — | Start awaits construction and scope activation; the scope manager independently monitors completion. Forward and Cancel return their ValueTasks. Scope-manager defects are attributed to their source boundary, not duplicated here. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceResult.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceResult.cs) | Compliant | — | WaitForCompletion exposes the backing Task as a ValueTask; proxy completion responsibility belongs to ExternalServiceScopeManager. |
| [src/Xtate.Core/StateMachineHost/Services/ExternalServiceScopeManager.cs](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceScopeManager.cs) | Deviation | [CONS-001](TODO.md#cons-001) | Start awaits resolution and detaches WaitAndCleanup through TaskMonitor; Cleanup can throw before the separately published result source is completed. Sync and async disposal remain distinct paths; all async awaits use ConfigureAwait(false). |
| [src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs) | Compliant | — | DelayedFire is explicitly transferred to TaskMonitor; delay, dispatch and logging are awaited; removal/disposal is in finally. Async cancel/disposal returns or awaits aggregate work. Caller-token and cleanup-drain policies are not C47 findings. |
| [src/Xtate.Core/StateMachineHost/Services/InternalEventDispatcher.cs](../../../src/Xtate.Core/StateMachineHost/Services/InternalEventDispatcher.cs) | Compliant | — | Switch returns exactly the chosen dispatcher/dead-letter ValueTask to the caller. |
| [src/Xtate.Core/StateMachineHost/Services/IoProcessorBase.cs](../../../src/Xtate.Core/StateMachineHost/Services/IoProcessorBase.cs) | Compliant | — | Explicit interface methods forward returned ValueTasks; router-event construction is synchronously completed; concrete Dispatch remains an asynchronous contract. |
| [src/Xtate.Core/StateMachineHost/Services/LocationInvokedStateMachine.cs](../../../src/Xtate.Core/StateMachineHost/Services/LocationInvokedStateMachine.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Services/ScxmlIoProcessor.cs](../../../src/Xtate.Core/StateMachineHost/Services/ScxmlIoProcessor.cs) | Compliant | — | Dispatch returns InternalEventDispatcher completion; other operations compute routing data synchronously. |
| [src/Xtate.Core/StateMachineHost/Services/ScxmlStringInvokedStateMachine.cs](../../../src/Xtate.Core/StateMachineHost/Services/ScxmlStringInvokedStateMachine.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Services/SecurityContext.cs](../../../src/Xtate.Core/StateMachineHost/Services/SecurityContext.cs) | Compliant | — | TaskFactory construction selects the dedicated or denying scheduler and returns it; no Task is started or discarded by this type. |
| [src/Xtate.Core/StateMachineHost/Services/SecurityContextFactory.cs](../../../src/Xtate.Core/StateMachineHost/Services/SecurityContextFactory.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/StateMachineHost/Services/SecurityContextRegistration.cs](../../../src/Xtate.Core/StateMachineHost/Services/SecurityContextRegistration.cs) | Compliant | — | DisposeAsync restores AsyncLocal state synchronously and returns a completed ValueTask; introduces no wait or background work. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineCollection.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineCollection.cs) | Compliant | — | Dispatch and Destroy await controller lookup and operation/dead-letter completion; pending lookup mechanics are a dependency context, not synchronously waited here. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineControllerBase.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineControllerBase.cs) | Deviation | [CONS-008](TODO.md#cons-008) | ExecuteAsync is monitored and ordinary interpreter paths complete the result/status. Startup failure can prevent a handle from being exported while private result/readiness proxy Tasks have already faulted; their separate observation is not guaranteed by observing ExecuteAsync. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineDestroyOnIdle.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineDestroyOnIdle.cs) | Deviation | [CONS-006](TODO.md#cons-006) | Timer exception callback discards Logger.Write ValueTask. StateTracker.OnChanged awaits interpreter acquisition; native async Timer disposal and the synchronous legacy shim are legitimate target-specific variants. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineExternalService.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineExternalService.cs) | Compliant | — | Execute/Dispatch and async disposal await work. Synchronous disposal explicitly transfers Destroy to TaskMonitor rather than blocking; no new background-disposal policy is inferred. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineHost.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineHost.cs) | Compliant | — | Start uses configured asynchronous enumeration and awaits every processor start; Stop awaits each popped processor. Failure may stop cleanup traversal (C13 policy outside scope), but the returned operation itself completes/faults and is not detached. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineResult.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineResult.cs) | Compliant | — | GetResult returns a ValueTask wrapping the published result Task; source completion defect is attributed to the scope manager. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineRuntimeController.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineRuntimeController.cs) | Compliant | — | No new methods; inherits the audited StateMachineControllerBase. The inherited defect is tracked once at its declaring type. |
| [src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs) | Deviation | [CONS-001](TODO.md#cons-001) | Run explicitly transfers WaitAndCleanup to TaskMonitor; Cleanup can throw before caller result publication reaches a setter. DestroyAll aggregates returned operations; skipping the successful private async-ValueTask Destroy result cannot lose a custom source because that private method uses the ordinary async builder. |

### C47-io-scheduling

| File | Disposition | Tasks | Evidence / rationale |
| --- | --- | --- | --- |
| [src/Xtate.Core/Common/IoBoundTask/Abstractions/IIoBoundTask.cs](../../../src/Xtate.Core/Common/IoBoundTask/Abstractions/IIoBoundTask.cs) | Compliant | — | Exposes TaskFactory ownership to the caller; contains no hidden asynchronous operation. |
| [src/Xtate.Core/Common/IoBoundTask/DependencyInjection/IIoBoundTaskModule.cs](../../../src/Xtate.Core/Common/IoBoundTask/DependencyInjection/IIoBoundTaskModule.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/Common/IoBoundTask/Services/DefaultIoBoundTask.cs](../../../src/Xtate.Core/Common/IoBoundTask/Services/DefaultIoBoundTask.cs) | Compliant | — | Returns a cached TaskFactory using the dedicated scheduler; starts no operation itself. |
| [src/Xtate.Core/Common/IoBoundTask/Services/IoBoundTaskScheduler.cs](../../../src/Xtate.Core/Common/IoBoundTask/Services/IoBoundTaskScheduler.cs) | Justified exception | — | Dedicated background worker threads wait on a semaphore to drain scheduled Task objects through TryExecuteTask. These infrastructure waits do not synchronously block an async caller; no user Task is discarded. Worker inlining and fault-drain tests are supporting evidence. |

### C47-persistent-scheduling

| File | Disposition | Tasks | Evidence / rationale |
| --- | --- | --- | --- |
| [src/Xtate.Core/Persistence/Model/PersistedScheduledEvent.cs](../../../src/Xtate.Core/Persistence/Model/PersistedScheduledEvent.cs) | Not applicable | — | Synchronous representation/storage model. Inherited cancellation behavior is in ScheduledEvent; no new asynchronous execution to assess. |
| [src/Xtate.Core/Persistence/Services/PersistentEventScheduler.cs](../../../src/Xtate.Core/Persistence/Services/PersistentEventScheduler.cs) | Compliant | — | InitializeAsync returns AsyncInit; Init awaits rescheduling/storage work; Schedule/Cancel await base operations; WaitForDispatch awaits delay/removal; semaphore acquisition is asynchronous and release is in finally. |

### C47-task-helpers

| File | Disposition | Tasks | Evidence / rationale |
| --- | --- | --- | --- |
| [src/Xtate.Core/Common/Extensions/TaskExtensions.cs](../../../src/Xtate.Core/Common/Extensions/TaskExtensions.cs) | Deviation | [CONS-005](TODO.md#cons-005) | Monitored overloads forward ownership. Parameterless Forget is empty and supplies no observer; the only production caller of parameterless Forget is LazyTask (ValueTask overload), with both helper shapes exposing the same unsafe fallback design. |
| [src/Xtate.Core/Common/Extensions/ValueTaskExtensions.cs](../../../src/Xtate.Core/Common/Extensions/ValueTaskExtensions.cs) | Deviation | [CONS-005](TODO.md#cons-005) | Monitored overloads forward ownership; parameterless Forget discards Preserve result. Token-only WaitAsync remains a wait adapter, not a promised monitoring transfer; the caller retains underlying-operation responsibility. |
| [src/Xtate.Core/Common/Helpers/LazyTask.cs](../../../src/Xtate.Core/Common/Helpers/LazyTask.cs) | Deviation | [CONS-005](TODO.md#cons-005) | Factory is awaited once; normal outcomes reach the shared completion source. If caller cancellation has won and no monitor is supplied, a late factory fault rethrows into the unmonitored Forget fallback. |

### C47-task-monitor

| File | Disposition | Tasks | Evidence / rationale |
| --- | --- | --- | --- |
| [src/Xtate.Core/Common/TaskMonitor/Abstractions/ITaskMonitor.cs](../../../src/Xtate.Core/Common/TaskMonitor/Abstractions/ITaskMonitor.cs) | Compliant | — | Declares all four wait and three Forget shapes; exposes the explicit task-observation transfer boundary. Implementation deviations are in TaskMonitor.cs. |
| [src/Xtate.Core/Common/TaskMonitor/DependencyInjection/TaskMonitorModule.cs](../../../src/Xtate.Core/Common/TaskMonitor/DependencyInjection/TaskMonitorModule.cs) | Not applicable | — | Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file. |
| [src/Xtate.Core/Common/TaskMonitor/Services/TaskMonitor.cs](../../../src/Xtate.Core/Common/TaskMonitor/Services/TaskMonitor.cs) | Deviation | [CONS-002](TODO.md#cons-002), [CONS-003](TODO.md#cons-003), [CONS-004](TODO.md#cons-004) | WaitAsync pre-cancelled branches abandon incomplete operations; successful ValueTask Forget skips consumption; discarded MonitorTaskCompletion can fault in reporting. Guarded GetResult only runs on known-completed faults and is not blocking. |

## Reproduction and verification commands

From the Core solution root, the audit utility can be rerun without importing the repository's build/package properties:

```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
dotnet run --project docs/project/consistency/AUDIT.probe.csproj `
  -p:ImportDirectoryBuildProps=false -p:ImportDirectoryPackagesProps=false -p:NuGetAudit=false `
  -- T:/Projects/TSSArt/xtate.dev/src/Xtate.Core/docs/project/consistency
```

This command regenerates the structural/probe evidence for the current source; reconcile inventory fingerprints before treating a later run as this audit. The supplied stub types are audit-only and do not replace production collaborators. A first sandboxed CLI attempt failed because telemetry could not read the registry; disabling CLI telemetry resolved that environment issue. The successful build used .NET 10.0.13 at runtime. MSBuild warned that its server was unavailable and completed with an in-process build.

Required product regression checks for future repairs are listed per TODO item; none is represented as already passing. No source remediation, convention change or full-solution audit was performed.

