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
using Xtate.Interpreter;
using Xtate.Logging;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class StateMachineDestroyOnIdleCoverageTest
{
	[TestMethod]
	public async Task FactoryDisablesTrackingForMissingAndInfiniteTimeouts()
	{
		var withoutTimeout = CreateService(timeout: null, out _);
		var infiniteTimeout = CreateService(Timeout.InfiniteTimeSpan, out _);

		Assert.IsNull(await withoutTimeout.Factory());
		Assert.IsNull(await infiniteTimeout.Factory());
	}

	[TestMethod]
	public async Task TrackerArmsAndCancelsTimerCachesInterpreterAndSupportsBothDisposalModes()
	{
		var service = CreateService(TimeSpan.FromHours(1), out var interpreterFactoryCalls);
		var tracker = await service.Factory();
		Assert.IsNotNull(tracker);

		await tracker.OnChanged(StateMachineInterpreterState.Waiting);
		await tracker.OnChanged(StateMachineInterpreterState.Proceed);
		await tracker.OnChanged(StateMachineInterpreterState.Waiting);

		Assert.AreEqual(expected: 1, interpreterFactoryCalls.Count);
		await ((IAsyncDisposable) tracker).DisposeAsync();

		var secondTracker = await service.Factory();
		Assert.IsNotNull(secondTracker);
		((IDisposable) secondTracker).Dispose();
	}

	[TestMethod]
	public async Task ZeroIdleTimeoutTriggersInterpreterDestroySignal()
	{
		var destroyed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static item => item.TriggerDestroySignal()).Callback(destroyed.SetResult);
		var service = CreateService(TimeSpan.Zero, interpreter.Object, Mock.Of<ILogger<StateMachineDestroyOnIdle>>());
		var tracker = await service.Factory();
		Assert.IsNotNull(tracker);

		await tracker.OnChanged(StateMachineInterpreterState.Waiting);
		await destroyed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		await ((IAsyncDisposable) tracker).DisposeAsync();

		interpreter.Verify(static item => item.TriggerDestroySignal(), Times.Once);
	}

	[TestMethod]
	public async Task DestroySignalFailureIsLoggedByTimerCallback()
	{
		var logged = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
		var failure = new InvalidOperationException("destroy failed");
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static item => item.TriggerDestroySignal()).Throws(failure);
		var logger = new Mock<ILogger<StateMachineDestroyOnIdle>>();
		logger.Setup(item => item.Write(Level.Error, 1, It.IsAny<string>(), It.IsAny<Exception>()))
			  .Callback<Level, int, string?, Exception>((_, _, _, exception) => logged.TrySetResult(exception))
			  .Returns(ValueTask.CompletedTask);
		var service = CreateService(TimeSpan.Zero, interpreter.Object, logger.Object);
		var tracker = await service.Factory();
		Assert.IsNotNull(tracker);

		await tracker.OnChanged(StateMachineInterpreterState.Waiting);
		var loggedException = await logged.Task.WaitAsync(TimeSpan.FromSeconds(5));
		await ((IAsyncDisposable) tracker).DisposeAsync();

		Assert.AreSame(failure, loggedException);
	}

	private static StateMachineDestroyOnIdle CreateService(TimeSpan? timeout, out InvocationCounter interpreterFactoryCalls)
	{
		interpreterFactoryCalls = new InvocationCounter();
		var counter = interpreterFactoryCalls;
		var interpreter = Mock.Of<IStateMachineInterpreter>();

		return new StateMachineDestroyOnIdle
			   {
				   Logger = Mock.Of<ILogger<StateMachineDestroyOnIdle>>(),
				   DestroyOnIdleTimeout = timeout is { } value ? new DestroyOnIdleTimeout(value) : null,
				   StateMachineInterpreterFactory = () =>
												{
													counter.Count ++;
													return new ValueTask<IStateMachineInterpreter>(interpreter);
												}
			   };
	}

	private static StateMachineDestroyOnIdle CreateService(
		TimeSpan timeout,
		IStateMachineInterpreter interpreter,
		ILogger<StateMachineDestroyOnIdle> logger) =>
		new()
		{
			Logger = logger,
			DestroyOnIdleTimeout = new DestroyOnIdleTimeout(timeout),
			StateMachineInterpreterFactory = () => new ValueTask<IStateMachineInterpreter>(interpreter)
		};

	private sealed record DestroyOnIdleTimeout(TimeSpan IdleTimeout) : IDestroyOnIdleTimeout;

	private sealed class InvocationCounter
	{
		public int Count { get; set; }
	}
}
