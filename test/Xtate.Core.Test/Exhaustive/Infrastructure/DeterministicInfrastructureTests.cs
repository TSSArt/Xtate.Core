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

namespace Xtate.Core.Test.Exhaustive.Infrastructure;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class DeterministicInfrastructureTests
{
	/* TEST-METADATA
	test_id: INFRA-SCHED-002
	requirement_ids: [INFRA-SCHED-001]
	title: Disposing the last virtual-scheduler lease prevents callback execution
	description: A single callback is scheduled then its lease is disposed before its due time. Correct behavior leaves the callback trace empty and the pending count zero; incorrect behavior runs cancelled work or retains it.
	authority: { source: exhaustive plan document 01, section: deterministic infrastructure, citation_or_rule: cancellation removes scheduled work before dispatch. }
	phase: 1
	feature: deterministic-scheduling
	target_components: [VirtualScheduler]
	test_kind: component-unit
	oracle_type: callback-absence-and-pending-count
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [cancellation, single-item, cleanup]
	dimensions: { callback_count: 1, cancellation_point: before-due-time }
	preconditions: [new virtual scheduler]
	dependencies: [VirtualScheduler]
	arrange: Schedule one callback at virtual time 1 and dispose its lease.
	stimulus: Advance virtual time to 1.
	expected: [trace is empty, pending count is 0]
	expected_exception_or_event: none
	forbidden: [callback execution, retained scheduled item]
	edge_cases: [last pending item cancellation]
	determinism: { clock: virtual, scheduling: deterministic, timeout_or_step_bound: 'one callback' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [scheduler disposal leaves no pending callback]
	resource_risk: timer-retention
	tier: fast
	tags: [Exhaustive, Infrastructure]
	related_tests: [INFRA-SCHED-001]
	known_issue: none
	compile_notes: none
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	public void INFRA_SCHED_002_Disposed_last_lease_does_not_run()
	{
		using var scheduler = new VirtualScheduler();
		var trace = new List<string>();

		using (scheduler.Schedule(delayMilliseconds: 1, () => trace.Add("cancelled"))) { }

		scheduler.AdvanceTo(1);
		Assert.AreEqual(expected: 0, trace.Count);
		Assert.AreEqual(expected: 0, scheduler.PendingCount);
	}

	/* TEST-METADATA
	test_id: INFRA-SCHED-003
	requirement_ids: [INFRA-SCHED-001]
	title: Zero-delay virtual work runs only when the scheduler is advanced
	description: A due-time-zero callback is queued without synchronous execution and runs exactly once when virtual time advances to zero; an eager scheduler would alter the trace before the explicit advance.
	authority: { source: exhaustive plan document 01, section: deterministic infrastructure, citation_or_rule: virtual scheduling is driven by explicit clock advancement. }
	phase: 1
	feature: deterministic-scheduling
	target_components: [VirtualScheduler]
	test_kind: component-unit
	oracle_type: ordered-trace-and-pending-count
	risk: medium
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [boundary,zero-delay,explicit-clock,cleanup]
	dimensions: { due_time: 0, advances: 1 }
	preconditions: [new virtual scheduler]
	dependencies: [VirtualScheduler]
	arrange: Schedule one callback at due time zero.
	stimulus: Observe before advancing, then advance to zero.
	expected: [trace is empty before advance, trace is 'zero' after advance, pending count is 0]
	expected_exception_or_event: none
	forbidden: [synchronous callback execution during Schedule, duplicate callback, retained work]
	edge_cases: [zero is both current time and a legal due time]
	determinism: { clock: virtual, scheduling: explicit-clock-advance, timeout_or_step_bound: 'one callback' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [scheduler disposal leaves no pending callback]
	resource_risk: timer-retention
	tier: fast
	tags: [Exhaustive, Infrastructure]
	related_tests: [INFRA-SCHED-001,INFRA-SCHED-002]
	known_issue: none
	compile_notes: none
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	public void INFRA_SCHED_003_Zero_due_work_waits_for_explicit_advance()
	{
		using var scheduler = new VirtualScheduler();
		var trace = new List<string>();
		scheduler.Schedule(delayMilliseconds: 0, () => trace.Add("zero"));
		Assert.AreEqual(expected: 0, trace.Count);
		scheduler.AdvanceTo(0);
		CollectionAssert.AreEqual(new[] { "zero" }, trace);
		Assert.AreEqual(expected: 0, scheduler.PendingCount);
	}

	/* TEST-METADATA
	test_id: INFRA-SCHED-004
	requirement_ids: [INFRA-SCHED-001]
	title: Advancing virtual time before due time preserves pending work
	description: Work due at virtual time two remains pending after advancing only to one; premature execution is observable as a nonempty trace.
	authority: { source: exhaustive plan document 01, section: deterministic infrastructure, citation_or_rule: scheduled work runs no earlier than its due time. }
	phase: 1
	feature: deterministic-scheduling
	target_components: [VirtualScheduler]
	test_kind: component-unit
	oracle_type: trace-and-pending-count
	risk: medium
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [boundary,before-due-time]
	dimensions: { due_time: 2, advance_time: 1 }
	preconditions: [new virtual scheduler]
	dependencies: [VirtualScheduler]
	arrange: Schedule one callback at time two.
	stimulus: Advance virtual time to one.
	expected: [empty trace,pending count 1]
	expected_exception_or_event: none
	forbidden: [premature callback execution,removed pending work]
	edge_cases: [advance immediately below due time]
	determinism: { clock: virtual, scheduling: deterministic, timeout_or_step_bound: 'one callback' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [scheduler disposal clears pending callback]
	resource_risk: timer-retention
	tier: fast
	tags: [Exhaustive,Infrastructure]
	related_tests: [INFRA-SCHED-001,INFRA-SCHED-003]
	known_issue: none
	compile_notes: none
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	public void INFRA_SCHED_004_Advance_before_due_time_preserves_pending_work()
	{
		using var scheduler = new VirtualScheduler();
		var trace = new List<string>();
		scheduler.Schedule(delayMilliseconds: 2, () => trace.Add("due"));
		scheduler.AdvanceTo(1);
		Assert.AreEqual(expected: 0, trace.Count);
		Assert.AreEqual(expected: 1, scheduler.PendingCount);
	}

	/*
	TEST-METADATA
	test_id: INFRA-SCHED-001
	requirement_ids: [INFRA-SCHED-001]
	title: Virtual scheduler orders equal due times by insertion order
	description: Schedules work at due times 10, 5, 5, and 5 then cancels one of the equal-time items; correct behavior emits first and second at time 5, third at time 10, and leaves no pending work, whereas a wrong scheduler reorders, runs cancelled work, or retains a timer.
	authority: { source: exhaustive plan document 01, section: deterministic infrastructure, citation_or_rule: due time then insertion order; cancellation removes scheduled work }
	phase: 1
	feature: deterministic-scheduling
	target_components: [VirtualScheduler]
	test_kind: component-unit
	oracle_type: ordered-trace-and-pending-count
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, equal-due-time, cancellation, cleanup]
	dimensions: { due_times: '5,5,5,10', cancelled_item: 'second equal-time item' }
	preconditions: [new virtual scheduler]
	dependencies: [VirtualScheduler]
	arrange: Schedule three equal/one later callbacks and dispose the middle equal-time lease.
	stimulus: Advance virtual time to 5 than by 5.
	expected: [trace is 'first,second,third', pending count is 0]
	expected_exception_or_event: none
	forbidden: [cancelled callback execution, timestamp reordering, retained scheduled callback]
	edge_cases: [equal timestamps]
	determinism: { clock: virtual, scheduling: due-time-then-insertion-order, timeout_or_step_bound: '4 callbacks' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [scheduler disposal releases all callback leases]
	resource_risk: timer-retention
	tier: fast
	tags: [Exhaustive, Infrastructure]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_SCHED_001_VirtualScheduler_orders_due_work_then_insertion_order_and_releases_all_work()
	{
		using var scheduler = new VirtualScheduler();
		var trace = new List<string>();

		scheduler.Schedule(delayMilliseconds: 10, () => trace.Add("third"));
		scheduler.Schedule(delayMilliseconds: 5, () => trace.Add("first"));
		var cancelled = scheduler.Schedule(delayMilliseconds: 5, () => trace.Add("cancelled"));
		scheduler.Schedule(delayMilliseconds: 5, () => trace.Add("second"));
		cancelled.Dispose();

		scheduler.AdvanceTo(5);
		CollectionAssert.AreEqual(new[] { "first", "second" }, trace);
		Assert.AreEqual(expected: 1, scheduler.PendingCount);

		scheduler.AdvanceBy(5);
		CollectionAssert.AreEqual(new[] { "first", "second", "third" }, trace);
		Assert.AreEqual(expected: 0, scheduler.PendingCount);
	}

	/*
	TEST-METADATA
	test_id: INFRA-RES-001
	requirement_ids: [INFRA-RES-001]
	title: Resource ledger reaches an empty state after nested leases close
	description: Tracks a reader and stream in nested scopes; correct behavior reports an empty ledger after both disposals, while an incorrect lease tracker retains either name or decrements the count incorrectly.
	authority: { source: exhaustive plan document 04, section: resource pass criteria, citation_or_rule: all tracked resources must be released at teardown }
	phase: 1
	feature: resource-ledger
	target_components: [ResourceLedger]
	test_kind: component-unit
	oracle_type: resource-ledger-empty
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, nested-disposal, cleanup]
	dimensions: { resources: 'reader,stream', close_order: 'LIFO' }
	preconditions: [new empty ledger]
	dependencies: [ResourceLedger]
	arrange: Acquire reader and stream leases in nested using scopes.
	stimulus: Exit both scopes then query the ledger.
	expected: [ledger has zero live entries]
	expected_exception_or_event: none
	forbidden: [retained reader lease, retained stream lease, negative resource count]
	edge_cases: [nested ownership]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: '2 disposals' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [ledger disposal leaves no registrations]
	resource_risk: resource-retention
	tier: fast
	tags: [Exhaustive, Infrastructure, Leak]
	related_tests: [INFRA-RES-003]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_RES_001_ResourceLedger_detects_and_proves_resource_cleanup()
	{
		using var ledger = new ResourceLedger();

		using (ledger.Track("reader"))
		using (ledger.Track("stream")) { }

		ledger.AssertEmpty();
	}

	/*
	TEST-METADATA
	test_id: INFRA-RES-002
	requirement_ids: [INFRA-RES-002]
	title: Weak-reference probe releases an unowned object after bounded collections
	description: Creates an object reachable only through a weak reference and performs the probe's bounded collection cycle; correct behavior makes the reference dead, whereas retained helper state leaves it alive.
	authority: { source: exhaustive plan document 04, section: memory-leak measurement protocol, citation_or_rule: completed test resources must not remain strongly reachable }
	phase: 1
	feature: leak-probe
	target_components: [WeakReferenceProbe]
	test_kind: component-unit
	oracle_type: weak-reference-collection
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [runtime-dependent]
	partitions: [unowned-object, bounded-gc, cleanup]
	dimensions: { ownership: none, collection_passes: bounded-full-collections }
	preconditions: [no strong reference remains after probe construction]
	dependencies: [WeakReferenceProbe]
	arrange: Request a weak reference from the probe.
	stimulus: Let the probe complete its bounded collection protocol.
	expected: [weak reference IsAlive is false]
	expected_exception_or_event: none
	forbidden: [test helper retaining the observed object, unbounded collection loop]
	edge_cases: [GC timing]
	determinism: { clock: not-applicable, scheduling: bounded-full-collection-protocol, timeout_or_step_bound: 'probe-defined finite passes' }
	isolation: { parallel_safe: false, shared_state: process-gc }
	cleanup: [probe releases all temporary strong references]
	resource_risk: managed-memory-retention
	tier: fast
	tags: [Exhaustive, Infrastructure, Leak]
	related_tests: []
	known_issue: none
	compile_notes: GC collection timing remains runtime-dependent; the helper provides the bounded protocol.
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_RES_002_Weak_reference_probe_proves_unowned_objects_are_collectible()
	{
		var reference = WeakReferenceProbe.ObserveCollectedObject();

		Assert.IsFalse(reference.IsAlive, message: "The probe object remained reachable after bounded full collections.");
	}

	/*
	TEST-METADATA
	test_id: INFRA-WATCH-001
	requirement_ids: [INFRA-WATCH-001]
	title: Operation watchdog rejects the first operation above its fixed budget
	description: A watchdog configured for two permitted operations receives three ticks; correct behavior throws TimeoutException on the third tick and records all three attempts, whereas a wrong implementation permits unbounded work or reports an inaccurate count.
	authority: { source: exhaustive plan document 04, section: universal bounded-execution contract, citation_or_rule: generated work has a deterministic finite operation limit }
	phase: 1
	feature: bounded-execution
	target_components: [OperationWatchdog]
	test_kind: component-unit
	oracle_type: exact-exception-and-operation-count
	risk: critical
	priority: critical
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [at-limit, above-limit, failure]
	dimensions: { budget: 2, attempted_operations: 3 }
	preconditions: [new watchdog with finite budget two]
	dependencies: [OperationWatchdog]
	arrange: Create the watchdog and tick it twice.
	stimulus: Tick it once more.
	expected: [third tick throws TimeoutException, Operations equals 3]
	expected_exception_or_event: TimeoutException
	forbidden: [silent third tick, operation count less than 3, unbounded execution]
	edge_cases: [exact boundary]
	determinism: { clock: virtual-or-not-applicable, scheduling: synchronous, timeout_or_step_bound: '3 ticks' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [no background work created]
	resource_risk: runaway-operation
	tier: fast
	tags: [Exhaustive, Infrastructure, Reliability]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_WATCH_001_Operation_watchdog_is_bounded_and_reports_the_count()
	{
		var watchdog = new OperationWatchdog(2);
		watchdog.Tick();
		watchdog.Tick();

		try
		{
			watchdog.Tick();
			Assert.Fail("The operation budget must terminate deterministic work.");
		}
		catch (TimeoutException) { }

		Assert.AreEqual(expected: 3, watchdog.Operations);
	}

	/*
	TEST-METADATA
	test_id: INFRA-TRACE-001
	requirement_ids: [INFRA-TRACE-001]
	title: Ordered trace preserves sequence numbers in its snapshot
	description: Records external queue, state-entry, and internal queue observations in order; correct behavior returns the three numbered observations unchanged, whereas a wrong trace reorders records, duplicates a sequence, or exposes mutable storage.
	authority: { source: exhaustive plan README, section: planned deterministic test vocabulary, citation_or_rule: trace callbacks are sequence-numbered in occurrence order }
	phase: 1
	feature: structured-tracing
	target_components: [OrderedTrace]
	test_kind: component-unit
	oracle_type: exact-sequenced-trace
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, cross-category-ordering, snapshot]
	dimensions: { categories: 'queue,state,queue', queue_kinds: 'external,internal' }
	preconditions: [empty trace]
	dependencies: [OrderedTrace]
	arrange: Record external queue, state entry, then internal queue facts.
	stimulus: Obtain the trace snapshot.
	expected: [snapshot equals '0:queue:external,1:state:entered,2:queue:internal', entry count is 3]
	expected_exception_or_event: none
	forbidden: [reordered entry, duplicate sequence number, missing entry]
	edge_cases: [interleaved categories]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: '3 records' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [trace is eligible for collection after test]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, Infrastructure, Trace]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_TRACE_001_Ordered_trace_assigns_stable_sequences_and_snapshots_without_reordering()
	{
		var trace = new OrderedTrace();
		trace.Record(category: "queue", value: "external");
		trace.Record(category: "state", value: "entered");
		trace.Record(category: "queue", value: "internal");

		CollectionAssert.AreEqual(new[] { "0:queue:external", "1:state:entered", "2:queue:internal" }, trace.Snapshot());
		Assert.AreEqual(expected: 3, trace.Entries.Count);
		Assert.AreEqual(expected: 2, trace.Entries[2].Sequence);
	}

	/*
	TEST-METADATA
	test_id: INFRA-RES-003
	requirement_ids: [INFRA-RES-003]
	title: Resource lease disposal is idempotent
	description: Disposes one cancellation-source lease twice; correct behavior removes the single ledger registration once and reports empty, whereas a wrong implementation throws on the second disposal or corrupts its resource count.
	authority: { source: exhaustive plan document 04, section: resource pass criteria, citation_or_rule: cleanup is safe and leaves zero live tracked resources }
	phase: 1
	feature: resource-ledger
	target_components: [ResourceLedger]
	test_kind: component-unit
	oracle_type: idempotent-disposal-and-empty-ledger
	risk: medium
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [repeated-disposal, cleanup]
	dimensions: { lease_kind: cancellation-source, dispose_calls: 2 }
	preconditions: [ledger contains one lease]
	dependencies: [ResourceLedger]
	arrange: Track one cancellation-source lease.
	stimulus: Dispose the same lease twice.
	expected: [no disposal exception, ledger is empty]
	expected_exception_or_event: none
	forbidden: [double decrement, retained lease, second-disposal exception]
	edge_cases: [idempotent cleanup]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: '2 disposal calls' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [owning ledger is disposed]
	resource_risk: resource-ledger-corruption
	tier: fast
	tags: [Exhaustive, Infrastructure, Leak]
	related_tests: [INFRA-RES-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_RES_003_Double_disposal_of_a_resource_lease_is_idempotent()
	{
		using var ledger = new ResourceLedger();
		var lease = ledger.Track("cancellation-source");

		lease.Dispose();
		lease.Dispose();

		ledger.AssertEmpty();
	}

	/*
	TEST-METADATA
	test_id: INFRA-SCHED-002
	requirement_ids: [INFRA-SCHED-002]
	title: Scheduler rejects negative virtual time and scheduling after disposal
	description: Requests negative delay, negative advance, and a zero-delay callback after disposal; correct behavior throws ArgumentOutOfRangeException for each negative value and ObjectDisposedException after disposal, whereas a wrong scheduler moves backward in time or accepts work it cannot own.
	authority: { source: exhaustive plan document 04, section: universal bounded-execution contract, citation_or_rule: invalid bounds fail explicitly and disposed resources reject new work }
	phase: 1
	feature: deterministic-scheduling
	target_components: [VirtualScheduler]
	test_kind: component-unit
	oracle_type: exact-exception-family
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative-input, disposed, boundary]
	dimensions: { delays: '-1,0', lifecycle: active-and-disposed }
	preconditions: [new scheduler]
	dependencies: [VirtualScheduler]
	arrange: Create a scheduler then dispose it after invalid active-state calls.
	stimulus: Schedule and advance with -1, then schedule with 0 after disposal.
	expected: [two ArgumentOutOfRangeException failures, one ObjectDisposedException failure]
	expected_exception_or_event: ArgumentOutOfRangeException and ObjectDisposedException
	forbidden: [negative virtual clock movement, post-disposal callback registration]
	edge_cases: [zero delay is only rejected because scheduler is disposed]
	determinism: { clock: virtual, scheduling: synchronous, timeout_or_step_bound: '3 rejected calls' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [disposed scheduler has no pending callbacks]
	resource_risk: callback-retention
	tier: fast
	tags: [Exhaustive, Infrastructure]
	related_tests: [INFRA-SCHED-003]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_SCHED_002_Scheduler_rejects_invalid_time_and_post_disposal_work()
	{
		var scheduler = new VirtualScheduler();

		try
		{
			scheduler.Schedule(delayMilliseconds: -1, static () => { });
			Assert.Fail("Negative delays must be rejected.");
		}
		catch (ArgumentOutOfRangeException) { }

		try
		{
			scheduler.AdvanceBy(-1);
			Assert.Fail("Negative advancement must be rejected.");
		}
		catch (ArgumentOutOfRangeException) { }

		scheduler.Dispose();

		try
		{
			scheduler.Schedule(delayMilliseconds: 0, static () => { });
			Assert.Fail("Disposed schedulers must reject new work.");
		}
		catch (ObjectDisposedException) { }
	}

	/*
	TEST-METADATA
	test_id: INFRA-SCHED-003
	requirement_ids: [INFRA-SCHED-003]
	title: Scheduler rejects a due-time addition that overflows virtual time
	description: Advances a scheduler to Int64.MaxValue and then schedules a one-tick delay; correct behavior throws OverflowException rather than wrapping to an earlier due time, while already scheduled work remains owned by the scheduler.
	authority: { source: exhaustive plan document 04, section: universal bounded-execution contract, citation_or_rule: numeric boundary input must fail explicitly without corrupting queued work }
	phase: 1
	feature: deterministic-scheduling
	target_components: [VirtualScheduler]
	test_kind: component-unit
	oracle_type: exact-exception-and-queue-integrity
	risk: high
	priority: high
	construction_routes: [test-helper]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [maximum-time, overflow, existing-work]
	dimensions: { current_time: Int64.MaxValue, delay: 1 }
	preconditions: [one callback is scheduled before time reaches maximum]
	dependencies: [VirtualScheduler]
	arrange: Schedule one callback and advance virtual time to Int64.MaxValue.
	stimulus: Schedule another callback with delay one.
	expected: [OverflowException]
	expected_exception_or_event: OverflowException
	forbidden: [wrapped due time, callback execution at an earlier time, silent overflow]
	edge_cases: [maximum signed 64-bit timestamp]
	determinism: { clock: virtual, scheduling: synchronous, timeout_or_step_bound: 'one overflowing schedule call' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [scheduler disposal releases its original queued callback]
	resource_risk: timer-retention
	tier: fast
	tags: [Exhaustive, Infrastructure, Boundary]
	related_tests: [INFRA-SCHED-002]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void INFRA_SCHED_003_Scheduler_rejects_virtual_time_overflow()
	{
		using var scheduler = new VirtualScheduler();
		scheduler.Schedule(delayMilliseconds: 1, static () => { });

		try
		{
			scheduler.AdvanceTo(long.MaxValue);
			scheduler.Schedule(delayMilliseconds: 1, static () => { });
			Assert.Fail("Virtual time overflow must be rejected.");
		}
		catch (OverflowException) { }
	}
}
