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

using System.Threading.Channels;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.TaskMonitor;

namespace Xtate.StateMachineHost.Services;

public abstract class StateMachineControllerBase : IStateMachineController, IAsyncInitialization
{
	private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();

	private readonly AsyncInit<StateMachineControllerBase> _start = new(smc => smc.Start());

	public required IStateMachineStatus StateMachineStatus { private get; [SetByIoC] init; }

	public required IStateMachineInterpreter StateMachineInterpreter { private get; [SetByIoC] init; }
	
	public required ITaskMonitor TaskMonitor { private get; [SetByIoC] init; }


	public required IEventDispatcher EventDispatcher { private get; [SetByIoC] init; }

	
#region Interface IAsyncInitialization

	public virtual ValueTask InitializeAsync() => AsyncInit.For(this).Run(_start);

#endregion

#region Interface IEventDispatcher

	public virtual ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => EventDispatcher.Dispatch(incomingEvent, token);

#endregion

#region Interface IExternalService

	public virtual ValueTask<DataModelValue> GetResult()
	{
		_start.EnsureInitialized();

		return new ValueTask<DataModelValue>(_completedTcs.Task);
	}

#endregion

#region Interface IStateMachineController

	public virtual async ValueTask Destroy()
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