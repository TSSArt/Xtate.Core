using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

/// <summary>
/// Authority-derived Phase 2 scenarios deliberately expressed through a test-side contract driver.
/// The driver is intentionally unresolved until the later harness campaign; the case table is the
/// executable-source specification and keeps every independently reportable requirement stable.
/// </summary>
[TestClass]
[TestCategory("Exhaustive.Generated")]
public sealed class Phase2RemainingRequirementsGeneratedTests
{
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
        Assert.AreEqual(0, observation.OutstandingResources, @case.CaseId);
    }

    public static IEnumerable<object[]> Cases() => CaseTable.Select(static c => new object[] { c });

    // case_id | requirement_ids | description | input/fixture | stimulus | expected | exception/event | forbidden | partitions | dimensions
    private static readonly GeneratedPhaseTwoCase[] CaseTable =
    [
        C("SCXML-LIFE-001-CASE-001","SCXML-LIFE-001","Normal completion publishes accepted, initialization, started, stable, completion and disposal once in order.","root final; trace","start","Accepted>Initialized>Started>Stable>Completed>Disposed","none","duplicate lifecycle notification","positive,cleanup","final completion"),
        C("SCXML-LIFE-002-CASE-001","SCXML-LIFE-002","Root selection uses initial attribute or first root child; no-root and illegal root initial are rejected.","no-root, first-child, initial attribute, invalid target/root initial element","start","valid: exact selected entry; invalid: validation failure","validation error","entry of unselected state","positive,negative,malformed","root selection"),
        C("SCXML-LIFE-003-CASE-001","SCXML-LIFE-003","Early data initializes document-order once; late data initializes first-entry only including reentry, history and parallel paths.","early/late nested data counters","enter, exit, reenter, history","early=[root,parent,child] once; late=one initialization per first entry","none","repeat initialization after late reentry","positive,boundary,error","binding"),
        C("SCXML-LIFE-004-CASE-001","SCXML-LIFE-004","Data initialization chooses expr/content/src/undefined/arguments without overwriting system variables and fails atomically on bad input.","five data sources, duplicate/unknown args, read-only system value","start","declared override values only; failed declaration has controlled startup error","error.execution","changed _event or partial forbidden write","positive,negative,error,security","startup arguments"),
        C("SCXML-LIFE-005-CASE-001","SCXML-LIFE-005","Root script runs after root data initialization and before root initial entry; failures and cancellation stop startup cleanly.","inline/external/empty/failing/async/cancel script","start at script await","data>global-script>initial-entry; failure emits error and disposes resource","error.execution","initial entry after script failure","positive,error,cancellation,resource","global script"),
        C("SCXML-LIFE-006-CASE-001","SCXML-LIFE-006","Stable callback occurs only after eventless closure, internal FIFO drain and deferred invokes are settled.","eventless chain plus raised event plus invoke","start","one Stable after all three completion observations","none","stable callback in intermediate configuration","positive,concurrency","macrostep closure"),
        C("SCXML-LIFE-007-CASE-001","SCXML-LIFE-007","Start/destroy/dispose/queue-close races have a bounded single winner and reject operations after terminal state.","barrier-controlled start/destroy/dispatch/dispose","release each race gate","one terminal result and all waiters complete within 100 operations","ObjectDisposedException or terminal rejection","two starts, leaked blocked waiter","negative,cancellation,concurrency,cleanup","lifecycle races"),
        C("SCXML-LIFE-008-CASE-001","SCXML-LIFE-008","Top-level final returns done data once, cancels activity and linearizes racing event acceptance at completion.","root final with delayed send/invoke and race event","complete/event race","one result and no further consumed event; activity cancelled","none","second done data or post-completion transition","positive,concurrency,cleanup","root completion"),
        C("SCXML-STATE-001-CASE-001","SCXML-STATE-001","Atomic, compound, parallel and final entry follows ancestor-before-descendant/document order with shared ancestors entered once.","nested compound, two-region parallel, shared-target ancestor","enter targets","exact entry trace parent>children document order; legal configuration","none","duplicate shared ancestor entry","positive","entry topology"),
        C("SCXML-STATE-002-CASE-001","SCXML-STATE-002","Exiting nested active configuration is deepest-first; onexit precedes transition content/removal and invoke cancellation follows contract.","parent/child invokes and trace actions","transition outward","child-onexit>parent-onexit>transition; each ancestor once","none","parent exit before child exit","positive,cleanup","exit ordering"),
        C("SCXML-STATE-003-CASE-001","SCXML-STATE-003","Compound initial content runs between parent entry and selected child entry for element, attribute, default and parallel multi-target forms.","four initial forms","enter compound","parent-entry>initial-content>child entries","none","child entry before initial content","positive","initial entry"),
        C("SCXML-STATE-004-CASE-001","SCXML-STATE-004","Parallel done event appears once only when all regions complete, including nested/re-exited/escaped activations.","staggered and nested parallel","complete regions and escape","done.state.parallel exactly once per activation after last region","done.state.parallel","done while a region nonfinal","positive,boundary","parallel completion"),
        C("SCXML-STATE-005-CASE-001","SCXML-STATE-005","Final entry actions precede correctly addressed done.state payload and parent completion processing.","root/compound/parallel/nested final with donedata","enter final","final-entry>done.state.parent(payload)>parent processing","done.state.parent","payload twice or done before final action","positive,error","final done data"),
        C("SCXML-STATE-006-CASE-001","SCXML-STATE-006","Generated microsteps maintain basic-state, compound-child, parallel-region and exited-state configuration invariants.","all nonisomorphic legal trees <= 6 nodes","each macrostep","independent configuration oracle equals runtime after every microstep","none","inactive/exited state in configuration","positive,reliability","generated configuration"),
        C("SCXML-STATE-007-CASE-001","SCXML-STATE-007","Depth 1-6, inline-capacity+1 and 100+ preserve order; configured over-limit fails controlledly rather than overflowing.","depth 1,2,3,4,5,6,capacity+1,101,limit+1","start","valid ordered entry; over-limit resource-policy failure","limit error","StackOverflowException or partial startup","boundary,resource,scalability","topology depth"),
        C("SCXML-TRANS-001-CASE-001","SCXML-TRANS-001","Eventless selection precedes queued events, handles false/error/new chains and bounds cycles across parallel regions.","true/false/error cond chains, external event, parallel","macrostep","eventless closure before event; condition error false plus platform event","error.execution","external event selected before enabled eventless","positive,error,reliability","eventless priority"),
        C("SCXML-TRANS-002-CASE-001","SCXML-TRANS-002","Descriptor matching preserves SCXML hierarchical boundaries for exact,prefix,wildcard,whitespace,empty,Unicode and case inputs.","descriptor matrix","match event name","case-table boolean match exactly","none","foo matches foobar","positive,negative,boundary","event descriptors"),
        C("SCXML-TRANS-003-CASE-001","SCXML-TRANS-003","First document-order matching true transition wins and later conditions are never evaluated; false/error permits specified successor.","ordered side-effecting conditions","dispatch event","selected first true; evaluation trace stops at selection","error.execution","later condition side effect after winner","positive,error","document order"),
        C("SCXML-TRANS-004-CASE-001","SCXML-TRANS-004","Descendant preempts conflicting ancestor while disjoint orthogonal transitions coexecute.","same/ancestor/sibling/cousin/parallel sources","dispatch matching event","reference selected transition set","none","conflicting ancestor action","positive","source priority"),
        C("SCXML-TRANS-005-CASE-001","SCXML-TRANS-005","Independent domain oracle computes exact exit sets for targetless,self,internal,external,ancestor,sibling,cross-region,multi-target,history and root.","transition domain matrix","select transition","runtime exit set and domain equal reference","none","state outside reference exit domain exited","positive,boundary","transition domain"),
        C("SCXML-TRANS-006-CASE-001","SCXML-TRANS-006","Internal/external/default semantics produce exact reentry and data/invoke reinitialization consequences; invalid type rejects.","compound-descendant,self,outside,type values","dispatch event","specified exited/reentered states and initialization counts","validation error","internal descendant reenters compound","positive,negative","transition type"),
        C("SCXML-TRANS-007-CASE-001","SCXML-TRANS-007","Targetless transitions execute content only, preserve configuration and correctly queue chained raises/failures.","eventless/eventful/condition/raise/failure","dispatch","unchanged configuration plus exact content trace","error.execution","entry or exit caused by targetless transition","positive,error","targetless"),
        C("SCXML-TRANS-008-CASE-001","SCXML-TRANS-008","Legal multi-target orthogonal descendants enter shared ancestors once/document order; illegal relation classes reject.","all target relationship classes","dispatch","legal configuration/reference order or validation failure","validation error","duplicate ancestor entry","positive,negative","multi-target"),
        C("SCXML-TRANS-009-CASE-001","SCXML-TRANS-009","Conflict resolver matches independent model for 2,3,many identical/intersecting/disjoint/parallel exit sets.","generated enabled transition sets","select","exact maximal nonconflicting set reference","none","lower-priority conflicting transition selected","positive,reliability","conflict resolution"),
        C("SCXML-TRANS-010-CASE-001","SCXML-TRANS-010","Every selected microstep orders all exits, transition content and entries as one total trace.","nested multi-transition trace actions","dispatch","all exit actions>all transition actions>all entry actions","none","entry before transition content","positive","microstep order"),
        C("SCXML-TRANS-011-CASE-001","SCXML-TRANS-011","Condition fault acts false, queues platform error and leaves no partial transition effects under each error policy.","throwing condition plus alternative","dispatch","alternative behavior plus error.execution and policy result","error.execution","partial exit from failed condition","error","condition fault"),
        C("SCXML-TRANS-012-CASE-001","SCXML-TRANS-012","Executable failure stops remaining sequence but retains completed side effects across exit/transition/entry/initial/finalize/foreach/if/donedata.","fault at each action position","trigger action","prefix effects retained, suffix absent, exact error event","error.execution","action after fault","error,cancellation","action fault"),
        C("SCXML-TRANS-013-CASE-001","SCXML-TRANS-013","Cancellation at each named await boundary linearizes to whole before/after configuration and completes teardown.","gate selection/exit/action/entry/invoke/queue","cancel gate","pre-state or post-state only; zero resources","OperationCanceledException","hybrid configuration","cancellation,cleanup","await boundaries"),
        C("SCXML-TRANS-014-CASE-001","SCXML-TRANS-014","Small legal generated machines and streams match independent reference configuration and trace every macrostep with shrinkable seed.","nonisomorphic trees <= practical bound; seeded streams","replay seed","exact reference trace/configuration per step","none","unshrunk mismatch or divergent trace","reliability,scalability","differential"),
        C("SCXML-HIST-001-CASE-001","SCXML-HIST-001","Shallow retains immediate children and deep retains atomic descendants across compound/parallel nested exits.","history topology matrix","exit and reenter","remembered shallow/deep configuration exact","none","deep restores only immediate child","positive","history capture"),
        C("SCXML-HIST-002-CASE-001","SCXML-HIST-002","Uninitialized history takes default content once; initialized history restores without default content.","history default counter","first then later entry","counter=1 and stored target restored","none","default content on stored history","positive","history default"),
        C("SCXML-HIST-003-CASE-001","SCXML-HIST-003","Shallow reentry applies child default; deep restores descendants directly with ancestor entry and data/invoke rules.","nested child defaults/data/invokes","reenter shallow/deep","exact entry trace and initialization counts","none","shallow directly restores grandchild","positive","history reentry"),
        C("SCXML-HIST-004-CASE-001","SCXML-HIST-004","Multiple shallow/deep histories remain independent through overwrite, never-active, nested and multi-target paths.","two histories and capture paths","capture/target histories","each history's own stored/default result","none","cross-history contamination","positive,negative","history isolation"),
        C("SCXML-HIST-005-CASE-001","SCXML-HIST-005","History IDs/targets survive serialization and resume; corrupt/missing persisted history is controlled failure.","serialized and corrupt snapshots","serialize,resume","same restored configuration or persistence/validation error","persistence error","illegal active configuration","positive,malformed,error","history persistence"),
        C("SCXML-EVENT-001-CASE-001","SCXML-EVENT-001","Event names retain default/segments/dots/whitespace/wildcard literals/long Unicode/case identity and reject invalid boundary input.","event-name lexical matrix","construct/match","exact segments/string/match or boundary error","ArgumentException","normalization or case folding","positive,negative,boundary","event name"),
        C("SCXML-EVENT-002-CASE-001","SCXML-EVENT-002","_event has correct field types and values before start, eventless, internal/external/platform/invoke, boundaries and resume.","events of each origin","inspect _event at hooks","specified fields or Undefined outside event processing","none","stale previous event fields","positive","_event context"),
        C("SCXML-EVENT-003-CASE-001","SCXML-EVENT-003","Internal FIFO has priority over next external; producers from exit/transition/entry/finalize/error join exact order.","multi-producer trace","enqueue internal and external","internal producer order before external","none","external consumed before queued internal","positive","internal queue"),
        C("SCXML-EVENT-004-CASE-001","SCXML-EVENT-004","External FIFO spans host/send/invoke/restored/HTTP/pipe and concurrent dispatch follows documented ticket linearization without torn payload.","source matrix and barrier dispatch","dispatch sequence","ticket order, one copy per payload","none","loss, duplicate, torn payload","concurrency,reliability","external queue"),
        C("SCXML-EVENT-005-CASE-001","SCXML-EVENT-005","Only matching active invoke event finalizes before selection; stale/nonmatching IDs do neither.","matching/stale invoke events","dispatch","matching finalize>selection; nonmatching ignored","none","finalize for stale ID","positive,negative","invoke event filtering"),
        C("SCXML-EVENT-006-CASE-001","SCXML-EVENT-006","Closing/cancelling/faulting queues in every lifecycle phase wakes waiters and releases payloads under configured termination.","internal/external queue phase matrix","close/cancel/fault","waiters complete and payload ledger zero","ChannelClosedException or cancellation","blocked waiter/resource retention","error,cancellation,cleanup","queue termination"),
        C("SCXML-EVENT-007-CASE-001","SCXML-EVENT-007","Dispatch snapshots payloads across source mutation, transition/finalize mutation and sessions.","mutable nested payload","mutate after each boundary","each recipient observes its specified snapshot","none","cross-event/session alias mutation","security,reliability","payload isolation"),
        C("SCXML-EVENT-008-CASE-001","SCXML-EVENT-008","Storms/self-send/recursive raise/eventless cycles hit bounded livelock policy while finite progress beyond threshold completes.","loop and long-progress generators","run bounded steps","loop controlled termination; progressing finite chain completes","livelock error","false positive on progressing chain","resource,reliability,scalability","event amplification"),
        C("SCXML-EXEC-001-CASE-001","SCXML-EXEC-001","Raise enqueues exactly one payloadless internal event in order; missing/invalid event rejects on every route.","raise route/error/finalize/exit matrix","execute raise","one named internal event or validation/evaluation error","validation error","external event or payload","positive,negative","raise"),
        C("SCXML-EXEC-002-CASE-001","SCXML-EXEC-002","If evaluates lazily/in order and executes exactly first true or else, including nested/empty/fault/cancel branches.","instrumented branch conditions","execute if","condition trace through winner only; selected actions only","error.execution or cancellation","evaluation/action in skipped branch","positive,error,cancellation","if"),
        C("SCXML-EXEC-003-CASE-001","SCXML-EXEC-003","Foreach snapshots or live-iterates according to contract once, orders items/index and restores scopes after success/error/cancel/nesting.","mutable collection/nested scopes","execute foreach","defined iteration trace and original scope restored","error.execution or cancellation","scope variable retained","positive,error,cancellation","foreach"),
        C("SCXML-EXEC-004-CASE-001","SCXML-EXEC-004","Log obeys enablement/evaluation contract, invariant formatting, zero mutation and controlled logger/evaluator fault without retention.","all data kinds/cultures/faulting logger","execute log","exact log/no-log and unchanged data","error.execution","culture-dependent value or retained graph","positive,error,resource","log"),
        C("SCXML-EXEC-005-CASE-001","SCXML-EXEC-005","Assign evaluates RHS/location in required order and atomically handles undefined/empty/multi-location operations.","instrumented expressions and locations","execute assign","order trace and all-or-no required mutation","error.execution","partial atomic mutation","positive,error","assign orchestration"),
        C("SCXML-EXEC-006-CASE-001","SCXML-EXEC-006","Scripts cover inline/external/global/local/no-op/async/error/cancel/unsupported/media/base/disposal/repetition.","script source matrix","execute script","specified effect once and external stream disposed","error.execution or cancellation","reused stale script result/stream leak","positive,error,cancellation,resource","script"),
        C("SCXML-EXEC-007-CASE-001","SCXML-EXEC-007","Custom actions resolve namespace/name, receive content/context, preserve order and report missing/multiple/fault/cancel/dispose providers.","provider matrix","execute custom action","exact provider invocation or controlled error","error.execution","unknown content silently ignored","positive,error,cancellation","custom action"),
        C("SCXML-EXEC-008-CASE-001","SCXML-EXEC-008","Blocks 0,1,2,255,256,1k,100k preserve order, cancellation and linear resources without recursion overflow.","action count matrix","execute/cancel midpoint","ordered prefix/full result and bounded resource ledger","OperationCanceledException","StackOverflowException or reordered actions","boundary,cancellation,resource,scalability","action blocks"),
        C("SCXML-DATA-001-CASE-001","SCXML-DATA-001","Data declarations support no source/expr/content/src across binding,args,reentry,media,empty,malformed and failures.","data source cross-product","initialize/reenter","case-specified value/count or controlled error","error.execution","wrong source precedence or partial init","positive,error","data declarations"),
        C("SCXML-DATA-002-CASE-001","SCXML-DATA-002","Params evaluate expr/location in order across null/scalar/list/XML/repeats/empty names and snapshot aliases.","parameter value matrix","construct payload","ordered named payload or controlled error","error.execution","alias mutation or later evaluation after failure","positive,negative,error","params"),
        C("SCXML-DATA-003-CASE-001","SCXML-DATA-003","Namelist evaluates locations once lexical order, uses intended keys and enforces duplicate/combination rules.","instrumented namelist/params/content","construct payload","one evaluation per lexical location and legal payload","validation error","duplicate key overwrite","positive,negative","namelist"),
        C("SCXML-DATA-004-CASE-001","SCXML-DATA-004","Content expr/body preserves whitespace/text/XML/mixed namespaces/special/large data and handles malformed/cache failures.","content lexical/source matrix","evaluate repeatedly","exact typed content and defined cache retry behavior","error.execution","escaped/lost namespace or stale failed cache","positive,malformed,resource","content"),
        C("SCXML-DATA-005-CASE-001","SCXML-DATA-005","Done data none/params/content/all kinds appears exactly once only when final entered; errors/cancel do not duplicate.","final/nonfinal donedata matrix","enter or bypass final","one addressed payload or no evaluation","error.execution or cancellation","done payload without final entry","positive,error,cancellation","donedata"),
        C("SCXML-DATA-006-CASE-001","SCXML-DATA-006","Writable/read-only/constant access and metadata survive conversion,event,invoke,persist/resume; forbidden writes are atomic.","access flag route matrix","attempt write/roundtrip","metadata preserved; forbidden write fails unchanged","InvalidOperationException","partial forbidden mutation","security,error","data access"),
        C("SCXML-SEND-001-CASE-001","SCXML-SEND-001","Send resolves literal/expressions exactly once/order and never calls router after required field failure/cancel.","field expression error matrix","execute send","ordered successful route or error before router","error.execution or cancellation","router call after early failure","positive,error,cancellation","send resolution"),
        C("SCXML-SEND-002-CASE-001","SCXML-SEND-002","Send IDs generate/propagate uniquely under concurrency/resume and idlocation failure has specified compensating schedule outcome.","explicit/empty/duplicate/generated IDs/idlocation fault","send concurrently","unique IDs and contract-specified scheduled count","error.execution","duplicate generated ID or orphan schedule","concurrency,error","send ids"),
        C("SCXML-SEND-003-CASE-001","SCXML-SEND-003","Send payload selection validates namelist/params/content conflicts, preserves raw strings/types and isolates data.","payload form matrix","send/mutate source","exact payload or validation error","validation error","payload alias or coerced raw string","positive,negative,security","send payload"),
        C("SCXML-SEND-004-CASE-001","SCXML-SEND-004","Delayed send boundaries/cancel-fire races dispatch exactly permitted count and never affect distinct IDs.","virtual due-time/cancel matrix","advance clock and race gates","0 or 1 exact dispatch as policy permits","none","duplicate/early/late dispatch","boundary,concurrency,cleanup","delayed send"),
        C("SCXML-SEND-005-CASE-001","SCXML-SEND-005","Routing resolves default/canonical/alias/custom/internal/session/parent/invoke targets and rejects malformed/unavailable routes with exact metadata.","target/type route matrix","send","exact origin/origintype/sender/target or communication error","error.communication","wrong target delivery","positive,negative","routing"),
        C("SCXML-SEND-006-CASE-001","SCXML-SEND-006","Router/scheduler sync/async fault,timeout,cancel,partial dispatch emits communication policy result and cleans tasks/schedules.","faulting router/scheduler matrix","send","error.communication plus zero forgotten task/schedule","error.communication","silent background failure","error,cancellation,cleanup","send failure"),
        C("SCXML-CANCEL-001-CASE-001","SCXML-CANCEL-001","Cancel resolves literal/expression IDs, rejects absent/wrong/empty, cancels matching one/many and preserves distinct ID events.","ID/cancellation/concurrency matrix","cancel","only matching scheduled events removed","validation error","distinct ID cancellation","positive,negative,concurrency","cancel"),
        C("SCXML-CANCEL-002-CASE-001","SCXML-CANCEL-002","Scheduler disposal for 0/1/many, cancellation faults, dispatch and simultaneous schedule/cancel aggregates errors and leaves zero timers/tasks.","scheduler teardown matrix","dispose sync/async","consistent aggregate/primary error and zero ledger","AggregateException","timer/task leak","error,concurrency,cleanup","scheduler disposal"),
        C("SCXML-INVOKE-001-CASE-001","SCXML-INVOKE-001","Invoke resolves type/src/content/id/params/namelist in order, rejects conflicts and handles source fault/cancel.","invoke source resolution matrix","enter invoke state","ordered evaluation and exactly one provider start or controlled error","error.execution or cancellation","provider start after resolution failure","positive,error,cancellation","invoke resolution"),
        C("SCXML-INVOKE-002-CASE-001","SCXML-INVOKE-002","Invoke IDs are unique; idlocation/ambient context is scoped to start/cancel/finalize and cleared across nested/concurrent terminal paths.","nested concurrent invokes","start/cancel/finalize","unique IDs and no ambient value after terminal path","none","ambient invoke ID leak","concurrency,cleanup","invoke ids"),
        C("SCXML-INVOKE-003-CASE-001","SCXML-INVOKE-003","Invokes begin only at stable point and never start if eventless/internal/external exit happens first.","three immediate-exit paths","enter then exit","zero starts before exit; otherwise start after stable trace","none","invoke start before stable","positive","deferred invoke start"),
        C("SCXML-INVOKE-004-CASE-001","SCXML-INVOKE-004","One/many/nested/parallel/source/content/service invokes maintain active registry and dispose exactly once under all terminal paths.","invoke topology/provider fault matrix","start,complete,cancel,destroy","registry accurate and each invoke disposal count=1","error.execution","duplicate dispose or orphan registry","positive,error,cleanup","invoke lifecycle"),
        C("SCXML-INVOKE-005-CASE-001","SCXML-INVOKE-005","State exit cancels active invokes in contract order across success/fault/hang/race/duplicate exit and teardown escalation.","blocking invokes/onexit trace","exit/race completion","specified onexit-cancel order, bounded completion and one cancellation","error.execution","hung invoke blocks teardown forever","error,concurrency,cleanup","invoke cancellation"),
        C("SCXML-INVOKE-006-CASE-001","SCXML-INVOKE-006","Matching returned done/error/arbitrary events finalize before selection; stale IDs never finalize; fault/cancel/mutation is isolated.","returned event matrix","dispatch","matching finalize>selection or controlled error","error.execution or cancellation","selection before finalize","positive,error,cancellation","finalize"),
        C("SCXML-INVOKE-007-CASE-001","SCXML-INVOKE-007","Autoforward true forwards each external event once to applicable invokes; false/default/internal/exit/loop paths do not overforward.","autoforward topology matrix","dispatch external/internal","one ordered child delivery per active applicable invoke","none","forwarded internal or duplicate event","positive,concurrency","autoforward"),
        C("SCXML-INVOKE-008-CASE-001","SCXML-INVOKE-008","Child inheritance is limited to documented location,args,security,parent,ID,type,I/O; child state/data/events do not leak to siblings/parent.","parent/two children distinct context","invoke and mutate","only defined returned event/result crosses boundary","none","sibling or parent data/configuration leak","security,reliability","child isolation"),
        C("SCXML-ERROR-001-CASE-001","SCXML-ERROR-001","Every parse-through-cleanup phase fault maps to exact error event/result table with no unreported background fault.","fault injection phase matrix","fault each phase","case-table phase error event/result","error.execution or error.communication","silent fault","error","phase errors"),
        C("SCXML-ERROR-002-CASE-001","SCXML-ERROR-002","Every UnhandledErrorBehaviour value produces its exact lifecycle, exit,result,queue,membership and disposal outcome.","all current/future enum values","inject runtime error","policy-specific normalized terminal observation","error event","fallback/default policy silently used","error","unhandled policy"),
        C("SCXML-ERROR-003-CASE-001","SCXML-ERROR-003","Nested/repeated/error-event-loop faults remain bounded, preserve policy and avoid recursive/background failures.","error during error and repeated loop","inject sequence","bounded policy terminal result","error event","StackOverflowException or unobserved task","error,reliability","error recursion"),
        C("SCXML-ERROR-004-CASE-001","SCXML-ERROR-004","Error data preserves stable exception fields/types/access across inner/aggregate/custom/lazy/persist routes and releases after processing.","exception shape matrix","raise error then persist/release","contract fields preserved; resource collectible after event","error event","unstable stack equality required or retained graph","error,resource","error data"),
        C("SCXML-ERROR-005-CASE-001","SCXML-ERROR-005","Livelock detector terminates true short/long eventless/internal loops but accepts high finite/progress/parallel/queue-changing runs.","threshold neighborhood generators","run deterministic steps","true loops bounded termination; finite traces complete","livelock error","false positive finite machine","reliability,scalability","livelock"),
        C("SCXML-ERROR-006-CASE-001","SCXML-ERROR-006","Destroy/terminate closes queues,stops schedulers/invokes,exits states,completes waiters,rejects later operations and retains primary failure with cleanup aggregate.","active state with faulting cleanup services","terminate","ordered teardown, terminal rejection, primary+aggregate failure","AggregateException","resource leak or lost primary failure","error,cleanup","termination")
    ];

    private static GeneratedPhaseTwoCase C(string id, string requirement, string description, string fixture, string stimulus, string expected, string exceptionOrEvent, string forbidden, string partitions, string dimensions) =>
        new(id, requirement, description, fixture, stimulus, expected, exceptionOrEvent, forbidden, partitions, dimensions, "critical", "all-project-targets/platform-independent", "PhaseTwoContractHarness and its normalized observation adapters are generated test-side helpers.", 100000);

    public sealed record GeneratedPhaseTwoCase(string CaseId, string RequirementIds, string Description, string InputFixture, string Stimulus, string Expected, string ExpectedExceptionOrEvent, string Forbidden, string Partitions, string Dimensions, string Risk, string TargetFrameworksPlatforms, string CompileNotes, int OperationBound);
}
