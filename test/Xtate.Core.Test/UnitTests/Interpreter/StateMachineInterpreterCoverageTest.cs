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

// ReSharper disable MethodHasAsyncOverload

using System.Reflection;
using System.Threading;
using Xtate.DataModel;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Interpreter;

[TestClass]
public class StateMachineInterpreterCoverageTest
{
	[TestMethod]
	public async Task DestroyTerminationCancellationAndEmptyFinalizePathsAreCovered()
	{
		var eventReader = new Mock<IEventReader>();
		var interpreter = CreateInterpreter(eventReader.Object, CancellationToken.None);

		interpreter.TriggerDestroySignal();
		eventReader.Verify(static reader => reader.Complete(), Times.Once);
		var failedDestroyReader = new Mock<IEventReader>();
		CreateInterpreter(failedDestroyReader.Object, CancellationToken.None).TriggerDestroySignal(new InvalidOperationException("destroy"));
		failedDestroyReader.Verify(static reader => reader.Complete(), Times.Once);

		await interpreter.HandleMainLoopFailure(new InvalidOperationException("failure"));
		Assert.AreSame(StateMachineInterpreterState.Terminated, interpreter.NotifiedState);
		Assert.IsTrue(interpreter.IsInterpreterError(new InvalidOperationException("failure")));
		Assert.IsTrue(interpreter.IsInterpreterError(new OperationCanceledException()));

		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();
		var cancelledInterpreter = CreateInterpreter(Mock.Of<IEventReader>(), cancellationTokenSource.Token);
		Assert.IsFalse(cancelledInterpreter.IsInterpreterError(new OperationCanceledException()));

		var invoke = new InvokeNode(new DocumentIdNode(list: null), new InvokeSource()) { DataConverter = null! };
		var applyFinalize = typeof(StateMachineInterpreter).GetMethod(name: "ApplyFinalize", BindingFlags.Instance | BindingFlags.NonPublic)!;
		var result = (ValueTask)applyFinalize.Invoke(interpreter, [invoke])!;
		await result;
	}

	private static TestInterpreter CreateInterpreter(IEventReader eventReader, CancellationToken cancellationToken) =>
		new()
		{
			StateMachineRuntimeError = new StateMachineRuntimeError(new ScopeObject()),
			StateMachineArguments = null!,
			DataConverter = null!,
			CaseSensitivity = null!,
			EventReader = eventReader,
			Logger = null!,
			Model = null!,
			NotifyStateChanged = [],
			UnhandledErrorBehaviour = null,
			StateMachineContext = null!,
			InvokeController = null!,
			DisposeToken = new DisposeToken(cancellationToken)
		};

	[SuppressMessage(category: "ReSharper", checkId: "RedundantOverriddenMember")]
	private sealed class TestInterpreter : StateMachineInterpreter
	{
		public StateMachineInterpreterState? NotifiedState { get; private set; }

		public ValueTask HandleMainLoopFailure(Exception exception) => HandleMainLoopException(exception);

		public bool IsInterpreterError(Exception exception) => IsError(exception);

		public override void TriggerDestroySignal(Exception? innerException) => base.TriggerDestroySignal(innerException);

		protected override ValueTask EnterSteps() => base.EnterSteps();

		protected override ValueTask<List<TransitionNode>> ExternalEventTransitions() => base.ExternalEventTransitions();

		protected override ValueTask<IIncomingEvent> ReadExternalEvent() => base.ReadExternalEvent();

		protected override bool IsError(Exception ex) => base.IsError(ex);

		protected override ValueTask NotifyInterpreterState(StateMachineInterpreterState state)
		{
			NotifiedState = state;

			return default;
		}
	}

	private sealed class InvokeSource : IInvoke
	{
	#region Interface IInvoke

		public FullUri? Type => null;

		public IValueExpression? TypeExpression => null;

		public Uri? Source => null;

		public IValueExpression? SourceExpression => null;

		public string? Id => null;

		public ILocationExpression? IdLocation => null;

		public ImmutableArray<ILocationExpression> NameList => [];

		public bool AutoForward => false;

		public ImmutableArray<IParam> Parameters => [];

		public IFinalize? Finalize => null;

		public IContent? Content => null;

	#endregion
	}
}