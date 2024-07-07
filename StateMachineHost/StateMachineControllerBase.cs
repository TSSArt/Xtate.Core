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
using Xtate.Service;

namespace Xtate.Core;

public class StateMachineControllerProxy(StateMachineRuntimeController stateMachineRuntimeController) : IStateMachineController
{
	private readonly IStateMachineController _baseStateMachineController = stateMachineRuntimeController;

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync() => _baseStateMachineController.DisposeAsync();

#endregion

#region Interface IEventDispatcher

	public ValueTask Send(IEvent evt, CancellationToken token = default) => _baseStateMachineController.Send(evt, token);

#endregion

#region Interface IService

	public ValueTask Destroy(CancellationToken token) => _baseStateMachineController.Destroy(token);

	ValueTask<DataModelValue> IService.GetResult(CancellationToken token) => _baseStateMachineController.GetResult(token);

#endregion

#region Interface IStateMachineController

	public void TriggerDestroySignal() => _baseStateMachineController.TriggerDestroySignal();

	public ValueTask StartAsync(CancellationToken token) => _baseStateMachineController.StartAsync(token);

	public SessionId SessionId            => _baseStateMachineController.SessionId;
	public Uri       StateMachineLocation => _baseStateMachineController.StateMachineLocation;

#endregion
}

public abstract class StateMachineControllerBase : IStateMachineController, IService, IExternalCommunication, INotifyStateChanged, IAsyncDisposable, IInvokeController
{
	private readonly TaskCompletionSource<int>            _acceptedTcs  = new();
	private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();
	private readonly InterpreterOptions                   _defaultOptions;

	private readonly CancellationTokenSource _destroyTokenSource;

	//private readonly DeferredFinalizer                    _finalizer;
	private readonly IStateMachineOptions? _options;

	//private readonly ISecurityContext                     _securityContext;
	private readonly IStateMachine?    _stateMachine;
	private readonly IStateMachineHost _stateMachineHost;

	protected StateMachineControllerBase(SessionId sessionId,
										 IStateMachineOptions? options,
										 IStateMachine? stateMachine,
										 Uri? stateMachineLocation,
										 IStateMachineHost stateMachineHost,
										 InterpreterOptions defaultOptions)
	{
		SessionId = sessionId;
		StateMachineLocation = stateMachineLocation;
		_options = options;
		_stateMachine = stateMachine;
		_stateMachineHost = stateMachineHost;
		_defaultOptions = defaultOptions;

		//_securityContext = securityContext;
		//_finalizer = finalizer;

		_destroyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_defaultOptions.DestroyToken, token2: default);
	}

	public required Func<ValueTask<IStateMachineInterpreter>> _stateMachineInterpreterFactory { private get; [UsedImplicitly] init; }

	protected abstract Channel<IEvent>   EventChannel     { get; }
	public required    IEventQueueWriter EventQueueWriter { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IEventDispatcher

	//public virtual ValueTask Send(IEvent evt, CancellationToken token) => EventChannel.Writer.WriteAsync(evt, token);
	public virtual ValueTask Send(IEvent evt, CancellationToken token) => EventQueueWriter.WriteAsync(evt);

#endregion

#region Interface IExternalCommunication

	ValueTask<SendStatus> IExternalCommunication.TrySendEvent(IOutgoingEvent outgoingEvent) => _stateMachineHost.DispatchEvent(SessionId, outgoingEvent, CancellationToken.None);

	ValueTask IExternalCommunication.CancelEvent(SendId sendId) => _stateMachineHost.CancelEvent(SessionId, sendId, CancellationToken.None);

	ValueTask IExternalCommunication.StartInvoke(InvokeData invokeData) => _stateMachineHost.StartInvoke(SessionId, invokeData, /*_securityContext,*/ CancellationToken.None);

	ValueTask IExternalCommunication.CancelInvoke(InvokeId invokeId) => _stateMachineHost.CancelInvoke(SessionId, invokeId, CancellationToken.None);

	ValueTask IExternalCommunication.ForwardEvent(IEvent evt, InvokeId invokeId) => _stateMachineHost.ForwardEvent(SessionId, evt, invokeId, CancellationToken.None);

#endregion

#region Interface IInvokeController

	ValueTask IInvokeController.Start(InvokeData invokeData) => _stateMachineHost.StartInvoke(SessionId, invokeData, /*_securityContext, */CancellationToken.None);

	ValueTask IInvokeController.Cancel(InvokeId invokeId) => _stateMachineHost.CancelInvoke(SessionId, invokeId, CancellationToken.None);

#endregion

#region Interface INotifyStateChanged

	ValueTask INotifyStateChanged.OnChanged(StateMachineInterpreterState state)
	{
		StateChanged(state);

		if (state == StateMachineInterpreterState.Accepted)
		{
			_acceptedTcs.TrySetResult(0);
		}

		return default;
	}

#endregion

#region Interface IService

	public ValueTask<DataModelValue> GetResult(CancellationToken token) => _completedTcs.WaitAsync(token);

	ValueTask IService.Destroy(CancellationToken token)
	{
		TriggerDestroySignal();

		//TODO: Wait StateMachine destroyed

		return default;
	}

#endregion

#region Interface IStateMachineController

	public async ValueTask StartAsync(CancellationToken token)
	{
		ExecuteAsync().Forget();

		await _acceptedTcs.WaitAsync(token).ConfigureAwait(false);
	}

	public void TriggerDestroySignal() => _destroyTokenSource.Cancel();

	public Uri? StateMachineLocation { get; }

	public SessionId SessionId { get; }

#endregion

	protected virtual void StateChanged(StateMachineInterpreterState state) { }

	protected virtual ValueTask DisposeAsyncCore()
	{
		_destroyTokenSource.Dispose();

		return default;
	}

	protected virtual CancellationToken GetSuspendToken() => _defaultOptions.SuspendToken;

	protected virtual ValueTask Initialize() => default;

	private async ValueTask<DataModelValue> ExecuteAsync()
	{
		//_finalizer.DefferFinalization();
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
					//var stateMachineInterpreter = _defaultOptions.ServiceLocator.GetService<IStateMachineInterpreter>();
					var stateMachineInterpreter = await _stateMachineInterpreterFactory().ConfigureAwait(false);
					var result = await stateMachineInterpreter.RunAsync().ConfigureAwait(false);

					//var result = await stateMachineInterpreter.RunAsync(SessionId, _stateMachine, EventChannel.Reader, GetOptions()).ConfigureAwait(false);
					//await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
					_acceptedTcs.TrySetResult(0);
					_completedTcs.TrySetResult(result);

					return result;
				}
				catch (StateMachineSuspendedException) when (!_defaultOptions.SuspendToken.IsCancellationRequested) { }

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
		var anyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_defaultOptions.StopToken, _defaultOptions.DestroyToken, _defaultOptions.SuspendToken);
		try
		{
			if (await EventChannel.Reader.WaitToReadAsync(anyTokenSource.Token).ConfigureAwait(false))
			{
				return;
			}

			await EventChannel.Reader.ReadAsync(anyTokenSource.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == anyTokenSource.Token && _defaultOptions.StopToken.IsCancellationRequested)
		{
			throw new OperationCanceledException(Resources.Exception_StateMachineHasBeenHalted, ex, _defaultOptions.StopToken);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == anyTokenSource.Token && _defaultOptions.SuspendToken.IsCancellationRequested)
		{
			throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended, ex);
		}
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