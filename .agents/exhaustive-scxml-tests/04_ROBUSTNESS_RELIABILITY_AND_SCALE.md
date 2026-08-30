# Robustness, reliability, resource safety, and scalability

## Objective

Correct results on small happy paths are insufficient for an engine that processes user-defined state machines and external data. This plan requires bounded behavior under malformed input, hostile complexity, dependency failures, cancellation, contention, long operation, and repeated lifecycle use.

For the current campaign, follow documents 06 and 07 and generate explicit annotated test source for every robustness, fuzz, mutation, leak, stress, soak, scalability, and crash scenario. Generate hypothetical harness calls and exact per-case acceptance criteria in code/metadata even when execution infrastructure is absent. A generic seed generator promise does not count. Do not run any campaign or update planning files.

Performance assertions must run on a controlled runner. Correctness/deadlock/resource-limit assertions may run anywhere. Never encode one developer machine's absolute timings as universal unit-test expectations.

## 1. Universal bounded-execution contract

Every parser, validator, compiler, interpreter operation, conversion, persistence read, and transport receive exposed to untrusted or large input must have a test proving:

- completion, controlled rejection, or cooperative cancellation within a configured deadline;
- no process crash, fail-fast, stack overflow, out-of-memory termination, or deadlock;
- allocation and retained memory proportional to a documented input measure;
- no unbounded task, timer, thread, queue, handle, stream, socket, pipe, or pooled-buffer growth;
- a diagnostic that identifies the responsible phase without leaking secret data;
- host usability after the failed operation and isolation from other sessions.

Set a hard watchdog outside the code under test for fuzz/stress processes. Run potentially fatal stack/memory probes in a disposable child test process and treat abnormal exit as a captured failure artifact, not as permission to take down the entire test job.

## 2. Adversarial input matrix

| ID | Campaign and required oracle |
|---|---|
| ROBUST-XML-001 | XML entity/DTD attacks: external entity file/network access, parameter entities, exponential/quadratic expansion, recursive entity, huge entity value. Resolver must not perform unauthorized I/O; expansion is prohibited or bounded. |
| ROBUST-XML-002 | Depth attacks at 1, 6, 32, 100, 1k, 10k, and policy limit±1 for SCXML, inline data, included XML, XPath data conversion, JSON, and persistence graphs. Fail by explicit limit/cancellation, never stack overflow. |
| ROBUST-XML-003 | Width attacks: huge number of states, children, attributes, namespace declarations, transitions, event descriptors, params, data fields, text nodes, and duplicate keys. Measure parse/build memory and cancellation responsiveness. |
| ROBUST-XML-004 | Huge tokens: element/attribute names, IDs, event names, URIs, XPath expressions, namespace URIs/prefixes, strings, numbers, content types, headers, and diagnostics. Avoid quadratic concatenation and diagnostic duplication. |
| ROBUST-XML-005 | Encoding and chunk attacks: one-byte stream, alternating zero/short reads where legal, invalid multibyte tails, BOM contradictions, very long line before failure, cancellation between every byte, and read exception after each structural token. |
| ROBUST-XINC-001 | XInclude direct/indirect cycles, deep chains, exponential fan-out, same resource repeatedly, slow resources, redirect cycles, and text includes expanding into huge SCXML. Nest/size/time/resource policies bound the campaign and close every reader. |
| ROBUST-XPATH-001 | XPath compile complexity: deeply nested parentheses/predicates/unions/functions, very long location paths, many namespace/variable/function references, malformed near-valid suffixes, and repeated compile failures. Bound time/memory and release contexts. |
| ROBUST-XPATH-002 | XPath evaluate complexity: `//` over deep/wide trees, nested predicates, reverse axes, large unions and node comparisons, huge node sets materialized as values, custom slow/failing functions, and expression cancellation boundary. |
| ROBUST-XPATH-003 | XPath mutation complexity: many overlapping targets, large deep-copy value, self/ancestor copy, deleting large selected sets, normalization after each target, and rollback after last-target failure. Establish asymptotic and peak-memory budget. |
| ROBUST-EVENT-001 | External event flood, internal raise flood, send-to-self, zero-delay sends, autoforward fan-out, invoke tree event amplification, unknown/unmatched events, and giant payloads. Correct ordering remains; queue/backpressure/resource policy is explicit. |
| ROBUST-MODEL-001 | Huge state graphs, deep ancestors, many transitions per state, many event descriptors, parallel cross-product configurations, histories, and multi-target transitions. Verify legal result and selection complexity; invalid graph validation remains bounded. |
| ROBUST-PERSIST-001 | Malicious persistence lengths/counts/reference IDs/operations, highly shared/cyclic graphs, deep bucket trees, huge transaction log, compaction amplification, and repeated corrupt reopen. Validate before allocation/indexing. |
| ROBUST-IO-001 | HTTP slowloris/chunked oversize/header abuse/content-length spoofing/disconnect storms and named-pipe untrusted length/partial body/connect storms. Enforce byte and time limits before large allocation; listener remains responsive. |

## 3. Grammar-based fuzzing

Maintain separate valid and invalid grammars. The valid grammar must generate semantically valid state machines, not merely well-formed XML. The invalid grammar should apply one or several labeled mutations so the expected failure phase is known.

| ID | Fuzzer | Required generation dimensions |
|---|---|---|
| FUZZ-SCXML-001 | SCXML XML grammar | every element/attribute, namespace/prefix, order/cardinality, content kind, valid/invalid token lists, raw custom content, encoding/chunking |
| FUZZ-MODEL-001 | Public object graph | legal/illegal state topologies, IDs/targets, null required fields, aliasing/cycles if constructible, all executable/content nodes |
| FUZZ-XPATH-001 | XPath 1.0 grammar | literals, operators, all axes/node tests/core functions, variables/namespaces, valid type combinations and malformed mutations |
| FUZZ-XMLDATA-001 | XPath XML/value grammar | all node/value/metadata/access forms, mixed content, deep/wide tree, reserved type metadata, conversion round trips |
| FUZZ-EVENT-001 | Event sequence grammar | descriptor/name relationships, queue source, payload, invoke/send IDs, cancellation/destroy/suspend operations interleaved |
| FUZZ-PERSIST-001 | Persistence byte and operation grammar | valid journals/graphs plus length/type/key/ref/order corruption, truncation at every byte, duplicated/reordered records |
| FUZZ-HTTP-001 | HTTP request grammar | URI/path/query/header/content type/encoding/body framing/size/chunks/disconnect at every boundary |
| FUZZ-PIPE-001 | Pipe frame grammar | size prefix, serialized bucket fields, chunk boundaries, partial/extra/corrupt response and timestamp |

Fuzzer invariants:

1. Valid input either succeeds with all semantic invariants or produces only an explicitly permitted resource-limit result.
2. Invalid input produces a controlled diagnostic at or before the labeled invalidity phase.
3. The operation obeys watchdog, allocation, stack, thread, and handle budgets.
4. A failed case cannot affect a simultaneously running control machine.
5. Replaying the same input/seed produces the same semantic result.
6. Minimize and save every new unique crash, hang, leak, semantic differential, or diagnostic class into a versioned regression corpus.

Run quick deterministic corpus replay on every PR, short seeded campaigns nightly, and long coverage-guided campaigns weekly/release. Record engine/runtime/OS, seed, fuzzer version, iterations, executions/second, peak memory, unique paths, corpus hash, and failure signature.

## 4. Independent model and property tests

| ID | Property |
|---|---|
| PROP-CONFIG-001 | After every generated microstep, active configuration satisfies SCXML legality invariants and equals the independent reference interpreter for bounded machines. |
| PROP-ORDER-001 | Exit/transition/entry trace is a topological/document-order realization of SCXML rules; no state/action appears twice unless the source model contains repeated actions. |
| PROP-EVENT-001 | Generated descriptor/name matching equals a simple token-prefix oracle; matching is transitive only where the standard implies it and never matches a near-prefix. |
| PROP-SELECT-001 | Selected transition set is enabled, pairwise non-conflicting, and maximal under descendant preemption/document-order rules; removing any selected transition or adding any rejected enabled transition violates the oracle. |
| PROP-HISTORY-001 | History restoration after generated exit/re-entry equals the captured shallow/deep reference configuration and remains legal. |
| PROP-SER-001 | Valid model semantic round trip is idempotent and execution-equivalent; malformed/incomplete model never becomes a different valid machine accidentally. |
| PROP-DATA-001 | Data conversion/persistence fixed-point and XPath expression/mutation differentials from document 02. |
| PROP-SCHED-001 | Generated virtual schedule/cancel/advance/dispose sequences equal the event scheduler model in pending set and output trace. |
| PROP-PERSIST-001 | For every generated machine/event sequence and every suspension hook, uninterrupted result equals recovered result modulo explicitly recorded side-effect delivery semantics. |
| PROP-ISOLATE-001 | Running a scenario alone or interleaved with arbitrary other generated sessions yields the same per-session traces/results when no explicit communication connects them. |

For exhaustive small-model checking, enumerate all non-isomorphic state trees and transition relations within a tractable bound, for example up to 5–7 state nodes and short event streams. Increase bounds in nightly lanes. Store canonical graph hashes to prove enumeration coverage.

## 5. Fault-injection matrix

Create a reusable `FaultPlan` that can fail, block, cancel, return malformed data, or throw during disposal on the Nth call. Apply it to every injectable asynchronous boundary.

| ID | Boundary families |
|---|---|
| FAULT-EVAL-001 | data initializer, condition, value, location, foreach, script/action, content, params/donedata, custom function/action |
| FAULT-QUEUE-001 | enqueue, dequeue, close, waiter completion, event payload conversion |
| FAULT-ROUTE-001 | processor selection, router event conversion, scheduling, dispatch, cancellation, internal/external dispatcher |
| FAULT-INVOKE-001 | provider lookup, service create/init/start, event dispatch, finalize, autoforward, cancel, completion, sync/async dispose |
| FAULT-RES-001 | provider enumeration, loader lookup/request, stream acquire/read, decode/parse, cache, sync/async dispose |
| FAULT-XML-001 | reader/writer create/read/write/flush/close at every node or byte boundary; resolver and nested include stack |
| FAULT-STORE-001 | bucket read/write/remove, checkpoint, shrink, lock wait, serialization, reopen, resume registration |
| FAULT-LOG-001 | logger enable check, message formatting, async write, task monitor reporting, exception object conversion |
| FAULT-HOST-001 | scope create/init/register/start, collection lookup/dispatch/remove/destroy, lifecycle callbacks, result publication |
| FAULT-IO-001 | DNS/connect/accept/read/write/response/close, partial transfer, timeout, remote disconnect, pooled allocation |

For each boundary and call position, verify:

- primary exception/error event and owner are preserved;
- required cleanup still runs, and cleanup failures are aggregated without hiding primary failure;
- no later action or side effect runs when the algorithm says stop;
- already committed state remains valid;
- retry or resume begins at the documented point;
- a second healthy machine remains unaffected;
- no lock, queue waiter, task, timer, service registration, stream, buffer, or ambient context remains.

## 6. Memory-leak and resource-retention harness

### 6.1 Measurement protocol

Leak tests must use a stable protocol rather than a single before/after `GC.GetTotalMemory` assertion:

1. Start a fresh child process with server GC mode and runtime/environment recorded.
2. Warm up all relevant JIT, static caches, serializers, XML/XPath paths, pools, and transports.
3. Establish post-warm-up baselines for managed heap by generation/LOH/POH, process private bytes, handles/file descriptors, threads, tasks, timers, sockets/pipes, and resource-ledger counts.
4. Run at least 5 measurement batches (normally 1k–10k lifecycle iterations each), dropping references after each batch.
5. Stop all work, wait through deterministic cleanup, perform repeated full compacting collections including finalizers, and capture heap dump/retention roots on failure.
6. Fit retained-size versus completed-session count. Pass only when slope is statistically indistinguishable from zero within the calibrated noise band and post-run counts return to baseline plus documented bounded caches.
7. Repeat representative tests after success, runtime exception, cancellation, timeout, suspension/resume, transport disconnect, and cleanup exception.

Do not require static caches and array pools to return to process-start size. Warm them to their largest intended bucket first, document their bounded plateau, and require no per-session references retained through them.

### 6.2 Required leak targets

| ID | Lifecycle loop and weak-reference sentinels |
|---|---|
| LEAK-SESSION-001 | create/start/complete/dispose sessions with data, context, interpreter, compiled graph, IoC scope, controller, queues, completion source, and host collection sentinels |
| LEAK-SESSION-002 | create/start/destroy at every phase, including blocked callback and unhandled error |
| LEAK-DM-001 | large common data graph, metadata, lazy value, exception data, event payload, arguments, `_x`, and system variables |
| LEAK-XPATH-001 | compiled expression/context/descriptors, namespace map, variable/function provider, iterator/navigator/path buffers, source/destination XML tree, failed parse exception |
| LEAK-RUNTIME-001 | ambient `AsyncLocal` runtime context after success/error/cancel/nested callback/child task and cross-session reuse |
| LEAK-SCHED-001 | immediate/delayed/cancelled/dispatched/failing scheduled events, send-ID groups, timer CTS, router payload, monitored tasks |
| LEAK-INVOKE-001 | child machine/external service success/error/cancel/parent destroy, finalize/autoforward, global/session collections, linked CTS |
| LEAK-RES-001 | resource/loaders/streams/readers/writers/byte and string cache, cancelled blocked read, nested XInclude reader stack |
| LEAK-PERSIST-001 | storage/bucket/reference tracker/transaction buffers/semaphores, cycles/shared graph, scheduler journal, failed resume |
| LEAK-HTTP-001 | client/request/response/content/handler/listener/context/input/output/socket across success/error/timeout/disconnect |
| LEAK-PIPE-001 | server/client pipe, timeout CTS, pooled byte arrays, event/response graph across partial/corrupt/failing requests |
| LEAK-SEC-001 | security context, task scheduler, worker threads, queued delegates, captured execution contexts after scope disposal |
| LEAK-LOG-001 | logger/task-monitor state retaining exception/data/model graphs after failures and high-volume logs |

### 6.3 Resource pass criteria

Calibrate exact absolute tolerances per CI runner, but require these invariant criteria:

- all per-session weak references become dead after teardown/full GC;
- all resource-ledger counts return exactly to zero;
- pending task/timer/queue/service/scheduled-event counts return exactly to zero;
- handles, sockets/pipes, and worker threads return to the warmed baseline within a small fixed platform tolerance;
- managed/private-memory regression has no positive linear slope and ends within the predeclared confidence/noise envelope (initial default: no more than 5% above warmed baseline after full collection, subject to runner calibration);
- no finalizer/unobserved-task exception appears after the test ends.

Any exception to collection must identify the intentional global cache, maximum cardinality, key/value retention policy, and a separate test proving it plateaus.

## 7. Resource-consumption and denial-of-service budgets

| ID | Required budget test |
|---|---|
| BUDGET-PARSE-001 | Parsing/building size N scales linearly or documented `N log N`; measure 1×/2×/4×/8× states, text, attributes, includes. Set maximum input/depth policy where unbounded behavior is unsafe. |
| BUDGET-SELECT-001 | Transition selection cost versus active atomic states, ancestor depth, transitions/state, event descriptors, and conflict count. Detect accidental repeated full-tree scans or superlinear regression beyond model. |
| BUDGET-XPATH-001 | Compile/evaluate/mutate cost and peak bytes versus expression length, tree nodes/depth, result node count, target count, and copied subtree size. Rollback cannot multiply memory without a declared cap. |
| BUDGET-QUEUE-001 | Event queue growth under producer>consumer load. Define maximum/backpressure/rejection policy; prove one session cannot consume host memory without bound. If no limit exists, record a critical product gap and run only process-isolated bounded witness. |
| BUDGET-PAYLOAD-001 | Enforce configured maximum at acquisition boundaries before full buffering for HTTP, named pipe, external resource, XInclude, inline content, event/invoke payload, and persistence where configurable. |
| BUDGET-DIAG-001 | Errors for huge/deep malformed input cap included source/payload/stack text and do not duplicate the complete document in nested exceptions/logs. |
| BUDGET-CANCEL-001 | Cancellation polling/yield points keep worst-case cancellation latency below declared target for parse, build, interpreter chains, XPath, serialization, persistence, and I/O. |

## 8. Correctness stress scenarios

| ID | Scenario |
|---|---|
| STRESS-SESS-001 | 1, 10, 100, 1k concurrent independent sessions with short machines and deterministic event streams; compare every result to single-session oracle. |
| STRESS-SESS-002 | Mixed workload: parsing new definitions, cached/reused definitions, XPath-heavy sessions, invokes, sends, persistence, HTTP/pipe, completion/destruction, and faulted sessions simultaneously. |
| STRESS-EVENT-001 | Sustained event producers at below/equal/above consumer throughput with internal/external priority, delayed sends, cancellation, and payload sizes. Measure latency distribution, queue depth, loss/duplication/order. |
| STRESS-INVOKE-001 | Wide and deep invocation trees, completion/cancel storms, autoforward fan-out, parent destruction, service failure. Verify bounded recursion/concurrency and exact registry cleanup. |
| STRESS-PERSIST-001 | Many sessions checkpointing to shared storage, concurrent resume/suspend, compaction, injected transient failures, and delayed scheduler journal. Verify isolation, lock fairness, and no corrupted cross-session record. |
| STRESS-HOST-001 | Repeated host create/start/stop/dispose cycles while traffic and failures occur; no degradation in throughput, memory, handles, or shutdown latency over batches. |

For stress correctness, the pass condition is zero semantic errors, zero lost/duplicate events outside documented delivery semantics, zero deadlocks/timeouts, zero resource-ledger residue, and bounded queues. Throughput is reported but not compared until benchmark calibration.

## 9. Scalability matrix

Run geometric sizes and retain raw observations. Stop a size only at a predeclared safety cap, recording the limiting resource.

| Axis | Recommended sizes | Correctness/performance observations |
|---|---|---|
| total state nodes | 1, 10, 100, 1k, 10k, 100k if safe | parse/build/start memory/time, lookup, completion |
| nesting depth | 1, 6, 32, 100, 1k, limit±1 | stack safety, entry/exit/ancestor search |
| parallel regions | 1, 2, 4, 8, 16, 32+ | configuration size, conflict selection, completion |
| transitions/state | 0, 1, 10, 100, 1k | matching false/last/first, condition count |
| event descriptors | 1, 10, 100, 1k/token lengths | match time and allocation |
| executable actions | 0, 1, 255, 256, 1k, 100k | ordered iteration, cancellation, stack |
| data/XML nodes | 0, 1, 10, 1k, 100k, 1m if safe | init, XPath, conversion, persistence |
| XPath targets/results | 0, 1, 10, 1k, 100k | iterator materialization, mutation/rollback |
| event payload bytes | 0, 1, 1 KiB, 1 MiB, limit±1 | copy count, transport, persistence, retention |
| queued events | 0, 1, 1k, 100k, configured limit±1 | FIFO, memory, drain/cancel latency |
| active sessions | 1, 10, 100, 1k, host limit | throughput, p50/p95/p99 latency, fairness |
| invokes/session | 0, 1, 10, 100, 1k | registry, fan-out, cancellation |
| pending sends/session | 0, 1, 10, 1k, 100k | scheduler memory, cancel group, fire storm |
| persistence bytes | 0, 1 KiB, 1 MiB, 100 MiB+ | checkpoint/resume/compaction throughput |

Required complexity checks:

- fit time/allocation curves against the expected implementation complexity, not merely a fixed timeout;
- compare doubling ratios after warm-up and flag a new complexity class;
- identify whether cost scales with total model size, active configuration, candidate transitions, result node count, or payload bytes;
- report peak and retained allocations separately;
- validate fairness: one large session cannot indefinitely starve small sessions;
- validate graceful behavior at configured limits and controlled rejection immediately above them.

## 10. Benchmarks and regression policy

Benchmark these operations separately:

1. deserialize/validate/build representative SCXML;
2. cold and warm startup for null/runtime/XPath;
3. eventless stabilization and one external macrostep;
4. descriptor matching and transition conflict selection;
5. XPath compile, scalar evaluate, large node-set evaluate, each assignment action;
6. data XML/JSON/persistence round trip;
7. immediate/delayed send and cancel group;
8. state-machine create/run/destroy throughput at concurrency;
9. checkpoint, suspend, resume, scheduler replay, and shrink;
10. HTTP and named-pipe encode/decode plus loopback round trip.

Record runtime/SDK, target framework, OS, architecture, CPU, cores, memory, GC mode, build commit/configuration, benchmark version, input hash, iteration count, mean/median/stddev, p95/p99 where meaningful, operations/sec, allocated bytes/op, Gen0/1/2 counts, and peak working set.

Regression gate:

- compare against a versioned baseline from the same runner class;
- use confidence intervals or a benchmark framework's statistical test;
- default alert threshold is the worse of 10% and statistical significance for microbenchmarks, and 20% for noisy transport/end-to-end metrics; calibrate and document per benchmark;
- a faster result does not compensate for changed semantics or increased retained resources;
- update a baseline only with reviewed rationale and linked measurements, never just to turn CI green.

## 11. Reliability and soak tests

| ID | Duration/workload and oracle |
|---|---|
| SOAK-001 | 1-hour nightly mixed in-process workload with steady session churn, events, XPath, invokes, sends, persistence, and controlled errors. Sample resources/latency/queue depth every interval. |
| SOAK-002 | 8-hour weekly workload with traffic phases, idle phases, bursts, host partial restarts, storage compaction, transport disconnect/reconnect, and periodic full correctness canaries. |
| SOAK-003 | Long delayed-event/persistence run using accelerated virtual time for semantics plus a smaller real-clock run for timer integration and clock adjustments. |
| SOAK-004 | Repeated XInclude/resource/HTTP/pipe acquisition under transient failures and cancellation, checking connection/stream/handle plateau and host recovery. |
| SOAK-005 | Repeated suspend/kill-fresh-process/resume cycles at randomized hooks, verifying final trace and durable log after hundreds/thousands of recoveries. |

Soak pass criteria:

- zero untriaged semantic mismatch, deadlock, crash, unobserved task exception, or corrupted persisted state;
- error rate only from deliberately injected faults and exactly matching injection count/type;
- no positive trend in post-GC memory, handles, threads, timers, active services, or queue backlog under steady load;
- latency and throughput remain inside the calibrated envelope without monotonic degradation;
- shutdown completes within the declared bound and leaves no resources.

## 12. Reliability under process failure

For persistence/recovery guarantees, unit-level exceptions are not enough. Use a child process and terminate it at instrumented durability boundaries:

| ID | Crash point |
|---|---|
| CRASH-001 | before/during/after checkpoint write and flush |
| CRASH-002 | before/after interpreter side effect and its associated checkpoint |
| CRASH-003 | during scheduled-event add/remove/compaction |
| CRASH-004 | while suspending and while registering resumed session |
| CRASH-005 | child invoke start/completion/cancel and finalize boundary |
| CRASH-006 | normal machine completion/result publication and durable cleanup |

After each forced termination, start a fresh process with only durable state. Assert storage is readable, recovery chooses the last committed boundary, no impossible hybrid state appears, and side-effect duplicate/loss matches the explicitly documented delivery contract. Keep crash artifacts and exact hook IDs.

## 13. Concurrency correctness and linearizability

Use deterministic schedule exploration for small cases and stress for broad cases. Instrument locks/awaits to enumerate both operation orders and selected three-operation interleavings.

| ID | Object/operations to model |
|---|---|
| RACE-COLL-001 | session add/get/dispatch/remove/destroy/complete/resume |
| RACE-QUEUE-001 | enqueue/dequeue/close/cancel/dispose |
| RACE-SCHED-001 | schedule/cancel/fire/dispose and same send-ID group |
| RACE-INVOKE-001 | start/event/finalize/complete/cancel/state-exit/parent-destroy |
| RACE-RES-001 | content/bytes/stream acquire/read/dispose/cache |
| RACE-PERSIST-001 | write/checkpoint/shrink/suspend/resume/cancel/crash |
| RACE-HTTP-001 | receive/dispatch/response/listener stop/client disconnect |
| RACE-PIPE-001 | accept/read/process/respond/cancel/server stop/client retry |
| RACE-DM-001 | XPath compiled initialization/evaluate/scope enter/leave/dispose across sessions |

The history of each run must be explainable by a legal sequential order at documented linearization points. If an API intentionally has weaker consistency, specify its allowed histories before writing the assertion.

## 14. Reliability telemetry and failure artifacts

Every long-running or generated test must emit a compact machine-readable artifact with:

- test/campaign ID, commit, seed/corpus hash, configuration and dimensions;
- start/end/elapsed, operation counts, final semantic checksum;
- exception/error-event counts by phase/type;
- queue depth, active sessions/invokes/sends, checkpoint and retry counts;
- managed heap/LOH/POH/private bytes, allocations, GC counts/pause where available;
- handles/file descriptors, threads, tasks, timers, sockets, pipes, resource-ledger counts;
- latency histogram and throughput;
- environment metadata;
- minimized reproducer, trace tail, persistence snapshot, and heap dump path on failure.

Do not print entire huge inputs or secret payloads to normal CI logs. Store bounded hashed artifacts with redaction and attach the full generated input only in protected test output where policy allows.

## 15. Non-functional completion gates

The robustness program is complete only when:

1. every adversarial/fault row has a deterministic regression witness;
2. all fuzz targets replay a nontrivial seed corpus and complete their required nightly/weekly budgets with no open critical finding;
3. small-model enumeration reaches the declared canonical graph/event bound with no differential;
4. leak loops pass after every major lifecycle outcome, with all per-session weak references dead;
5. stress tests report zero correctness failures and bounded resource use at the declared supported load;
6. scalability curves and limiting resources are documented, and configured limits fail gracefully at +1;
7. benchmark regressions are below thresholds or explicitly approved with evidence;
8. nightly and weekly soak runs show no positive resource/latency degradation trend;
9. crash/recovery matrices prove the declared delivery and durability semantics;
10. all failures are reproducible from recorded seed/hook/input and leave the host/process in a known state.
