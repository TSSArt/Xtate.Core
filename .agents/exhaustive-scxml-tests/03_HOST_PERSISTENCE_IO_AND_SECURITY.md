# Host, persistence, I/O, resources, external services, and security

## Scope

This document tests the engine as a hosted, asynchronous system. It covers state-machine scope and collection lifecycle, routing and scheduling, resource acquisition, SCXML/HTTP/named-pipe processors, invocation/external services, security contexts, persistence journals, suspension, and exact recovery.

For the current campaign, follow documents 06 and 07 and generate explicit annotated C# source for every remaining assigned host, transport, persistence, and security test. Generate test bodies even when runners, processes, transports, or helpers do not exist. Generic requirement factories and broad range-only cases do not count. Do not compile, execute, provision resources, or update planning files.

Every asynchronous scenario must assert a linearization point, bounded completion, cancellation behavior, task observation, and resource cleanup. Where real transports are necessary, use loopback-only ephemeral resources in an isolated category; prove core formatting and routing with deterministic in-memory doubles first.

## 1. Options, dependency graph, and scope isolation

| ID | Required scenarios and oracle |
|---|---|
| HOST-OPT-001 | Every `StateMachineOptions` default and explicit value: persistence level, unhandled-error behavior, idle timeout, XInclude allowed, and nesting limit. Test invalid enum casts, negative/zero/extreme times/limits, option mutation before/after scope creation, and independent option scopes. |
| HOST-OPT-002 | HTTP options: null/valid/invalid listen URL, public base URL absolute/relative/scheme/host/path/query/fragment/Unicode/IDN, max message size 0/1/boundaries/max/negative, timeout infinite/zero/positive/invalid below infinite. Constructors and setters reject consistently. |
| HOST-OPT-003 | Named-pipe options: local/remote/IDN/invalid hosts; null/empty/dot/dot-dot/escaped/Unicode/mixed-case/long pipe names; size and timeout boundaries. Round-trip valid names through target generation/parsing. |
| HOST-IOC-001 | Resolve all public registration paths with minimum/default/custom services. Missing required dependency, duplicate registration, wrong lifetime, factory exception/cancel, circular/deferred dependency, and disposal during async initialization produce precise bounded failures. |
| HOST-IOC-002 | One host with 1/many sequential and concurrent state-machine scopes. IDs, contexts, data, queues, handlers, expression scopes, event schedulers, external services, security, options, and persistence buckets remain isolated. |
| HOST-IOC-003 | Disposal order for nested IoC scopes in success, startup failure, runtime failure, suspend, resume, and destroy. Every `IDisposable`/`IAsyncDisposable` dependency is called at most once; async disposal is awaited; aggregate errors preserve all failures. |
| HOST-IOC-004 | Concurrent first resolution/initialization of deferred, lazy, and async services. Exactly one instance per intended lifetime; no partially initialized object escapes; failed initialization follows documented retry/cache behavior. |

## 2. State-machine collection, controller, and lifecycle races

| ID | Required scenarios and oracle |
|---|---|
| HOST-LIFE-001 | Create/start/get/dispatch/destroy one machine with explicit/generated session ID. Verify collection membership and controller state at each lifecycle point, normal result, and removal after completion/disposal. |
| HOST-LIFE-002 | Duplicate session IDs sequentially and concurrently; ID collision with active/completing/destroying/suspended session; retry after failed create; generated-ID uniqueness under high concurrency. No existing machine may be replaced accidentally. |
| HOST-LIFE-003 | Dispatch before create/start, during start, running, stable wait, completing, suspended, resuming, destroying, destroyed, and unknown session. Define acceptance linearization and verify no lost/ghost event. |
| HOST-LIFE-004 | Destroy before start, during every startup phase, during condition/action/invoke/send/persist/wait, after normal completion, and concurrently from many callers. Operation is idempotent or fails consistently, waiters complete, and collection removes once. |
| HOST-LIFE-005 | Host/controller disposal with 0/1/many active, idle, blocked, failing, suspended, and invoked child machines. Cross synchronous/async disposal and cleanup failures. No session, background task, timer, stream, or service remains. |
| HOST-LIFE-006 | Idle-destruction timeout infinite/zero/positive and event at timeout−ε/exact/+ε; activity resetting deadline; delayed send/pending invoke while otherwise idle; suspend/resume; clock jumps under real-clock adapter. Use virtual time for semantic tests. |
| HOST-LIFE-007 | Controller/public method cancellation before call, while waiting for lock/queue, during operation, and after linearization. Caller cancellation must not silently cancel unrelated machine lifetime unless contract says so. |
| HOST-LIFE-008 | Completion result and exception delivery to 1/many concurrent waiters, late waiter, cancelled waiter, destroyed machine, and child invocation. Exactly one underlying completion; waiter cancellation does not consume result for others. |

## 3. Queues, task monitoring, and in-process event scheduler

| ID | Required scenarios and oracle |
|---|---|
| HOST-QUEUE-001 | FIFO for empty/one/many enqueue/dequeue, concurrent producers, one/many consumers as supported, close empty/nonempty, cancellation while waiting, producer after close, and dispose. Verify payload release after consume/close. |
| HOST-QUEUE-002 | Internal versus external queue priority through the interpreter, including internal events generated while an external event is processed and events racing a stable-state wait. Use sequence-numbered trace. |
| HOST-SCHED-001 | Schedule plain and already-scheduled router events with no/empty/explicit send ID, zero/positive/huge delay, cancellation token already cancelled, scheduler disposal token, and unsupported origin type/router type. |
| HOST-SCHED-002 | Several events under the same send ID and distinct IDs. Cancel group before wait, during wait, during dispatch, after dispatch, and concurrently with scheduling; repeat cancellation. Only the intended group is affected and collection entries disappear. |
| HOST-SCHED-003 | Select first capable router deterministically; zero capable, one, many overlapping routers; `CanHandle` throws; dispatch throws synchronously/asynchronously; logger enabled/disabled/failing. Every forgotten task is observed through task monitor. |
| HOST-SCHED-004 | Sync/async scheduler disposal with delayed and dispatching events, cancellation exceptions from one/many events, concurrent schedule/cancel/dispose, and repeated disposal. Verify aggregate exception policy and zero live timers/tasks/events. |
| HOST-SCHED-005 | Deterministic scheduler reference model: generated schedule/cancel/advance/dispatch operation sequences match model in dispatch count/order and pending set. Include same-timestamp ordering and ID reuse. |
| HOST-TASK-001 | Task monitor observes successful, faulted, and cancelled fire-and-forget tasks; handler/logging failure; host disposal waits or reports outstanding tasks according to contract; exceptions never become unobserved finalizer events. |

## 4. Basic SCXML I/O processor and event routing

| ID | Required scenarios and oracle |
|---|---|
| IO-SCXML-001 | `CanHandle` for omitted type, canonical SCXML processor URI, alias, equal-but-distinct URI object, case/path variants, unknown, malformed, and custom processor collision. Default/omitted routes to SCXML. |
| IO-SCXML-002 | Internal targets: canonical `#_internal` and full SCXML internal target, equal variants, null, session/invoke/parent, malformed. Internal event enters the correct internal dispatcher and never external routing. |
| IO-SCXML-003 | Omitted target routes to current session. Parent short/full target routes to parent only when parent exists; no-parent case produces the required communication error. |
| IO-SCXML-004 | Explicit session and invoke targets: valid/empty/Unicode/long IDs, prefixes only, wrong prefix/case, URI query/fragment extras, nonexistent service, current/other session, nested invoke. Parse exact target without prefix confusion. |
| IO-SCXML-005 | Router event fields: sender session/invoke ID, target service ID, origin processor ID, origin target, event name/type, send/invoke ID, data, delay. Compare state-machine sender versus invoked-service sender. |
| IO-SCXML-006 | Concurrent routing between many sessions and parent/child invokes, destruction races, dispatcher backpressure/error/cancel, and circular send-to-self/parent/child. No cross-session dispatch or lost ownership. |
| IO-REG-001 | `_ioprocessors` registry contains every enabled processor under required keys with correct location/target and immutable access; disabled processors absent; duplicate IDs/aliases resolve by explicit rule. |

## 5. HTTP I/O processor

Separate pure `HttpController` message/URI tests, in-memory handler outbound tests, and real loopback listener tests.

### 5.1 Target construction and matching

| ID | Required scenarios and oracle |
|---|---|
| IO-HTTP-001 | Construct session/invoke targets from public base URLs with root/nested/trailing-slash paths, default/nondefault port, HTTP/HTTPS, IPv4/IPv6/IDN, escaped path, query/fragment. Result is canonical and round-trips through matching. |
| IO-HTTP-002 | `TryMatchTarget` exact scheme/host/port/base-path/session-or-invoke segment. Test host case, IDN, path case, percent encoding, dot segments, base-prefix trap (`/base` vs `/baseevil`), missing/extra slash, empty ID, query/fragment, userinfo, and alternate port. Reject cross-origin/confusable targets. |
| IO-HTTP-003 | Target IDs with slash, `#`, `?`, `%`, spaces, Unicode, very long values, and normalization differences. Generation must escape and parsing must recover exactly or reject invalid IDs by contract. |

### 5.2 Outbound messages

| ID | Required scenarios and oracle |
|---|---|
| IO-HTTP-010 | Undefined/null payload gives no body; string gives UTF-8 text/plain; keyed string dictionary gives form encoding; mixed/unkeyed/nested list and boolean/number/date give XML. Test every numeric/date/string boundary and exact content type/charset/body. |
| IO-HTTP-011 | Form payload includes `_scxmleventname` once when event nondefault, then data in deterministic order. Empty/duplicate keys, repeated values, undefined/null/empty/non-ASCII/reserved characters, preexisting reserved key, and default event require explicit collision behavior. |
| IO-HTTP-012 | If event name is not in content, add it to query exactly once while preserving target query/fragment and escaping. Test default event, existing parameter, duplicate parameter, long/Unicode event. |
| IO-HTTP-013 | Set `Origin`, `SCXML-SendId`, and `SCXML-InvokeId` headers exactly when present; session/invoke sender origins; invalid header characters; no duplication across reused clients/messages. Header injection must fail safely. |
| IO-HTTP-014 | Max outbound size 0/unlimited and N>0 with body N−1/N/N+1. Cover multibyte text (byte size, not character count), form, XML, unknown/chunked length, content copy failure/cancel, and values near integer limits. Oversize must fail before network dispatch. |
| IO-HTTP-015 | HTTP success every 2xx, redirect policy, 3xx/4xx/5xx, DNS/connect/TLS/timeout/request-cancel/response-read failure, handler throwing, and client factory failure. Verify processor error mapping, request/client/response/content disposal, and no retry unless documented. |
| IO-HTTP-016 | Concurrent sends through clients returned by factory; token cancellation independent of configured client timeout; timeout infinite/zero/positive; race response versus cancel. No shared header/content mutation. |

### 5.3 Inbound messages

| ID | Required scenarios and oracle |
|---|---|
| IO-HTTP-020 | No body: event name from normalized query or HTTP method fallback. Test each HTTP method, absent/empty/whitespace/repeated/malformed/Unicode event query value and path match. |
| IO-HTTP-021 | `text/plain` with omitted/explicit charset, UTF variants, BOM, empty/whitespace/large content, chunked/non-seekable/slow stream, invalid bytes, and cancellation. Payload is exact string according to encoding policy. |
| IO-HTTP-022 | `application/x-www-form-urlencoded`: no/one/many fields, repeated keys, empty/null-like values, percent/plus/malformed encoding, reserved event field first/middle/last/repeated, absent event fallback, and size boundary. Preserve required ordering/duplicates. |
| IO-HTTP-023 | `text/xml`: every `DataModelValue` XML form, namespaces/mixed content, malformed/truncated/unsafe XML, decimal/boundary values, deep/large graph, cancellation. Cross converter tests from document 02. |
| IO-HTTP-024 | `application/json`: every value shape/type and numeric/date/string boundary, duplicate properties, arrays/objects, malformed/truncated/deep/large JSON, charset, cancellation, and cycle metadata if any. Assert documented conversion. |
| IO-HTTP-025 | Content types exact/case/parameters/whitespace, missing default, unsupported (`application/xml`, `+json`, binary, multipart) and malformed header. Supported set follows explicit contract; unsupported returns 415 without dispatch. |
| IO-HTTP-026 | Size limit N with actual bytes N−1/N/N+1; absent, valid, negative, nonnumeric, overflow, conflicting content lengths, chunked body, slow oversized stream, and spoofed query/header length. Never allocate/read beyond bounded tolerance; oversize returns 413. |
| IO-HTTP-027 | Parse `Origin`, send ID, invoke ID headers absent/empty/valid/malformed/repeated/injection/huge. Build event fields and origin type exactly. Invalid metadata yields controlled client error or defined omission, never process crash. |
| IO-HTTP-028 | Dispatcher success/error/cancel/timeout/unknown target/destroy race. Response status and close behavior are exact; body is not dispatched twice; all exception paths close response/input. Internal exceptions map to 500 without leaking sensitive details. |
| IO-HTTP-029 | Real loopback concurrency: 1/2/10/100 simultaneous requests, keep-alive/new connection, partial clients disconnecting, listener cancellation/restart/disposal, port conflict, and repeated host start/stop. No listener/task/socket leak. |

## 6. Named-pipe I/O processor

| ID | Required scenarios and oracle |
|---|---|
| IO-PIPE-001 | Enabled only when name configured. Generate and parse local/remote session/invoke targets; host/name case and IDN; loopback aliases; missing/extra segment; wrong scheme/port/fragment prefix; empty/escaped/Unicode/long ID; query/path fragment tricks. |
| IO-PIPE-002 | Pipe-name canonicalization and collision behavior for case variants. Invalid configured names/hosts are rejected before listener creation. Remote host and alternate pipe name route to exact endpoint. |
| IO-PIPE-003 | Serialize every incoming event field/data kind into persistence-bucket framing and deserialize equivalently. Verify deterministic byte format, timestamp, target service ID, exception response, null/optional fields, and cross-target-framework compatibility. |
| IO-PIPE-004 | Frame size prefix split across 1–8-byte reads, zero, negative, N−1/N/N+1 limit, max integer, >2 GB declaration, endian/corrupt bytes, EOF before prefix/body, body extra bytes, and malformed bucket. Reject before dangerous allocation and return pooled buffers cleared. |
| IO-PIPE-005 | Message body delivered 1 byte at a time, arbitrary chunking, blocked read/write, client disconnect, server disconnect, cancellation/timeout at connect/prefix/body/process/response, and stream error. Every pipe/CTS/pooled array is released. |
| IO-PIPE-006 | Sender validates response timestamp absent/matching/mismatching, error type none/exception/unknown/corrupt, exception message/text, partial response, and response after timeout. A mismatched response cannot be accepted for another request. |
| IO-PIPE-007 | Receiver processing success/failure/cancel. Send success/error response once where connection permits; preserve primary processing exception if response send also fails; continue host acceptance loop according to resilience contract. |
| IO-PIPE-008 | 1/many simultaneous clients up to and beyond server instance capacity; repeated connect/disconnect; same/different sessions; ordered delivery per connection; host cancellation/restart; duplicate event prevention under client retry. |
| IO-PIPE-009 | Fuzz frame and persistence payload with arbitrary bytes and lengths under strict time/allocation limits. No large allocation from untrusted prefix, stack overflow, process crash, pool corruption, or sensitive exception disclosure. |

## 7. Resource loaders and `Resource`

| ID | Required scenarios and oracle |
|---|---|
| RES-LOAD-001 | Provider selection for file, web, resx, unsupported/custom URI; relative URI against state-machine location; absolute URI; base with/without trailing slash; query/fragment; escaped path; wrong scheme; no provider; multiple providers. First applicable provider and errors are deterministic. |
| RES-LOAD-002 | File loader: existing/missing/directory, empty/large, relative traversal, UNC/symlink/case, permission denied, locked/replaced file, cancellation, and content type/encoding. Security context and base-directory policy prevent unauthorized reads. |
| RES-LOAD-003 | Web loader with 2xx/redirect/3xx/4xx/5xx, content type/charset, header propagation, timeout/cancel, partial/chunked/compressed/huge response, DNS/TLS failure, redirect to disallowed scheme/address, and disposal. Apply SSRF/network policy explicitly. |
| RES-LOAD-004 | Resx loader valid/missing assembly/resource/key, culture fallback, string/bytes/stream/wrong object type, escaped identifier, concurrent requests, unload context, and disposal. |
| RES-OBJ-001 | `Resource.GetContent`, `GetBytes`, and `GetStream(doNotCache true/false)` in every first-call and repeated-call order. Assert cache identity/copy policy, source stream consumption, encoding/BOM, returned stream writability/position, and behavior after disposal. |
| RES-OBJ-002 | Concurrent calls to content/bytes/stream, caller disposing returned stream, source read failure/cancel, resource dispose while reading, source sync/async dispose failure, repeated sync/async dispose. No double read, use-after-dispose, deadlock, or retained buffer beyond lifecycle. |
| RES-OBJ-003 | Cache sizes from 0 through large/resource limit. Verify intentional memory retention while resource lives and release after disposal; `doNotCache` does not accidentally retain; cancellation token interrupts blocked stream. |
| RES-SEC-001 | External resource acquisition is denied when XInclude/external access is disabled or security permissions lack access. Prove loaders/network/filesystem are never touched; nested child cannot gain parent permissions. |

## 8. External services and invoked state machines

| ID | Required scenarios and oracle |
|---|---|
| EXT-SVC-001 | Provider selection by canonical type/alias/custom/unknown, source/content/params, 0/1/many matching providers, provider create/init/start failure, cancellation, and disposal. |
| EXT-SVC-002 | Service global collection, per-session collection, scope manager, controller, event router: add/get/dispatch/remove normal and duplicate/unknown IDs; concurrent complete/cancel/dispatch/destroy; collection consistency and exactly-once cleanup. |
| EXT-SVC-003 | Invoked SCXML from inline raw content, data-model string content, relative/absolute source, and both supplied through public model. Define precedence/rejection; propagate location, parent session, invoke ID, type, arguments, and security. |
| EXT-SVC-004 | Child normal completion with every result value, child error/destroy/cancel, parent exit/destroy, nested depth, sibling children, recursive invoke cycle, and child that invokes parent-like source. Enforce recursion/resource limits and correct done/error event. |
| EXT-SVC-005 | Dispatch event while service active/completing/disposed, cancellation linked from caller and destroy token, one/many concurrent events, blocked child queue, and stale service ID. No dispatch occurs after disposal linearization. |
| EXT-SVC-006 | Sync disposal launches destroy through monitored task; async disposal awaits it. Test destroy fault/cancel/hang and repeated disposal. No unobserved task or child-session leak. |

## 9. Security contexts and permissions

| ID | Required scenarios and oracle |
|---|---|
| SEC-CTX-001 | Every security context type and permission flag individually and in combinations, including none/full/unknown bits. `HasPermissions` and `CheckPermissions` enforce subset semantics and stable diagnostics. |
| SEC-CTX-002 | Create nested trusted machine, normal machine, and invoked service from each parent permission set. Verify required create permission, exact inherited/reduced permissions, and no privilege escalation through nesting. |
| SEC-CTX-003 | I/O-bound task factory with/without permission: schedule one/many tasks, inline attempt, child attach, scheduler hiding, continuations, exception/cancel, and disposal. No-access fails before user delegate runs. |
| SEC-CTX-004 | Maximum I/O-bound concurrency is enforced (currently two where contractual): block tasks, count simultaneous execution, release, cancel, and nest scheduling. No starvation/deadlock and no thread explosion. |
| SEC-CTX-005 | Security context propagation across awaits, runtime callbacks, resource loads, HTTP/named pipe, invoked child, persistence suspend/resume, task creation, and execution-context suppression. Context cannot leak to unrelated host/session. |
| SEC-CTX-006 | Attack cases: malicious SCXML attempts file/network XInclude, external script/data, arbitrary URI send/invoke, recursive trusted-machine creation, reserved system mutation, and resource exhaustion. Each capability is denied at the earliest authorized boundary without side effect. |

## 10. Persistence storage primitives and data graph

| ID | Required scenarios and oracle |
|---|---|
| PERSIST-STORE-001 | In-memory and every production storage provider: begin/write/checkpoint/shrink/read/close normal path; empty log; one/many records; key boundaries; overwrite/remove subtree; nested buckets; reopened storage. |
| PERSIST-STORE-002 | Inject failure/cancel before, during, and after each storage operation, including partial/short/torn write, flush failure, checkpoint failure, shrink failure, read corruption, and dispose failure. Previous committed state remains readable; uncommitted state is not presented as committed. |
| PERSIST-STORE-003 | Corrupt inputs: truncated prefix/body, wrong operation/type/version, unknown enum, missing/duplicate key, invalid reference ID, forward/back reference cycle, oversized length/count, integer overflow, random bytes. Reject in bounded time/allocation with `PersistenceException`, not incidental runtime failure. |
| PERSIST-DATA-001 | Persist and restore every `DataModelValue` kind/boundary, list shape, key, metadata, access mode, lazy policy, shared reference, direct/indirect cycle, and large/deep graph. Preserve reference identity where the persistence format promises it. |
| PERSIST-DATA-002 | Persist exceptions, event fields/data, queues, ordered sets, key lists, configuration, histories, active invokes, IDs, locations, options/levels, interpreter method state, and completion/done data. Missing/extra fields follow explicit version policy. |
| PERSIST-DATA-003 | Repeated store/load/store reaches byte or semantic fixed point; deterministic output independent of dictionary hash order, culture, process, target framework, and session concurrency. |

## 11. Interpreter suspension, checkpoints, and exact resume

Run the complete matrix across `None`, `StableState`, `Event`, `Transition`, and `ExecutableAction` persistence levels. Instrument named hooks immediately before and after every awaited call and externally visible side effect.

| ID | Required scenarios and oracle |
|---|---|
| PERSIST-LEVEL-001 | Level `None` never persists/suspends. Each higher level checkpoints exactly at its promised granularity and no more; lower levels omit finer-grained checkpoints. Count/order checkpoints for representative and generated macrosteps. |
| PERSIST-SUSP-001 | Request suspension before start, data init, global script, initial entry, stable wait, external dequeue, event processing, transition selection, each exit/action/entry, internal event, invoke start/cancel/finalize, send/cancel, completion, and destroy. |
| PERSIST-SUSP-002 | At each hook, suspend just before operation, during an asynchronously blocked operation, just after operation but before checkpoint, during checkpoint, and just after checkpoint. Kill the original scope and resume from durable bytes in a fresh host. |
| PERSIST-SUSP-003 | For pure interpreter actions, resumed trace/configuration/data/queues/result must equal uninterrupted execution. For externally visible side effects, assert documented at-most-once/at-least-once/exactly-once semantics and use idempotency IDs where required. No unexplained duplication is acceptable. |
| PERSIST-SUSP-004 | Resume root/state IDs and compiled model nodes: explicit/generated IDs, deep states, histories, transitions/actions, modified/reordered/incompatible definition, same semantic definition re-parsed, missing target, duplicate ID. Definition mismatch must be detected, not misbound by an index. |
| PERSIST-SUSP-005 | Preserve early/late data initialization completion flags. Resume cannot repeat a completed initializer/script/action unless delivery semantics require it, and cannot skip an uncommitted one. Use counted side effects at every level. |
| PERSIST-SUSP-006 | Preserve `_event`, internal/external queue order, current event consumption position, pending error events, selected transitions, exit/entry work sets, active configuration/history, states-to-invoke, and interpreter running/lifecycle flags. |
| PERSIST-SUSP-007 | Preserve active invokes and external-service scopes, matching invoke IDs, finalize/autoforward state, child completion racing suspend, cancelled/completed service, and resumable versus nonresumable provider behavior. |
| PERSIST-SUSP-008 | Preserve delayed events with send ID, absolute fire time, origin/target/data, cancellation journal, and remaining delay. Resume before/exactly/after fire time, clock rollback/forward, duplicate add/remove records, overdue event, and same-ID group. |
| PERSIST-SUSP-009 | Concurrent suspension request, external dispatch, delayed fire, invoke completion, host destroy, idle timeout, and storage failure. Establish one winner by linearization; durable state and live collections agree. |
| PERSIST-SUSP-010 | Cancellation during persistence and resume initialization. A cancelled waiter cannot leave the storage lock/semaphore held or a half-registered session. Retry behavior is deterministic. |
| PERSIST-SUSP-011 | Resume same snapshot once, repeatedly, and concurrently in same/different host. Enforce intended single-owner/session rule; reject duplicate activation or provide isolated clone semantics explicitly. |
| PERSIST-SUSP-012 | Complete/destroyed machine snapshots and result retrieval: define whether resumable; never run exit/final actions or send done events twice. Cleanup durable records according to storage contract. |

## 12. Persistent scheduler journal and recovery

| ID | Required scenarios and oracle |
|---|---|
| PERSIST-SCHED-001 | Journal add one/many events, remove one/group, interleaved add/remove, no-op remove, same ID reuse, record index boundary, and concurrent operations serialized by lock. Reopen reconstructs exact pending set. |
| PERSIST-SCHED-002 | Checkpoint failure after in-memory add/remove journal mutation rolls record index and subtree back exactly. Retry succeeds without duplicate/missing record. Inject cancellation waiting for and holding the lock. |
| PERSIST-SCHED-003 | Initialization scans valid log, unknown operation, missing event/ref ID, remove nonexistent/already removed, duplicate remove, truncated final record, corrupt event. Controlled failure or documented recovery; never dispatch corrupt event. |
| PERSIST-SCHED-004 | Compaction/shrink after removals rewrites only live events with stable semantics, checkpoints before shrink, survives failure at every rewrite/checkpoint/shrink point, and reopens consistently. |
| PERSIST-SCHED-005 | Restored event overdue/future/exactly now; cancellation racing restore; dispatch removes durable record before/after side effect according to delivery contract. Crash at that boundary exposes and validates duplicate/loss semantics. |
| PERSIST-SCHED-006 | Date/time and clock behavior: UTC storage, extreme delay, leap/daylight irrelevant, backward/forward wall-clock change, monotonic scheduling adapter where used. No negative overflow or multi-year `Task.Delay` failure. |

## 13. Cross-target, compatibility, and platform matrix

| ID | Required scenarios and oracle |
|---|---|
| PLAT-COMPAT-001 | Run deterministic semantic suite on every target framework in the test project and every source-supported target that can be hosted. Differences require an explicit compatibility decision. |
| PLAT-COMPAT-002 | Create persistence bytes on each target, read on every other compatible target, and compare resumed result. Include numeric/date/XML/event/invoke/queue graphs and old/new format fixtures. |
| PLAT-COMPAT-003 | Named-pipe tests on supported Windows lanes; case/host/IDN and framing compatibility. Unsupported platforms must report capability cleanly rather than fail unrelated test discovery. |
| PLAT-COMPAT-004 | HTTP and file path behavior on Windows and at least one Unix lane where supported: path case/separators, IPv6, certificate store independence, newline/culture/timezone. |
| PLAT-COMPAT-005 | Build/run under invariant globalization and representative cultures/time zones. IDs, URIs, numeric/date serialization, XPath, delay parsing, ordering, and persistence must remain semantically identical. |

## 14. Mandatory race schedules

Use a deterministic interleaving harness, not repeated hope-based loops, for at least these two-operation races. Pause both operations immediately before their state-changing point and run both orders plus simultaneous release:

- dispatch × destroy;
- dispatch × suspend;
- complete × cancel invoke;
- external event × delayed event fire;
- cancel send × delayed fire;
- schedule × scheduler dispose;
- state exit × invoke start;
- finalize event × parent exit;
- idle timeout × new event;
- resource read × resource dispose;
- HTTP/named-pipe receive × listener dispose;
- persistence checkpoint × host cancellation;
- persistent event add/remove × crash/reopen;
- duplicate resume × session registration;
- scope initialization × scope disposal.

For each race, assert one of a small set of fully valid outcomes and prohibit hybrid state, duplicate side effect, leaked registration, deadlock, and unobserved exception. Record the chosen linearization point in the test description.
