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
using Xtate.IoC;

namespace Xtate.Core;

public abstract class StateMachineControllerBase : IStateMachineController, INotifyStateChanged, IAsyncDisposable, IAsyncInitialization

{
	private readonly TaskCompletionSource _acceptedTcs = new();

	private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();

	private readonly CancellationTokenSource _destroyTokenSource;

	private readonly DisposingToken _disposingToken = new();

	private readonly AsyncInit _startAsyncInit;

	protected StateMachineControllerBase(SessionId sessionId,
										 IStateMachineOptions? options,
										 IStateMachine? stateMachine,
										 Uri? stateMachineLocation,
										 IStateMachineHost stateMachineHost)
	{
		SessionId = sessionId;
		StateMachineLocation = stateMachineLocation;

		_destroyTokenSource = CancellationTokenSource.CreateLinkedTokenSource( /*_defaultOptions.DestroyToken*/token1: default, token2: default);

		_startAsyncInit = AsyncInit.Run(Start);
	}

	public required IStateMachineInterpreter StateMachineInterpreter { private get; [UsedImplicitly] init; }
	
	public required TaskCollector TaskCollector { private get; [UsedImplicitly] init; }

	protected abstract Channel<IIncomingEvent> EventChannel { get; }

	public required IEventQueueWriter EventQueueWriter { private get; [UsedImplicitly] init; }

	public Uri? StateMachineLocation { get; }

	public SessionId SessionId { get; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IAsyncInitialization

	public Task Initialization => _startAsyncInit.Task;

#endregion

#region Interface IEventDispatcher

	//public virtual ValueTask Send(IIncomingEvent abc, CancellationToken token) => EventChannel.Writer.WriteAsync(abc, token);
	public virtual ValueTask Dispatch(IIncomingEvent incomingEvent) => EventQueueWriter.WriteAsync(incomingEvent);

#endregion

#region Interface IExternalService

	public ValueTask<DataModelValue> GetResult() => new(_completedTcs.Task);

#endregion

#region Interface INotifyStateChanged

	ValueTask INotifyStateChanged.OnChanged(StateMachineInterpreterState state)
	{
		StateChanged(state);

		if (state == StateMachineInterpreterState.Accepted)
		{
			_acceptedTcs.TrySetResult();
		}

		return default;
	}

#endregion

	public Task Wait() => GetResult().AsTask();

	protected virtual ValueTask Start()
	{
		var valueTask = ExecuteAsync();
		
		TaskCollector.Collect(valueTask);

		return new ValueTask(_acceptedTcs.Task.WaitAsync(_disposingToken.Token));
	}

	public void TriggerDestroySignal() => _destroyTokenSource.Cancel();

	protected virtual void StateChanged(StateMachineInterpreterState state) { }

	protected virtual ValueTask DisposeAsyncCore()
	{
		_destroyTokenSource.Dispose();
		_disposingToken.Dispose();

		return default;
	}

	protected virtual CancellationToken GetSuspendToken() => default; //_defaultOptions.SuspendToken;

	protected virtual ValueTask Initialize() => default;

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
					var result = await StateMachineInterpreter.RunAsync().ConfigureAwait(false);

					_acceptedTcs.TrySetResult();
					_completedTcs.TrySetResult(result);

					return result;
				}
				catch (StateMachineSuspendedException) /*when (!_defaultOptions.SuspendToken.IsCancellationRequested) */ { }

				await WaitForResume().ConfigureAwait(false);
			}
			catch (OperationCanceledException ex)
			{
				//await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
				_acceptedTcs.TrySetCanceled(ex.CancellationToken);
				_completedTcs.TrySetCanceled(ex.CancellationToken);

				throw;
			}
			catch (Exception ex)
			{
				//await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
				_acceptedTcs.TrySetException(ex);
				_completedTcs.TrySetException(ex);

				throw;
			}
		}
	}

	private async ValueTask WaitForResume()
	{
		//var anyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_defaultOptions.StopToken, _defaultOptions.DestroyToken, _defaultOptions.SuspendToken);
		var anyTokenSource = new CancellationTokenSource();

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
			throw new OperationCanceledException(Resources.Exception_StateMachineHasBeenHalted, ex, _defaultOptions.StopToken);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == anyTokenSource.Token && _defaultOptions.SuspendToken.IsCancellationRequested)
		{
			throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended, ex);
		}*/
		catch (ChannelClosedException ex)
		{
			throw new StateMachineQueueClosedException(Resources.Exception_StateMachineExternalQueueHasBeenClosed, ex);
		}
		finally
		{
			anyTokenSource.Dispose();
		}
	}
}