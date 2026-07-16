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

using Xtate.Interpreter;
using Xtate.Logging;

namespace Xtate.StateMachineHost.Services;

public interface IDestroyOnIdleTimeout
{
	TimeSpan IdleTimeout { get; }
}

[InstantiatedByIoC]
public class StateMachineDestroyOnIdle
{
	private IStateMachineInterpreter? _stateMachineInterpreter;

	public required ILogger<StateMachineDestroyOnIdle> Logger { private get; [SetByIoC] init; }

	public required Func<ValueTask<IStateMachineInterpreter>> StateMachineInterpreterFactory { private get; [SetByIoC] init; }

	public required IDestroyOnIdleTimeout? DestroyOnIdleTimeout { private get; [SetByIoC] init; }

	public ValueTask<INotifyStateChanged?> Factory()
	{
		var idlePeriod = DestroyOnIdleTimeout?.IdleTimeout;

		var stateTracker = idlePeriod != null && idlePeriod != Timeout.InfiniteTimeSpan ? new StateTracker(this) : null;

		return new ValueTask<INotifyStateChanged?>(stateTracker);
	}

	private sealed class StateTracker(StateMachineDestroyOnIdle owner) : INotifyStateChanged, IDisposable, IAsyncDisposable
	{
		private readonly Timer _destroyTimer = new(DestroyOnIdle, owner, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

		public void Dispose() => _destroyTimer.Dispose();

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1
		public ValueTask DisposeAsync() => _destroyTimer.DisposeAsync();
#else
		public ValueTask DisposeAsync()
		{
			_destroyTimer.Dispose();

			return ValueTask.CompletedTask;
		}
#endif

	#region Interface INotifyStateChanged

		public async ValueTask OnChanged(StateMachineInterpreterState state)
		{
			owner._stateMachineInterpreter ??= await owner.StateMachineInterpreterFactory().ConfigureAwait(false);

			if (state == StateMachineInterpreterState.Waiting)
			{
				_destroyTimer.Change(owner.DestroyOnIdleTimeout!.IdleTimeout, Timeout.InfiniteTimeSpan);
			}

			if (state == StateMachineInterpreterState.Proceed)
			{
				_destroyTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			}
		}

	#endregion

		private static void DestroyOnIdle(object? state)
		{
			var owner = (StateMachineDestroyOnIdle?) state;

			Infra.NotNull(owner);

			try
			{
				Infra.NotNull(owner._stateMachineInterpreter);

				owner._stateMachineInterpreter.TriggerDestroySignal();
			}
			catch (Exception ex)
			{
				owner.Logger.Write(Level.Error, eventId: 1, Resources.Message_AnErrorOccurredWhileTriggeringTheDestroySignal, ex);
			}
		}
	}
}