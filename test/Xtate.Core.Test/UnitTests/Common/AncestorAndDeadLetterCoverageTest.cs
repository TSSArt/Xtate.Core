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

using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.Logging;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class AncestorAndDeadLetterCoverageTest
{
	[TestMethod]
	public void AncestorContainerExposesValueAndNullableAncestor()
	{
		var value = new object();
		var ancestor = new object();
		var container = new AncestorContainer(value, ancestor);

		Assert.AreSame(value, container.Value);
		Assert.AreSame(ancestor, ((IAncestorProvider) container).Ancestor);
		Assert.IsNull(((IAncestorProvider) new AncestorContainer(value, ancestor: null)).Ancestor);
	}

	[TestMethod]
	public void AncestorSearchHandlesDirectContainerProviderAndMissingValues()
	{
		var value = new object();
		var direct = new Ancestor<object>(value);
		Assert.IsTrue(direct.Is<object>(out var directResult));
		Assert.AreSame(value, directResult);
		Assert.AreSame(value, direct.As<object>());

		var container = new Ancestor<object>(new AncestorContainer(value, ancestor: null));
		Assert.IsTrue(container.Is<object>(out var containerResult));
		Assert.AreSame(value, containerResult);

		var providerValue = new TargetValue();
		var providerSource = new ProviderSource(new ProviderSource(providerValue));
		var provider = new Ancestor<ProviderSource>(providerSource);
		Assert.IsTrue(provider.Is<TargetValue>(out var providerResult));
		Assert.AreSame(providerValue, providerResult);

		var missing = new Ancestor<string>("text");
		Assert.IsFalse(missing.Is<IDisposable>(out var missingResult));
		Assert.IsNull(missingResult);
		Assert.IsTrue(IsAncestor<object>(value));
		Assert.IsFalse(IsAncestor<IDisposable>("text"));
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private static bool IsAncestor<T>(object value)
	{
		var ancestor = new Ancestor<object>(value);

		return ancestor.Is<T>();
	}

	[TestMethod]
	public async Task DeadLetterQueueWritesWarningForUndeliverableEvent()
	{
		var logger = new Mock<ILogger<IDeadLetterQueue<TestSource>>>();
		logger.Setup(static l => l.IsEnabled(Level.Warning)).Returns(false);
		var incomingEvent = Mock.Of<IIncomingEvent>();
		var queue = new DeadLetterQueue<TestSource> { Logger = logger.Object };

		await queue.Enqueue(SessionId.FromString("recipient"), incomingEvent);

		logger.Verify(static l => l.IsEnabled(Level.Warning), Times.Once);

		var enabledLogger = new Mock<ILogger<IDeadLetterQueue<TestSource>>>();
		enabledLogger.Setup(static l => l.IsEnabled(Level.Warning)).Returns(true);
		var enabledQueue = new DeadLetterQueue<TestSource> { Logger = enabledLogger.Object };
		await enabledQueue.Enqueue(InvokeId.FromString("invoke"), incomingEvent);
		Assert.AreEqual(expected: 1, enabledLogger.Invocations.Count(static invocation => invocation.Method.Name == nameof(ILogger<IDeadLetterQueue<TestSource>>.Write)));
	}

	public sealed class TestSource;

	private sealed class ProviderSource(object? ancestor) : IAncestorProvider
	{
		public object? Ancestor => ancestor;
	}

	private sealed class TargetValue;
}
