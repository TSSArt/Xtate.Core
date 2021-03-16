﻿#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.Persistence
{
	internal sealed class StateMachineSingleMacroStepController : StateMachineControllerBase
	{
		private readonly TaskCompletionSource<StateMachineInterpreterState> _doneCompletionSource = new();
		private readonly CancellationTokenSource                            _suspendTokenSource   = new();

		public StateMachineSingleMacroStepController(SessionId sessionId, IStateMachineOptions? options, IStateMachine? stateMachine, Uri? stateMachineLocation,
													 IStateMachineHost stateMachineHost, InterpreterOptions defaultOptions, ISecurityContext securityContext, DeferredFinalizer finalizer)
				: base(sessionId, options, stateMachine, stateMachineLocation, stateMachineHost, defaultOptions, securityContext, finalizer) { }

		protected override Channel<IEvent> EventChannel { get; } = new SingleItemChannel<IEvent>();

		public override async ValueTask Send(IEvent evt, CancellationToken token)
		{
			await base.Send(evt, token).ConfigureAwait(false);

			var state = await _doneCompletionSource.WaitAsync(token).ConfigureAwait(false);

			if (state == StateMachineInterpreterState.Waiting)
			{
				_suspendTokenSource.Cancel();
			}

			try
			{
				await GetResult(token).ConfigureAwait(false);
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

			private class ChannelReader : ChannelReader<T>
			{
				private TaskCompletionSource<T>? _tcs;

				public ChannelReader(TaskCompletionSource<T> tcs) => _tcs = tcs;

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

					await tcs.WaitAsync(token).ConfigureAwait(false);

					return true;
				}
			}

			private class ChannelWriter : ChannelWriter<T>
			{
				private readonly TaskCompletionSource<T> _tcs;

				public ChannelWriter(TaskCompletionSource<T> tcs) => _tcs = tcs;

				public override bool TryWrite(T item) => _tcs.TrySetResult(item);

				public override ValueTask<bool> WaitToWriteAsync(CancellationToken token = default) => new(!_tcs.Task.IsCompleted);
			}
		}
	}
}