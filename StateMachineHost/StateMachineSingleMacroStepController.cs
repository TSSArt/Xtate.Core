// Copyright © 2019-2024 Sergii Artemenko
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

using System.Threading.Channels;

namespace Xtate.Persistence;

internal sealed class StateMachineSingleMacroStepController(
	SessionId sessionId,
	IStateMachineOptions? options,
	IStateMachine? stateMachine,
	Uri? stateMachineLocation,
	IStateMachineHost stateMachineHost//,
	//InterpreterOptions defaultOptions

	// SecurityContext securityContext,
	//										 DeferredFinalizer finalizer
) : StateMachineControllerBase(sessionId, options, stateMachine, stateMachineLocation, stateMachineHost/*, defaultOptions*/)
{
	private readonly TaskCompletionSource<StateMachineInterpreterState> _doneCompletionSource = new();
	private readonly CancellationTokenSource                            _suspendTokenSource   = new();

	protected override Channel<IEvent> EventChannel { get; } = new SingleItemChannel<IEvent>();

	public override async ValueTask Send(IEvent evt, CancellationToken token)
	{
		await base.Send(evt, token).ConfigureAwait(false);

		var state = await _doneCompletionSource.Task.WaitAsync(token).ConfigureAwait(false);

		if (state == StateMachineInterpreterState.Waiting)
		{
			await _suspendTokenSource.CancelAsync().ConfigureAwait(false);
		}

		try
		{
			await GetResult().ConfigureAwait(false);
		}
		catch (StateMachineSuspendedException) { }
	}

	protected override CancellationToken GetSuspendToken() => _suspendTokenSource.Token;

	protected override void StateChanged(StateMachineInterpreterState state)
	{
		if (state is StateMachineInterpreterState.Waiting or StateMachineInterpreterState.Exited)
		{
			_doneCompletionSource.TrySetResult(state);
		}

		base.StateChanged(state);
	}

	private class SingleItemChannel<T> : Channel<T>
	{
		public SingleItemChannel()
		{
			var tcs = new TaskCompletionSource<T>();
			Reader = new ChannelReader(tcs);
			Writer = new ChannelWriter(tcs);
		}

		private class ChannelReader(TaskCompletionSource<T> tcs) : ChannelReader<T>
		{
			private TaskCompletionSource<T>? _tcs = tcs;

			public override bool TryRead([MaybeNullWhen(false)] out T item)
			{
				if (_tcs is { } tcs)
				{
					_tcs = default;

					item = tcs.Task.Result;

					return true;
				}

				item = default;

				return false;
			}

			public override async ValueTask<bool> WaitToReadAsync(CancellationToken token = default)
			{
				if (_tcs is not { } tcs)
				{
					return false;
				}

				await tcs.Task.WaitAsync(token).ConfigureAwait(false);

				return true;
			}
		}

		private class ChannelWriter(TaskCompletionSource<T> tcs) : ChannelWriter<T>
		{
			public override bool TryWrite(T item) => tcs.TrySetResult(item);

			public override ValueTask<bool> WaitToWriteAsync(CancellationToken token = default) => new(!tcs.Task.IsCompleted);
		}
	}
}