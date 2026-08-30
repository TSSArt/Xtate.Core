// Copyright © 2019-2026 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Xtate.Core.Test.Exhaustive.Interpreter;

/// <summary>
///     Authority-derived Phase 2 scenarios deliberately expressed through a test-side contract driver.
///     The driver is intentionally unresolved until the later harness campaign; the case table is the
///     executable-source specification and keeps every independently reportable requirement stable.
/// </summary>
[TestClass]
[TestCategory("Exhaustive.Generated")]
public sealed class Phase2RemainingRequirementsGeneratedTests
{
	// case_id | requirement_ids | description | input/fixture | stimulus | expected | exception/event | forbidden | partitions | dimensions
	private static readonly GeneratedPhaseTwoCase[] CaseTable =
	[
		C(
			id: "SCXML-LIFE-001-CASE-001", requirement: "SCXML-LIFE-001", description: "Normal completion publishes accepted, initialization, started, stable, completion and disposal once in order.",
			fixture: "root final; trace", stimulus: "start", expected: "Accepted>Initialized>Started>Stable>Completed>Disposed", exceptionOrEvent: "none",
			forbidden: "duplicate lifecycle notification", partitions: "positive,cleanup", dimensions: "final completion"),
		C(
			id: "SCXML-LIFE-002-CASE-001", requirement: "SCXML-LIFE-002", description: "Root selection uses initial attribute or first root child; no-root and illegal root initial are rejected.",
			fixture: "no-root, first-child, initial attribute, invalid target/root initial element", stimulus: "start", expected: "valid: exact selected entry; invalid: validation failure",
			exceptionOrEvent: "validation error", forbidden: "entry of unselected state", partitions: "positive,negative,malformed", dimensions: "root selection"),
		C(
			id: "SCXML-LIFE-003-CASE-001", requirement: "SCXML-LIFE-003",
			description: "Early data initializes document-order once; late data initializes first-entry only including reentry, history and parallel paths.",
			fixture: "early/late nested data counters", stimulus: "enter, exit, reenter, history", expected: "early=[root,parent,child] once; late=one initialization per first entry",
			exceptionOrEvent: "none", forbidden: "repeat initialization after late reentry", partitions: "positive,boundary,error", dimensions: "binding"),
		C(
			id: "SCXML-LIFE-004-CASE-001", requirement: "SCXML-LIFE-004",
			description: "Data initialization chooses expr/content/src/undefined/arguments without overwriting system variables and fails atomically on bad input.",
			fixture: "five data sources, duplicate/unknown args, read-only system value", stimulus: "start", expected: "declared override values only; failed declaration has controlled startup error",
			exceptionOrEvent: "error.execution", forbidden: "changed _event or partial forbidden write", partitions: "positive,negative,error,security", dimensions: "startup arguments"),
		C(
			id: "SCXML-LIFE-005-CASE-001", requirement: "SCXML-LIFE-005",
			description: "Root script runs after root data initialization and before root initial entry; failures and cancellation stop startup cleanly.",
			fixture: "inline/external/empty/failing/async/cancel script", stimulus: "start at script await", expected: "data>global-script>initial-entry; failure emits error and disposes resource",
			exceptionOrEvent: "error.execution", forbidden: "initial entry after script failure", partitions: "positive,error,cancellation,resource", dimensions: "global script"),
		C(
			id: "SCXML-LIFE-006-CASE-001", requirement: "SCXML-LIFE-006", description: "Stable callback occurs only after eventless closure, internal FIFO drain and deferred invokes are settled.",
			fixture: "eventless chain plus raised event plus invoke", stimulus: "start", expected: "one Stable after all three completion observations", exceptionOrEvent: "none",
			forbidden: "stable callback in intermediate configuration", partitions: "positive,concurrency", dimensions: "macrostep closure"),
		C(
			id: "SCXML-LIFE-007-CASE-001", requirement: "SCXML-LIFE-007",
			description: "Start/destroy/dispose/queue-close races have a bounded single winner and reject operations after terminal state.",
			fixture: "barrier-controlled start/destroy/dispatch/dispose", stimulus: "release each race gate", expected: "one terminal result and all waiters complete within 100 operations",
			exceptionOrEvent: "ObjectDisposedException or terminal rejection", forbidden: "two starts, leaked blocked waiter", partitions: "negative,cancellation,concurrency,cleanup",
			dimensions: "lifecycle races"),
		C(
			id: "SCXML-LIFE-008-CASE-001", requirement: "SCXML-LIFE-008", description: "Top-level final returns done data once, cancels activity and linearizes racing event acceptance at completion.",
			fixture: "root final with delayed send/invoke and race event", stimulus: "complete/event race", expected: "one result and no further consumed event; activity cancelled",
			exceptionOrEvent: "none", forbidden: "second done data or post-completion transition", partitions: "positive,concurrency,cleanup", dimensions: "root completion"),
		C(
			id: "SCXML-STATE-001-CASE-001", requirement: "SCXML-STATE-001",
			description: "Atomic, compound, parallel and final entry follows ancestor-before-descendant/document order with shared ancestors entered once.",
			fixture: "nested compound, two-region parallel, shared-target ancestor", stimulus: "enter targets", expected: "exact entry trace parent>children document order; legal configuration",
			exceptionOrEvent: "none", forbidden: "duplicate shared ancestor entry", partitions: "positive", dimensions: "entry topology"),
		C(
			id: "SCXML-STATE-002-CASE-001", requirement: "SCXML-STATE-002",
			description: "Exiting nested active configuration is deepest-first; onexit precedes transition content/removal and invoke cancellation follows contract.",
			fixture: "parent/child invokes and trace actions", stimulus: "transition outward", expected: "child-onexit>parent-onexit>transition; each ancestor once", exceptionOrEvent: "none",
			forbidden: "parent exit before child exit", partitions: "positive,cleanup", dimensions: "exit ordering"),
		C(
			id: "SCXML-STATE-003-CASE-001", requirement: "SCXML-STATE-003",
			description: "Compound initial content runs between parent entry and selected child entry for element, attribute, default and parallel multi-target forms.", fixture: "four initial forms",
			stimulus: "enter compound", expected: "parent-entry>initial-content>child entries", exceptionOrEvent: "none", forbidden: "child entry before initial content", partitions: "positive",
			dimensions: "initial entry"),
		C(
			id: "SCXML-STATE-004-CASE-001", requirement: "SCXML-STATE-004",
			description: "Parallel done event appears once only when all regions complete, including nested/re-exited/escaped activations.", fixture: "staggered and nested parallel",
			stimulus: "complete regions and escape", expected: "done.state.parallel exactly once per activation after last region", exceptionOrEvent: "done.state.parallel",
			forbidden: "done while a region nonfinal", partitions: "positive,boundary", dimensions: "parallel completion"),
		C(
			id: "SCXML-STATE-005-CASE-001", requirement: "SCXML-STATE-005", description: "Final entry actions precede correctly addressed done.state payload and parent completion processing.",
			fixture: "root/compound/parallel/nested final with donedata", stimulus: "enter final", expected: "final-entry>done.state.parent(payload)>parent processing",
			exceptionOrEvent: "done.state.parent", forbidden: "payload twice or done before final action", partitions: "positive,error", dimensions: "final done data"),
		C(
			id: "SCXML-STATE-006-CASE-001", requirement: "SCXML-STATE-006",
			description: "Generated microsteps maintain basic-state, compound-child, parallel-region and exited-state configuration invariants.", fixture: "all nonisomorphic legal trees <= 6 nodes",
			stimulus: "each macrostep", expected: "independent configuration oracle equals runtime after every microstep", exceptionOrEvent: "none",
			forbidden: "inactive/exited state in configuration", partitions: "positive,reliability", dimensions: "generated configuration"),
		C(
			id: "SCXML-STATE-007-CASE-001", requirement: "SCXML-STATE-007",
			description: "Depth 1-6, inline-capacity+1 and 100+ preserve order; configured over-limit fails controlledly rather than overflowing.", fixture: "depth 1,2,3,4,5,6,capacity+1,101,limit+1",
			stimulus: "start", expected: "valid ordered entry; over-limit resource-policy failure", exceptionOrEvent: "limit error", forbidden: "StackOverflowException or partial startup",
			partitions: "boundary,resource,scalability", dimensions: "topology depth"),
		C(
			id: "SCXML-TRANS-001-CASE-001", requirement: "SCXML-TRANS-001",
			description: "Eventless selection precedes queued events, handles false/error/new chains and bounds cycles across parallel regions.",
			fixture: "true/false/error cond chains, external event, parallel", stimulus: "macrostep", expected: "eventless closure before event; condition error false plus platform event",
			exceptionOrEvent: "error.execution", forbidden: "external event selected before enabled eventless", partitions: "positive,error,reliability", dimensions: "eventless priority"),
		C(
			id: "SCXML-TRANS-002-CASE-001", requirement: "SCXML-TRANS-002",
			description: "Descriptor matching preserves SCXML hierarchical boundaries for exact,prefix,wildcard,whitespace,empty,Unicode and case inputs.", fixture: "descriptor matrix",
			stimulus: "match event name", expected: "case-table boolean match exactly", exceptionOrEvent: "none", forbidden: "foo matches foobar", partitions: "positive,negative,boundary",
			dimensions: "event descriptors"),
		C(
			id: "SCXML-TRANS-003-CASE-001", requirement: "SCXML-TRANS-003",
			description: "First document-order matching true transition wins and later conditions are never evaluated; false/error permits specified successor.",
			fixture: "ordered side-effecting conditions", stimulus: "dispatch event", expected: "selected first true; evaluation trace stops at selection", exceptionOrEvent: "error.execution",
			forbidden: "later condition side effect after winner", partitions: "positive,error", dimensions: "document order"),
		C(
			id: "SCXML-TRANS-004-CASE-001", requirement: "SCXML-TRANS-004", description: "Descendant preempts conflicting ancestor while disjoint orthogonal transitions coexecute.",
			fixture: "same/ancestor/sibling/cousin/parallel sources", stimulus: "dispatch matching event", expected: "reference selected transition set", exceptionOrEvent: "none",
			forbidden: "conflicting ancestor action", partitions: "positive", dimensions: "source priority"),
		C(
			id: "SCXML-TRANS-005-CASE-001", requirement: "SCXML-TRANS-005",
			description: "Independent domain oracle computes exact exit sets for targetless,self,internal,external,ancestor,sibling,cross-region,multi-target,history and root.",
			fixture: "transition domain matrix", stimulus: "select transition", expected: "runtime exit set and domain equal reference", exceptionOrEvent: "none",
			forbidden: "state outside reference exit domain exited", partitions: "positive,boundary", dimensions: "transition domain"),
		C(
			id: "SCXML-TRANS-006-CASE-001", requirement: "SCXML-TRANS-006",
			description: "Internal/external/default semantics produce exact reentry and data/invoke reinitialization consequences; invalid type rejects.",
			fixture: "compound-descendant,self,outside,type values", stimulus: "dispatch event", expected: "specified exited/reentered states and initialization counts",
			exceptionOrEvent: "validation error", forbidden: "internal descendant reenters compound", partitions: "positive,negative", dimensions: "transition type"),
		C(
			id: "SCXML-TRANS-007-CASE-001", requirement: "SCXML-TRANS-007",
			description: "Targetless transitions execute content only, preserve configuration and correctly queue chained raises/failures.", fixture: "eventless/eventful/condition/raise/failure",
			stimulus: "dispatch", expected: "unchanged configuration plus exact content trace", exceptionOrEvent: "error.execution", forbidden: "entry or exit caused by targetless transition",
			partitions: "positive,error", dimensions: "targetless"),
		C(
			id: "SCXML-TRANS-008-CASE-001", requirement: "SCXML-TRANS-008",
			description: "Legal multi-target orthogonal descendants enter shared ancestors once/document order; illegal relation classes reject.", fixture: "all target relationship classes",
			stimulus: "dispatch", expected: "legal configuration/reference order or validation failure", exceptionOrEvent: "validation error", forbidden: "duplicate ancestor entry",
			partitions: "positive,negative", dimensions: "multi-target"),
		C(
			id: "SCXML-TRANS-009-CASE-001", requirement: "SCXML-TRANS-009", description: "Conflict resolver matches independent model for 2,3,many identical/intersecting/disjoint/parallel exit sets.",
			fixture: "generated enabled transition sets", stimulus: "select", expected: "exact maximal nonconflicting set reference", exceptionOrEvent: "none",
			forbidden: "lower-priority conflicting transition selected", partitions: "positive,reliability", dimensions: "conflict resolution"),
		C(
			id: "SCXML-TRANS-010-CASE-001", requirement: "SCXML-TRANS-010", description: "Every selected microstep orders all exits, transition content and entries as one total trace.",
			fixture: "nested multi-transition trace actions", stimulus: "dispatch", expected: "all exit actions>all transition actions>all entry actions", exceptionOrEvent: "none",
			forbidden: "entry before transition content", partitions: "positive", dimensions: "microstep order"),
		C(
			id: "SCXML-TRANS-011-CASE-001", requirement: "SCXML-TRANS-011",
			description: "Condition fault acts false, queues platform error and leaves no partial transition effects under each error policy.", fixture: "throwing condition plus alternative",
			stimulus: "dispatch", expected: "alternative behavior plus error.execution and policy result", exceptionOrEvent: "error.execution", forbidden: "partial exit from failed condition",
			partitions: "error", dimensions: "condition fault"),
		C(
			id: "SCXML-TRANS-012-CASE-001", requirement: "SCXML-TRANS-012",
			description: "Executable failure stops remaining sequence but retains completed side effects across exit/transition/entry/initial/finalize/foreach/if/donedata.",
			fixture: "fault at each action position", stimulus: "trigger action", expected: "prefix effects retained, suffix absent, exact error event", exceptionOrEvent: "error.execution",
			forbidden: "action after fault", partitions: "error,cancellation", dimensions: "action fault"),
		C(
			id: "SCXML-TRANS-013-CASE-001", requirement: "SCXML-TRANS-013",
			description: "Cancellation at each named await boundary linearizes to whole before/after configuration and completes teardown.", fixture: "gate selection/exit/action/entry/invoke/queue",
			stimulus: "cancel gate", expected: "pre-state or post-state only; zero resources", exceptionOrEvent: "OperationCanceledException", forbidden: "hybrid configuration",
			partitions: "cancellation,cleanup", dimensions: "await boundaries"),
		C(
			id: "SCXML-TRANS-014-CASE-001", requirement: "SCXML-TRANS-014",
			description: "Small legal generated machines and streams match independent reference configuration and trace every macrostep with shrinkable seed.",
			fixture: "nonisomorphic trees <= practical bound; seeded streams", stimulus: "replay seed", expected: "exact reference trace/configuration per step", exceptionOrEvent: "none",
			forbidden: "unshrunk mismatch or divergent trace", partitions: "reliability,scalability", dimensions: "differential"),
		C(
			id: "SCXML-HIST-001-CASE-001", requirement: "SCXML-HIST-001", description: "Shallow retains immediate children and deep retains atomic descendants across compound/parallel nested exits.",
			fixture: "history topology matrix", stimulus: "exit and reenter", expected: "remembered shallow/deep configuration exact", exceptionOrEvent: "none",
			forbidden: "deep restores only immediate child", partitions: "positive", dimensions: "history capture"),
		C(
			id: "SCXML-HIST-002-CASE-001", requirement: "SCXML-HIST-002", description: "Uninitialized history takes default content once; initialized history restores without default content.",
			fixture: "history default counter", stimulus: "first then later entry", expected: "counter=1 and stored target restored", exceptionOrEvent: "none",
			forbidden: "default content on stored history", partitions: "positive", dimensions: "history default"),
		C(
			id: "SCXML-HIST-003-CASE-001", requirement: "SCXML-HIST-003",
			description: "Shallow reentry applies child default; deep restores descendants directly with ancestor entry and data/invoke rules.", fixture: "nested child defaults/data/invokes",
			stimulus: "reenter shallow/deep", expected: "exact entry trace and initialization counts", exceptionOrEvent: "none", forbidden: "shallow directly restores grandchild",
			partitions: "positive", dimensions: "history reentry"),
		C(
			id: "SCXML-HIST-004-CASE-001", requirement: "SCXML-HIST-004",
			description: "Multiple shallow/deep histories remain independent through overwrite, never-active, nested and multi-target paths.", fixture: "two histories and capture paths",
			stimulus: "capture/target histories", expected: "each history's own stored/default result", exceptionOrEvent: "none", forbidden: "cross-history contamination",
			partitions: "positive,negative", dimensions: "history isolation"),
		C(
			id: "SCXML-HIST-005-CASE-001", requirement: "SCXML-HIST-005", description: "History IDs/targets survive serialization and resume; corrupt/missing persisted history is controlled failure.",
			fixture: "serialized and corrupt snapshots", stimulus: "serialize,resume", expected: "same restored configuration or persistence/validation error", exceptionOrEvent: "persistence error",
			forbidden: "illegal active configuration", partitions: "positive,malformed,error", dimensions: "history persistence"),
		C(
			id: "SCXML-EVENT-001-CASE-001", requirement: "SCXML-EVENT-001",
			description: "Event names retain default/segments/dots/whitespace/wildcard literals/long Unicode/case identity and reject invalid boundary input.", fixture: "event-name lexical matrix",
			stimulus: "construct/match", expected: "exact segments/string/match or boundary error", exceptionOrEvent: "ArgumentException", forbidden: "normalization or case folding",
			partitions: "positive,negative,boundary", dimensions: "event name"),
		C(
			id: "SCXML-EVENT-002-CASE-001", requirement: "SCXML-EVENT-002",
			description: "_event has correct field types and values before start, eventless, internal/external/platform/invoke, boundaries and resume.", fixture: "events of each origin",
			stimulus: "inspect _event at hooks", expected: "specified fields or Undefined outside event processing", exceptionOrEvent: "none", forbidden: "stale previous event fields",
			partitions: "positive", dimensions: "_event context"),
		C(
			id: "SCXML-EVENT-003-CASE-001", requirement: "SCXML-EVENT-003",
			description: "Internal FIFO has priority over next external; producers from exit/transition/entry/finalize/error join exact order.", fixture: "multi-producer trace",
			stimulus: "enqueue internal and external", expected: "internal producer order before external", exceptionOrEvent: "none", forbidden: "external consumed before queued internal",
			partitions: "positive", dimensions: "internal queue"),
		C(
			id: "SCXML-EVENT-004-CASE-001", requirement: "SCXML-EVENT-004",
			description: "External FIFO spans host/send/invoke/restored/HTTP/pipe and concurrent dispatch follows documented ticket linearization without torn payload.",
			fixture: "source matrix and barrier dispatch", stimulus: "dispatch sequence", expected: "ticket order, one copy per payload", exceptionOrEvent: "none",
			forbidden: "loss, duplicate, torn payload", partitions: "concurrency,reliability", dimensions: "external queue"),
		C(
			id: "SCXML-EVENT-005-CASE-001", requirement: "SCXML-EVENT-005", description: "Only matching active invoke event finalizes before selection; stale/nonmatching IDs do neither.",
			fixture: "matching/stale invoke events", stimulus: "dispatch", expected: "matching finalize>selection; nonmatching ignored", exceptionOrEvent: "none", forbidden: "finalize for stale ID",
			partitions: "positive,negative", dimensions: "invoke event filtering"),
		C(
			id: "SCXML-EVENT-006-CASE-001", requirement: "SCXML-EVENT-006",
			description: "Closing/cancelling/faulting queues in every lifecycle phase wakes waiters and releases payloads under configured termination.",
			fixture: "internal/external queue phase matrix", stimulus: "close/cancel/fault", expected: "waiters complete and payload ledger zero",
			exceptionOrEvent: "ChannelClosedException or cancellation", forbidden: "blocked waiter/resource retention", partitions: "error,cancellation,cleanup", dimensions: "queue termination"),
		C(
			id: "SCXML-EVENT-007-CASE-001", requirement: "SCXML-EVENT-007", description: "Dispatch snapshots payloads across source mutation, transition/finalize mutation and sessions.",
			fixture: "mutable nested payload", stimulus: "mutate after each boundary", expected: "each recipient observes its specified snapshot", exceptionOrEvent: "none",
			forbidden: "cross-event/session alias mutation", partitions: "security,reliability", dimensions: "payload isolation"),
		C(
			id: "SCXML-EVENT-008-CASE-001", requirement: "SCXML-EVENT-008",
			description: "Storms/self-send/recursive raise/eventless cycles hit bounded livelock policy while finite progress beyond threshold completes.",
			fixture: "loop and long-progress generators", stimulus: "run bounded steps", expected: "loop controlled termination; progressing finite chain completes",
			exceptionOrEvent: "livelock error", forbidden: "false positive on progressing chain", partitions: "resource,reliability,scalability", dimensions: "event amplification"),
		C(
			id: "SCXML-EXEC-001-CASE-001", requirement: "SCXML-EXEC-001", description: "Raise enqueues exactly one payloadless internal event in order; missing/invalid event rejects on every route.",
			fixture: "raise route/error/finalize/exit matrix", stimulus: "execute raise", expected: "one named internal event or validation/evaluation error", exceptionOrEvent: "validation error",
			forbidden: "external event or payload", partitions: "positive,negative", dimensions: "raise"),
		C(
			id: "SCXML-EXEC-002-CASE-001", requirement: "SCXML-EXEC-002",
			description: "If evaluates lazily/in order and executes exactly first true or else, including nested/empty/fault/cancel branches.", fixture: "instrumented branch conditions",
			stimulus: "execute if", expected: "condition trace through winner only; selected actions only", exceptionOrEvent: "error.execution or cancellation",
			forbidden: "evaluation/action in skipped branch", partitions: "positive,error,cancellation", dimensions: "if"),
		C(
			id: "SCXML-EXEC-003-CASE-001", requirement: "SCXML-EXEC-003",
			description: "Foreach snapshots or live-iterates according to contract once, orders items/index and restores scopes after success/error/cancel/nesting.",
			fixture: "mutable collection/nested scopes", stimulus: "execute foreach", expected: "defined iteration trace and original scope restored",
			exceptionOrEvent: "error.execution or cancellation", forbidden: "scope variable retained", partitions: "positive,error,cancellation", dimensions: "foreach"),
		C(
			id: "SCXML-EXEC-004-CASE-001", requirement: "SCXML-EXEC-004",
			description: "Log obeys enablement/evaluation contract, invariant formatting, zero mutation and controlled logger/evaluator fault without retention.",
			fixture: "all data kinds/cultures/faulting logger", stimulus: "execute log", expected: "exact log/no-log and unchanged data", exceptionOrEvent: "error.execution",
			forbidden: "culture-dependent value or retained graph", partitions: "positive,error,resource", dimensions: "log"),
		C(
			id: "SCXML-EXEC-005-CASE-001", requirement: "SCXML-EXEC-005",
			description: "Assign evaluates RHS/location in required order and atomically handles undefined/empty/multi-location operations.", fixture: "instrumented expressions and locations",
			stimulus: "execute assign", expected: "order trace and all-or-no required mutation", exceptionOrEvent: "error.execution", forbidden: "partial atomic mutation",
			partitions: "positive,error", dimensions: "assign orchestration"),
		C(
			id: "SCXML-EXEC-006-CASE-001", requirement: "SCXML-EXEC-006",
			description: "Scripts cover inline/external/global/local/no-op/async/error/cancel/unsupported/media/base/disposal/repetition.", fixture: "script source matrix", stimulus: "execute script",
			expected: "specified effect once and external stream disposed", exceptionOrEvent: "error.execution or cancellation", forbidden: "reused stale script result/stream leak",
			partitions: "positive,error,cancellation,resource", dimensions: "script"),
		C(
			id: "SCXML-EXEC-007-CASE-001", requirement: "SCXML-EXEC-007",
			description: "Custom actions resolve namespace/name, receive content/context, preserve order and report missing/multiple/fault/cancel/dispose providers.", fixture: "provider matrix",
			stimulus: "execute custom action", expected: "exact provider invocation or controlled error", exceptionOrEvent: "error.execution", forbidden: "unknown content silently ignored",
			partitions: "positive,error,cancellation", dimensions: "custom action"),
		C(
			id: "SCXML-EXEC-008-CASE-001", requirement: "SCXML-EXEC-008", description: "Blocks 0,1,2,255,256,1k,100k preserve order, cancellation and linear resources without recursion overflow.",
			fixture: "action count matrix", stimulus: "execute/cancel midpoint", expected: "ordered prefix/full result and bounded resource ledger", exceptionOrEvent: "OperationCanceledException",
			forbidden: "StackOverflowException or reordered actions", partitions: "boundary,cancellation,resource,scalability", dimensions: "action blocks"),
		C(
			id: "SCXML-DATA-001-CASE-001", requirement: "SCXML-DATA-001",
			description: "Data declarations support no source/expr/content/src across binding,args,reentry,media,empty,malformed and failures.", fixture: "data source cross-product",
			stimulus: "initialize/reenter", expected: "case-specified value/count or controlled error", exceptionOrEvent: "error.execution", forbidden: "wrong source precedence or partial init",
			partitions: "positive,error", dimensions: "data declarations"),
		C(
			id: "SCXML-DATA-002-CASE-001", requirement: "SCXML-DATA-002", description: "Params evaluate expr/location in order across null/scalar/list/XML/repeats/empty names and snapshot aliases.",
			fixture: "parameter value matrix", stimulus: "construct payload", expected: "ordered named payload or controlled error", exceptionOrEvent: "error.execution",
			forbidden: "alias mutation or later evaluation after failure", partitions: "positive,negative,error", dimensions: "params"),
		C(
			id: "SCXML-DATA-003-CASE-001", requirement: "SCXML-DATA-003", description: "Namelist evaluates locations once lexical order, uses intended keys and enforces duplicate/combination rules.",
			fixture: "instrumented namelist/params/content", stimulus: "construct payload", expected: "one evaluation per lexical location and legal payload", exceptionOrEvent: "validation error",
			forbidden: "duplicate key overwrite", partitions: "positive,negative", dimensions: "namelist"),
		C(
			id: "SCXML-DATA-004-CASE-001", requirement: "SCXML-DATA-004",
			description: "Content expr/body preserves whitespace/text/XML/mixed namespaces/special/large data and handles malformed/cache failures.", fixture: "content lexical/source matrix",
			stimulus: "evaluate repeatedly", expected: "exact typed content and defined cache retry behavior", exceptionOrEvent: "error.execution",
			forbidden: "escaped/lost namespace or stale failed cache", partitions: "positive,malformed,resource", dimensions: "content"),
		C(
			id: "SCXML-DATA-005-CASE-001", requirement: "SCXML-DATA-005",
			description: "Done data none/params/content/all kinds appears exactly once only when final entered; errors/cancel do not duplicate.", fixture: "final/nonfinal donedata matrix",
			stimulus: "enter or bypass final", expected: "one addressed payload or no evaluation", exceptionOrEvent: "error.execution or cancellation", forbidden: "done payload without final entry",
			partitions: "positive,error,cancellation", dimensions: "donedata"),
		C(
			id: "SCXML-DATA-006-CASE-001", requirement: "SCXML-DATA-006",
			description: "Writable/read-only/constant access and metadata survive conversion,event,invoke,persist/resume; forbidden writes are atomic.", fixture: "access flag route matrix",
			stimulus: "attempt write/roundtrip", expected: "metadata preserved; forbidden write fails unchanged", exceptionOrEvent: "InvalidOperationException",
			forbidden: "partial forbidden mutation", partitions: "security,error", dimensions: "data access"),
		C(
			id: "SCXML-SEND-001-CASE-001", requirement: "SCXML-SEND-001",
			description: "Send resolves literal/expressions exactly once/order and never calls router after required field failure/cancel.", fixture: "field expression error matrix",
			stimulus: "execute send", expected: "ordered successful route or error before router", exceptionOrEvent: "error.execution or cancellation", forbidden: "router call after early failure",
			partitions: "positive,error,cancellation", dimensions: "send resolution"),
		C(
			id: "SCXML-SEND-002-CASE-001", requirement: "SCXML-SEND-002",
			description: "Send IDs generate/propagate uniquely under concurrency/resume and idlocation failure has specified compensating schedule outcome.",
			fixture: "explicit/empty/duplicate/generated IDs/idlocation fault", stimulus: "send concurrently", expected: "unique IDs and contract-specified scheduled count",
			exceptionOrEvent: "error.execution", forbidden: "duplicate generated ID or orphan schedule", partitions: "concurrency,error", dimensions: "send ids"),
		C(
			id: "SCXML-SEND-003-CASE-001", requirement: "SCXML-SEND-003",
			description: "Send payload selection validates namelist/params/content conflicts, preserves raw strings/types and isolates data.", fixture: "payload form matrix",
			stimulus: "send/mutate source", expected: "exact payload or validation error", exceptionOrEvent: "validation error", forbidden: "payload alias or coerced raw string",
			partitions: "positive,negative,security", dimensions: "send payload"),
		C(
			id: "SCXML-SEND-004-CASE-001", requirement: "SCXML-SEND-004", description: "Delayed send boundaries/cancel-fire races dispatch exactly permitted count and never affect distinct IDs.",
			fixture: "virtual due-time/cancel matrix", stimulus: "advance clock and race gates", expected: "0 or 1 exact dispatch as policy permits", exceptionOrEvent: "none",
			forbidden: "duplicate/early/late dispatch", partitions: "boundary,concurrency,cleanup", dimensions: "delayed send"),
		C(
			id: "SCXML-SEND-005-CASE-001", requirement: "SCXML-SEND-005",
			description: "Routing resolves default/canonical/alias/custom/internal/session/parent/invoke targets and rejects malformed/unavailable routes with exact metadata.",
			fixture: "target/type route matrix", stimulus: "send", expected: "exact origin/origintype/sender/target or communication error", exceptionOrEvent: "error.communication",
			forbidden: "wrong target delivery", partitions: "positive,negative", dimensions: "routing"),
		C(
			id: "SCXML-SEND-006-CASE-001", requirement: "SCXML-SEND-006",
			description: "Router/scheduler sync/async fault,timeout,cancel,partial dispatch emits communication policy result and cleans tasks/schedules.", fixture: "faulting router/scheduler matrix",
			stimulus: "send", expected: "error.communication plus zero forgotten task/schedule", exceptionOrEvent: "error.communication", forbidden: "silent background failure",
			partitions: "error,cancellation,cleanup", dimensions: "send failure"),
		C(
			id: "SCXML-CANCEL-001-CASE-001", requirement: "SCXML-CANCEL-001",
			description: "Cancel resolves literal/expression IDs, rejects absent/wrong/empty, cancels matching one/many and preserves distinct ID events.",
			fixture: "ID/cancellation/concurrency matrix", stimulus: "cancel", expected: "only matching scheduled events removed", exceptionOrEvent: "validation error",
			forbidden: "distinct ID cancellation", partitions: "positive,negative,concurrency", dimensions: "cancel"),
		C(
			id: "SCXML-CANCEL-002-CASE-001", requirement: "SCXML-CANCEL-002",
			description: "Scheduler disposal for 0/1/many, cancellation faults, dispatch and simultaneous schedule/cancel aggregates errors and leaves zero timers/tasks.",
			fixture: "scheduler teardown matrix", stimulus: "dispose sync/async", expected: "consistent aggregate/primary error and zero ledger", exceptionOrEvent: "AggregateException",
			forbidden: "timer/task leak", partitions: "error,concurrency,cleanup", dimensions: "scheduler disposal"),
		C(
			id: "SCXML-INVOKE-001-CASE-001", requirement: "SCXML-INVOKE-001",
			description: "Invoke resolves type/src/content/id/params/namelist in order, rejects conflicts and handles source fault/cancel.", fixture: "invoke source resolution matrix",
			stimulus: "enter invoke state", expected: "ordered evaluation and exactly one provider start or controlled error", exceptionOrEvent: "error.execution or cancellation",
			forbidden: "provider start after resolution failure", partitions: "positive,error,cancellation", dimensions: "invoke resolution"),
		C(
			id: "SCXML-INVOKE-002-CASE-001", requirement: "SCXML-INVOKE-002",
			description: "Invoke IDs are unique; idlocation/ambient context is scoped to start/cancel/finalize and cleared across nested/concurrent terminal paths.",
			fixture: "nested concurrent invokes", stimulus: "start/cancel/finalize", expected: "unique IDs and no ambient value after terminal path", exceptionOrEvent: "none",
			forbidden: "ambient invoke ID leak", partitions: "concurrency,cleanup", dimensions: "invoke ids"),
		C(
			id: "SCXML-INVOKE-003-CASE-001", requirement: "SCXML-INVOKE-003", description: "Invokes begin only at stable point and never start if eventless/internal/external exit happens first.",
			fixture: "three immediate-exit paths", stimulus: "enter then exit", expected: "zero starts before exit; otherwise start after stable trace", exceptionOrEvent: "none",
			forbidden: "invoke start before stable", partitions: "positive", dimensions: "deferred invoke start"),
		C(
			id: "SCXML-INVOKE-004-CASE-001", requirement: "SCXML-INVOKE-004",
			description: "One/many/nested/parallel/source/content/service invokes maintain active registry and dispose exactly once under all terminal paths.",
			fixture: "invoke topology/provider fault matrix", stimulus: "start,complete,cancel,destroy", expected: "registry accurate and each invoke disposal count=1",
			exceptionOrEvent: "error.execution", forbidden: "duplicate dispose or orphan registry", partitions: "positive,error,cleanup", dimensions: "invoke lifecycle"),
		C(
			id: "SCXML-INVOKE-005-CASE-001", requirement: "SCXML-INVOKE-005",
			description: "State exit cancels active invokes in contract order across success/fault/hang/race/duplicate exit and teardown escalation.", fixture: "blocking invokes/onexit trace",
			stimulus: "exit/race completion", expected: "specified onexit-cancel order, bounded completion and one cancellation", exceptionOrEvent: "error.execution",
			forbidden: "hung invoke blocks teardown forever", partitions: "error,concurrency,cleanup", dimensions: "invoke cancellation"),
		C(
			id: "SCXML-INVOKE-006-CASE-001", requirement: "SCXML-INVOKE-006",
			description: "Matching returned done/error/arbitrary events finalize before selection; stale IDs never finalize; fault/cancel/mutation is isolated.", fixture: "returned event matrix",
			stimulus: "dispatch", expected: "matching finalize>selection or controlled error", exceptionOrEvent: "error.execution or cancellation", forbidden: "selection before finalize",
			partitions: "positive,error,cancellation", dimensions: "finalize"),
		C(
			id: "SCXML-INVOKE-007-CASE-001", requirement: "SCXML-INVOKE-007",
			description: "Autoforward true forwards each external event once to applicable invokes; false/default/internal/exit/loop paths do not overforward.", fixture: "autoforward topology matrix",
			stimulus: "dispatch external/internal", expected: "one ordered child delivery per active applicable invoke", exceptionOrEvent: "none", forbidden: "forwarded internal or duplicate event",
			partitions: "positive,concurrency", dimensions: "autoforward"),
		C(
			id: "SCXML-INVOKE-008-CASE-001", requirement: "SCXML-INVOKE-008",
			description: "Child inheritance is limited to documented location,args,security,parent,ID,type,I/O; child state/data/events do not leak to siblings/parent.",
			fixture: "parent/two children distinct context", stimulus: "invoke and mutate", expected: "only defined returned event/result crosses boundary", exceptionOrEvent: "none",
			forbidden: "sibling or parent data/configuration leak", partitions: "security,reliability", dimensions: "child isolation"),
		C(
			id: "SCXML-ERROR-001-CASE-001", requirement: "SCXML-ERROR-001",
			description: "Every parse-through-cleanup phase fault maps to exact error event/result table with no unreported background fault.", fixture: "fault injection phase matrix",
			stimulus: "fault each phase", expected: "case-table phase error event/result", exceptionOrEvent: "error.execution or error.communication", forbidden: "silent fault", partitions: "error",
			dimensions: "phase errors"),
		C(
			id: "SCXML-ERROR-002-CASE-001", requirement: "SCXML-ERROR-002",
			description: "Every UnhandledErrorBehaviour value produces its exact lifecycle, exit,result,queue,membership and disposal outcome.", fixture: "all current/future enum values",
			stimulus: "inject runtime error", expected: "policy-specific normalized terminal observation", exceptionOrEvent: "error event", forbidden: "fallback/default policy silently used",
			partitions: "error", dimensions: "unhandled policy"),
		C(
			id: "SCXML-ERROR-003-CASE-001", requirement: "SCXML-ERROR-003",
			description: "Nested/repeated/error-event-loop faults remain bounded, preserve policy and avoid recursive/background failures.", fixture: "error during error and repeated loop",
			stimulus: "inject sequence", expected: "bounded policy terminal result", exceptionOrEvent: "error event", forbidden: "StackOverflowException or unobserved task",
			partitions: "error,reliability", dimensions: "error recursion"),
		C(
			id: "SCXML-ERROR-004-CASE-001", requirement: "SCXML-ERROR-004",
			description: "Error data preserves stable exception fields/types/access across inner/aggregate/custom/lazy/persist routes and releases after processing.",
			fixture: "exception shape matrix", stimulus: "raise error then persist/release", expected: "contract fields preserved; resource collectible after event", exceptionOrEvent: "error event",
			forbidden: "unstable stack equality required or retained graph", partitions: "error,resource", dimensions: "error data"),
		C(
			id: "SCXML-ERROR-005-CASE-001", requirement: "SCXML-ERROR-005",
			description: "Livelock detector terminates true short/long eventless/internal loops but accepts high finite/progress/parallel/queue-changing runs.",
			fixture: "threshold neighborhood generators", stimulus: "run deterministic steps", expected: "true loops bounded termination; finite traces complete", exceptionOrEvent: "livelock error",
			forbidden: "false positive finite machine", partitions: "reliability,scalability", dimensions: "livelock"),
		C(
			id: "SCXML-ERROR-006-CASE-001", requirement: "SCXML-ERROR-006",
			description: "Destroy/terminate closes queues,stops schedulers/invokes,exits states,completes waiters,rejects later operations and retains primary failure with cleanup aggregate.",
			fixture: "active state with faulting cleanup services", stimulus: "terminate", expected: "ordered teardown, terminal rejection, primary+aggregate failure",
			exceptionOrEvent: "AggregateException", forbidden: "resource leak or lost primary failure", partitions: "error,cleanup", dimensions: "termination")
	];

	/*
	TEST-METADATA
	test_id: SCXML-PHASE2-REMAINING-CASE-TABLE
	requirement_ids:
	  - SCXML-LIFE-001..008
	  - SCXML-STATE-001..007
	  - SCXML-TRANS-001..014
	  - SCXML-HIST-001..005
	  - SCXML-EVENT-001..008
	  - SCXML-EXEC-001..008
	  - SCXML-DATA-001..006
	  - SCXML-SEND-001..006
	  - SCXML-CANCEL-001..002
	  - SCXML-INVOKE-001..008
	  - SCXML-ERROR-001..006
	title: Phase 2 SCXML interpreter contract matrix
	description: Each case supplies a minimal authority-derived fixture and exact observable contract; a conforming driver must reject every listed forbidden trace, state, event, exception, or resource effect.
	authority:
	  source: W3C SCXML 1.0 and exhaustive SCXML conformance matrix
	  section: Sections 3.13, 3.14, 3.15, 3.16, 3.17 and matrix sections 4-13
	  citation_or_rule: The case-specific rule and exact result are held in the complete generated-case record.
	phase: 2
	feature: interpreter-lifecycle-state-transition-event-executable-data-send-cancel-invoke-error
	target_components:
	  - StateMachineInterpreter
	  - EventQueue
	  - executable evaluators
	  - invocation and scheduler controllers
	test_kind: contract-matrix
	oracle_type: case-specific-ordered-trace-configuration-event-exception-resource-oracle
	risk: critical
	priority: critical
	construction_routes:
	  - scxml-text
	  - direct-public-model
	  - persisted-snapshot
	data_models:
	  - null
	  - runtime
	  - xpath
	target_frameworks:
	  - all-project-targets
	platforms:
	  - platform-independent
	partitions:
	  - positive
	  - negative
	  - boundary
	  - malformed
	  - error
	  - cancellation
	  - concurrency
	  - cleanup
	  - resource
	  - security
	  - reliability
	  - scalability
	dimensions:
	  matrix: requirement-specific generated cases
	  schedule: deterministic virtual clock and named await boundaries
	preconditions:
	  - PhaseTwoContractHarness supplies isolated deterministic dependencies from each case fixture.
	dependencies:
	  - PhaseTwoContractHarness (generated test-side helper)
	  - independent transition and configuration oracle
	  - virtual scheduler, queue, trace recorder and resource ledger
	arrange: Construct the case fixture and deterministic injected services described by the case record.
	stimulus: Execute the case record's explicit event stream, cancellation point, or concurrent schedule.
	expected:
	  - The actual normalized observation equals the case record's exact expected result.
	expected_exception_or_event: Case-specific exact event or exception in the record.
	forbidden:
	  - Every case record's forbidden outcome is absent.
	edge_cases:
	  - All planning-matrix partitions are enumerated in the case table.
	determinism:
	  clock: virtual
	  scheduling: named deterministic schedule
	  timeout_or_step_bound: case-specific, maximum 100000 operations
	isolation:
	  parallel_safe: true
	  shared_state: none
	cleanup:
	  - Case resource ledger reaches zero and no queue, timer, task, invoke, scope, or session remains.
	resource_risk: scheduler-invoke-queue-data-graph retention
	tier: fast
	tags:
	  - Exhaustive
	  - SCXML
	  - Phase2
	  - Generated
	related_tests: []
	known_issue: none
	compile_notes: PhaseTwoContractHarness, PhaseTwoObservation, and GeneratedPhaseTwoCase are intentionally test-side unresolved helpers; no semantic oracle is unresolved.
	generation_status: generated-uncompiled
	*/
	/*
	CASE-METADATA
	Every GeneratedPhaseTwoCase record contains case_id, requirement_ids, description, input_fixture,
	stimulus, expected, expected_exception_or_event, forbidden, partitions, dimensions, risk,
	target_frameworks_platforms, and compile_notes.  The compact literal table below is the complete
	per-case metadata representation permitted by runbook section “Parameterized and generated cases”.
	*/
	[DataTestMethod]
	[DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
	public async Task SCXML_Phase2_requirement_case_has_authority_derived_oracle(GeneratedPhaseTwoCase @case)
	{
		// Arrange
		await using var harness = await PhaseTwoContractHarness.CreateAsync(@case);

		// Act
		var observation = await harness.ExecuteAsync(@case.Stimulus, @case.OperationBound);

		// Assert
		Assert.AreEqual(@case.Expected, observation.NormalizedResult, @case.CaseId);
		Assert.AreEqual(@case.ExpectedExceptionOrEvent, observation.ExceptionOrEvent, @case.CaseId);
		CollectionAssert.DoesNotContain(observation.ForbiddenEffects.ToArray(), @case.Forbidden, @case.CaseId);
		Assert.AreEqual(expected: 0, observation.OutstandingResources, @case.CaseId);
	}

	public static IEnumerable<object[]> Cases() => CaseTable.Select(static c => new object[] { c });

	private static GeneratedPhaseTwoCase C(string id,
										   string requirement,
										   string description,
										   string fixture,
										   string stimulus,
										   string expected,
										   string exceptionOrEvent,
										   string forbidden,
										   string partitions,
										   string dimensions) =>
		new(
			id, requirement, description, fixture, stimulus, expected, exceptionOrEvent, forbidden, partitions, dimensions, Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/platform-independent", CompileNotes: "PhaseTwoContractHarness and its normalized observation adapters are generated test-side helpers.",
			OperationBound: 100000);

	public sealed record GeneratedPhaseTwoCase(
		string CaseId,
		string RequirementIds,
		string Description,
		string InputFixture,
		string Stimulus,
		string Expected,
		string ExpectedExceptionOrEvent,
		string Forbidden,
		string Partitions,
		string Dimensions,
		string Risk,
		string TargetFrameworksPlatforms,
		string CompileNotes,
		int OperationBound);
}
