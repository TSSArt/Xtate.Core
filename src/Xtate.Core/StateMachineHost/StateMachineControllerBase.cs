// Copyright © 2019-2025 Sergii Artemenko
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
using Xtate.IoC;

namespace Xtate.Core;

public abstract class StateMachineControllerBase : IStateMachineController, IAsyncInitialization
{
	private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();

	private readonly AsyncInit _startAsyncInit;

	protected StateMachineControllerBase() => _startAsyncInit = AsyncInit.Run(Start);

	public required IStateMachineStatus StateMachineStatus { private get; [SetByIoC] init; }

	public required IStateMachineInterpreter StateMachineInterpreter { private get; [SetByIoC] init; }

	[Obsolete]
	public required IStateMachineSessionId StateMachineSessionId   { private get; [SetByIoC] init; }

	public required TaskMonitor TaskMonitor { private get; [SetByIoC] init; }

	[Obsolete]
    protected abstract Channel<IIncomingEvent> EventChannel { get; }

	public required IEventQueueWriter EventQueueWriter { private get; [SetByIoC] init; }

	[Obsolete]
	public Uri? StateMachineLocation => default;

	[Obsolete]
	public SessionId SessionId => StateMachineSessionId.SessionId;

#region Interface IAsyncInitialization

	public Task Initialization => _startAsyncInit.Task;

#endregion

#region Interface IEventDispatcher

	public virtual ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => EventQueueWriter.WriteAsync(incomingEvent, token);

#endregion

#region Interface IExternalService

	public ValueTask<DataModelValue> GetResult() => new(_completedTcs.Task);

#endregion

#region Interface IStateMachineController

	public async ValueTask Destroy()
	{
		StateMachineInterpreter.TriggerDestroySignal();

		try
		{
			await _completedTcs.Task.ConfigureAwait(false);
		}
		catch (StateMachineDestroyedException) { }
	}

#endregion

	protected virtual async ValueTask Start()
	{
		ExecuteAsync().Forget(TaskMonitor);

		await StateMachineStatus.WhenAccepted().ConfigureAwait(false);
	}

	[Obsolete]
	protected virtual void StateChanged(StateMachineInterpreterState state) { }

	[Obsolete]
	protected virtual CancellationToken GetSuspendToken() => default; //_defaultOptions.SuspendToken;

	private async ValueTask<DataModelValue> ExecuteAsync()
	{
		try
		{
			var result = await StateMachineInterpreter.Run().ConfigureAwait(false);

			StateMachineStatus.ForceCompleted();

			_completedTcs.TrySetResult(result);

			return result;
		}
		catch (OperationCanceledException ex)
		{
			StateMachineStatus.ForceCancelled(ex.CancellationToken);

			_completedTcs.TrySetCanceled(ex.CancellationToken);

			throw;
		}
		catch (Exception ex)
		{
			StateMachineStatus.ForceFailed(ex);

			_completedTcs.TrySetException(ex);

			throw;
		}
	}

	/*
	private async ValueTask<DataModelValue> ExecuteAsync()
	{
		var initialized = false;

		while (true)
		{
			try
			{
				if (!initialized)
				{
					initialized = true;

					await Initialize().ConfigureAwait(false);
				}

				try
				{
					var result = await StateMachineInterpreter.Run().ConfigureAwait(false);

					StateMachineStatus.ForceCompleted();

					_completedTcs.TrySetResult(result);

					return result;
				}
				catch (StateMachineSuspendedException) /*when (!_defaultOptions.SuspendToken.IsCancellationRequested) *
				{
					// ignore
				}

				await WaitForResume().ConfigureAwait(false);
			}
			catch (OperationCanceledException ex)
			{
				StateMachineStatus.ForceCancelled(ex.CancellationToken);

				_completedTcs.TrySetCanceled(ex.CancellationToken);

				throw;
			}
			catch (Exception ex)
			{
				StateMachineStatus.ForceFailed(ex);

				_completedTcs.TrySetException(ex);

				throw;
			}
		}
	}
*/
   /*
   	private async ValueTask WaitForResume()
	{
		using var anyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(DisposeToken, _destroyCts.Token/*, suspend*);

		try
		{
			if (await EventChannel.Reader.WaitToReadAsync(anyTokenSource.Token).ConfigureAwait(false))
			{
				return;
			}

			await EventChannel.Reader.ReadAsync(anyTokenSource.Token).ConfigureAwait(false);
		}
		/*catch (OperationCanceledException ex) when (ex.CancellationToken == anyTokenSource.Token && _defaultOptions.StopToken.IsCancellationRequested)
		{
			throw new OperationCanceledException(Resources.Exception_StateMachineHasBeenTerminated, ex, _defaultOptions.StopToken);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == anyTokenSource.Token && _defaultOptions.SuspendToken.IsCancellationRequested)
		{
			throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended, ex);
		}*
		catch (ChannelClosedException ex)
		{
			throw new StateMachineQueueClosedException(Resources.Exception_StateMachineExternalQueueHasBeenClosed, ex);
		}
	}*/
}