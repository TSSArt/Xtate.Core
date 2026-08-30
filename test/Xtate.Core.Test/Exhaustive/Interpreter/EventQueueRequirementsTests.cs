using System.Threading;
using System.Threading.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class EventQueueRequirementsTests
{
	/*
	TEST-METADATA
	test_id: SCXML-EVENT-004-EXISTING-001
	requirement_ids: [SCXML-EVENT-004]
	title: External queue FIFO and completion
	description: Two host-dispatched external events must be read in dispatch order, then queue completion wakes readers and rejects a later dispatch.
	authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: External events are FIFO and a closed queue accepts no later event. }
	phase: 2
	feature: event-queue
	target_components: [EventQueue]
	test_kind: unit
	oracle_type: ordered-events-and-exact-exception
	risk: high
	priority: high
	construction_routes: [public-api]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,cleanup]
	dimensions: { producer: host-dispatch, terminal: complete }
	preconditions: [fresh queue]
	dependencies: [System.Threading.Channels]
	arrange: Dispatch first then second external incoming event.
	stimulus: Read events, complete queue, wait, and dispatch late event.
	expected: Read first then second; waiter returns false; late dispatch throws ChannelClosedException.
	expected_exception_or_event: ChannelClosedException
	forbidden: Reordered, duplicated, retained, or post-completion event.
	edge_cases: Completion after draining accepted events.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 6 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Dispose queue through using]
	resource_risk: blocked-reader retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-EVENT-004-EXISTING-002]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_004_External_queue_preserves_FIFO_order_and_completion_releases_waiters()
	{
		using var queue = new EventQueue();
		await queue.Dispatch(new IncomingEvent { Name = EventName.FromString("first"), Type = EventType.External }, CancellationToken.None);
		await queue.Dispatch(new IncomingEvent { Name = EventName.FromString("second"), Type = EventType.External }, CancellationToken.None);

		Assert.IsTrue(queue.TryReadEvent(out var first));
		Assert.AreEqual("first", first.Name.ToString());
		Assert.IsTrue(queue.TryReadEvent(out var second));
		Assert.AreEqual("second", second.Name.ToString());
		Assert.IsFalse(queue.TryReadEvent(out _));

		queue.Complete();
		Assert.IsFalse(await queue.WaitToEvent());
		await Assert.ThrowsExactlyAsync<ChannelClosedException>(async () => await queue.Dispatch(new IncomingEvent { Name = EventName.FromString("late") }, CancellationToken.None));
	}

	/* TEST-METADATA
	test_id: SCXML-EVENT-006-EXISTING-001
	requirement_ids: [SCXML-EVENT-006]
	title: Disposal drains accepted event and wakes waiters
	description: Disposal preserves an accepted payload for drain, terminates waiting, and rejects later dispatch without retaining a blocked consumer.
	authority: { source: Xtate EventQueue public contract, section: SCXML-EVENT-006, citation_or_rule: Queue shutdown releases pending payloads and waiters. }
	phase: 2
	feature: event-queue
	target_components: [EventQueue]
	test_kind: unit
	oracle_type: ordered-event-and-exact-exception
	risk: high
	priority: high
	construction_routes: [public-api]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [cleanup,resource]
	dimensions: { terminal: dispose, pending_event: one }
	preconditions: [one accepted event]
	dependencies: [System.Threading.Channels]
	arrange: Dispatch accepted event then dispose queue.
	stimulus: Drain, await readiness, and dispatch rejected event.
	expected: Accepted event readable, waiter false, later dispatch ChannelClosedException.
	expected_exception_or_event: ChannelClosedException
	forbidden: Lost payload, blocked waiter, or accepted late event.
	edge_cases: Disposal before accepted item read.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 5 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Queue disposed]
	resource_risk: payload-and-waiter retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-CANCEL-002-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_006_Disposal_completes_the_queue_after_already_accepted_events_are_drained()
	{
		var queue = new EventQueue();
		await queue.Dispatch(new IncomingEvent { Name = EventName.FromString("accepted"), Type = EventType.External }, CancellationToken.None);

		queue.Dispose();

		Assert.IsTrue(queue.TryReadEvent(out var accepted));
		Assert.AreEqual("accepted", accepted.Name.ToString());
		Assert.IsFalse(await queue.WaitToEvent());
		await Assert.ThrowsExactlyAsync<ChannelClosedException>(async () => await queue.Dispatch(new IncomingEvent { Name = EventName.FromString("rejected") }, CancellationToken.None));
	}

	/* TEST-METADATA
	test_id: SCXML-EVENT-004-EXISTING-002
	requirement_ids: [SCXML-EVENT-004]
	title: Cancelled dispatch has no queue side effect
	description: A pre-cancelled dispatch fails at the dispatch boundary and cannot enqueue even a partially accepted external event.
	authority: { source: Xtate EventQueue public contract, section: SCXML-EVENT-004, citation_or_rule: Concurrent dispatch has no partially accepted payload. }
	phase: 2
	feature: event-queue
	target_components: [EventQueue]
	test_kind: unit
	oracle_type: exact-exception-and-empty-queue
	risk: high
	priority: high
	construction_routes: [public-api]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [cancellation,concurrency]
	dimensions: { cancellation_point: before-dispatch }
	preconditions: [fresh queue and cancelled token]
	dependencies: [CancellationTokenSource]
	arrange: Cancel token before dispatch.
	stimulus: Dispatch event then attempt queue read.
	expected: Dispatch throws TaskCanceledException and queue is empty.
	expected_exception_or_event: TaskCanceledException
	forbidden: Any enqueued cancelled payload.
	edge_cases: Cancellation before rather than during write.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 2 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Dispose queue and cancellation source]
	resource_risk: cancelled-write retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-EVENT-004-EXISTING-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_004_Cancelled_dispatch_does_not_enqueue_an_event()
	{
		using var queue = new EventQueue();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
			await queue.Dispatch(new IncomingEvent { Name = EventName.FromString("cancelled"), Type = EventType.External }, cancellation.Token));

		Assert.IsFalse(queue.TryReadEvent(out _));
	}
}
