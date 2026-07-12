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

using System.Threading;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.StateMachine;
using Xtate.StateMachineHost;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class StateMachineHostModelCoverageTest
{
	[TestMethod]
	public async Task StateMachineStatusCompletesWhenAcceptedOrForced()
	{
		var accepted = new StateMachineStatus();
		Assert.AreSame(StateMachineInterpreterState.Initializing, accepted.CurrentState);
		Assert.IsFalse(accepted.WhenAccepted().IsCompleted);

		await accepted.OnChanged(StateMachineInterpreterState.Started);
		Assert.AreSame(StateMachineInterpreterState.Started, accepted.CurrentState);
		Assert.IsFalse(accepted.WhenAccepted().IsCompleted);

		await accepted.OnChanged(StateMachineInterpreterState.Accepted);
		await accepted.WhenAccepted();

		var forced = new StateMachineStatus();
		forced.ForceCompleted();
		await forced.WhenAccepted();
	}

	[TestMethod]
	public async Task StateMachineStatusCanBeForcedToFailOrCancel()
	{
		var failed = new StateMachineStatus();
		var failure = new InvalidOperationException("status failure");
		failed.ForceFailed(failure);
		var thrown = await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await failed.WhenAccepted());
		Assert.AreSame(failure, thrown);

		using var cancellationSource = new CancellationTokenSource();
		cancellationSource.Cancel();
		var cancelled = new StateMachineStatus();
		cancelled.ForceCancelled(cancellationSource.Token);
		var cancellation = await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await cancelled.WhenAccepted());
		Assert.AreEqual(cancellationSource.Token, cancellation.CancellationToken);
	}

	[TestMethod]
	public async Task ScheduledEventCopiesRouterEventAndExposesCancelableToken()
	{
		var sender = SessionId.FromString("sender");
		var source = new RouterEventSource
					 {
						 SenderServiceId = sender,
						 IoProcessorData = new DataModelList { ["key"] = new DataModelValue("value") },
						 DelayMs = 25,
						 TargetType = new FullUri("https://example.test/type"),
						 Target = new FullUri("https://example.test/target"),
						 Name = EventName.FromString("event.name"),
						 Type = EventType.External,
						 SendId = SendId.FromString("send"),
						 Origin = new FullUri("https://example.test/origin"),
						 OriginType = new FullUri("https://example.test/origin-type"),
						 InvokeId = InvokeId.FromString("invoke"),
						 Data = new DataModelValue("payload")
					 };

		var scheduledEvent = new ScheduledEvent(source);

		Assert.AreSame(sender, scheduledEvent.SenderServiceId);
		Assert.AreSame(source.IoProcessorData, scheduledEvent.IoProcessorData);
		Assert.AreEqual(expected: 25, scheduledEvent.DelayMs);
		Assert.AreEqual(source.TargetType, scheduledEvent.TargetType);
		Assert.AreEqual(source.Target, scheduledEvent.Target);
		Assert.AreEqual(source.Name, scheduledEvent.Name);
		Assert.AreEqual(source.Type, scheduledEvent.Type);
		Assert.AreEqual(source.SendId, scheduledEvent.SendId);
		Assert.AreEqual(source.Origin, scheduledEvent.Origin);
		Assert.AreEqual(source.OriginType, scheduledEvent.OriginType);
		Assert.AreEqual(source.InvokeId, scheduledEvent.InvokeId);
		Assert.AreEqual(source.Data, scheduledEvent.Data);
		Assert.IsFalse(scheduledEvent.CancellationToken.IsCancellationRequested);

		scheduledEvent.Cancel();

		Assert.IsTrue(scheduledEvent.CancellationToken.IsCancellationRequested);

		await scheduledEvent.CancelAsync();
		await scheduledEvent.Dispose();
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage]() => _ = scheduledEvent.CancellationToken.WaitHandle);
	}

	private sealed class RouterEventSource : IRouterEvent
	{
	#region Interface IIncomingEvent

		public FullUri? OriginType { get; init; }

		public InvokeId? InvokeId { get; init; }

		public EventName Name { get; init; }

		public SendId? SendId { get; init; }

		public EventType Type { get; init; }

		public DataModelValue Data { get; init; }

		public FullUri? Origin { get; init; }

	#endregion

	#region Interface IRouterEvent

		public ServiceId SenderServiceId { get; init; } = null!;

		public DataModelList? IoProcessorData { get; init; }

		public int DelayMs { get; init; }

		public FullUri? TargetType { get; init; }

		public FullUri? Target { get; init; }

	#endregion
	}
}