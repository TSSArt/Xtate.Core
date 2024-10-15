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
using Xtate.Service;

namespace Xtate.Core;

public class StateMachineControllerProxy(StateMachineRuntimeController stateMachineRuntimeController) : IStateMachineController
{
	private readonly IStateMachineController _baseStateMachineController = stateMachineRuntimeController;

#region Interface IEventDispatcher

	public ValueTask Send(IEvent evt, CancellationToken token = default) => _baseStateMachineController.Send(evt, token);

#endregion

#region Interface IService

	public ValueTask Destroy() => _baseStateMachineController.Destroy();

	ValueTask<DataModelValue> IService.GetResult() => _baseStateMachineController.GetResult();

#endregion


	//TODO:
	//public ValueTask DisposeAsync() => _baseStateMachineController.DisposeAsync();

	//public void TriggerDestroySignal() => _baseStateMachineController.TriggerDestroySignal();

	//public ValueTask StartAsync(CancellationToken token) => _baseStateMachineController.StartAsync(token);

	//public SessionId SessionId            => _baseStateMachineController.SessionId;
	//public Uri       StateMachineLocation => _baseStateMachineController.StateMachineLocation;
}

public class StateMachineHostExternalCommunication : IExternalCommunication
{
	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	public required IStateMachineLocation?  StateMachineLocation  { private get; [UsedImplicitly] init; }

	public required IStateMachineHost StateMachineHost { private get; [UsedImplicitly] init; }

	private SessionId SessionId => StateMachineSessionId.SessionId;

#region Interface IExternalCommunication

	public ValueTask<SendStatus> TrySendEvent(IOutgoingEvent outgoingEvent) => StateMachineHost.DispatchEvent(SessionId, outgoingEvent, CancellationToken.None);

	public ValueTask ForwardEvent(InvokeId invokeId, IEvent evt) => StateMachineHost.ForwardEvent(SessionId, invokeId, evt, token: default);

	public ValueTask CancelEvent(SendId sendId) => StateMachineHost.CancelEvent(SessionId, sendId, CancellationToken.None);

	public ValueTask StartInvoke(InvokeData invokeData) => StateMachineHost.StartInvoke(SessionId, StateMachineLocation?.Location, invokeData, CancellationToken.None);

	public ValueTask CancelInvoke(InvokeId invokeId) => StateMachineHost.CancelInvoke(SessionId, invokeId, token: default);

#endregion
}

public abstract class StateMachineControllerBase : IStateMachineController, IService, /*IExternalCommunication, */INotifyStateChanged, IAsyncDisposable, IAsyncInitialization, IKeepAlive

{
	private readonly TaskCompletionSource                 _acceptedTcs  = new();
	private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();
	private readonly InterpreterOptions                   _defaultOptions;

	private readonly CancellationTokenSource _destroyTokenSource;
	private readonly DisposingToken          _disposingToken = new();

	//private readonly DeferredFinalizer                    _finalizer;
	private readonly IStateMachineOptions? _options;
	private readonly AsyncInit             _startAsyncInit;

	//private readonly SecurityContext                     _securityContext;
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

		_startAsyncInit = AsyncInit.Run(Start);
	}

	public required IStateMachineInterpreter StateMachineInterpreter { private get; [UsedImplicitly] init; }

	protected abstract Channel<IEvent>   EventChannel     { get; }
	
	public required    IEventQueueWriter EventQueueWriter { private get; [UsedImplicitly] init; }

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

	//public virtual ValueTask Send(IEvent evt, CancellationToken token) => EventChannel.Writer.WriteAsync(evt, token);
	public virtual ValueTask Send(IEvent evt, CancellationToken token) => EventQueueWriter.WriteAsync(evt);

#endregion

#region Interface IKeepAlive

	public Task Wait() => GetResult().AsTask();

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

#region Interface IService

	public ValueTask<DataModelValue> GetResult() => new(_completedTcs.Task);

	ValueTask IService.Destroy()
	{
		TriggerDestroySignal();
		
		//TODO: Wait StateMachine destroyed

		return default;
	}

#endregion

	protected virtual ValueTask Start()
	{
		ExecuteAsync().Forget();

		return new ValueTask(_acceptedTcs.Task.WaitAsync(_disposingToken.Token));
	}

	public async ValueTask StartAsync(CancellationToken token)
	{
		//ExecuteAsync().Forget();

		await _acceptedTcs.Task.WaitAsync(token).ConfigureAwait(false);
	}

	public void TriggerDestroySignal() => _destroyTokenSource.Cancel();

	protected virtual void StateChanged(StateMachineInterpreterState state) { }

	protected virtual ValueTask DisposeAsyncCore()
	{
		_destroyTokenSource.Dispose();
		_disposingToken.Dispose();

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
					//var stateMachineInterpreter = await _stateMachineInterpreterFactory().ConfigureAwait(false);

					var result = await StateMachineInterpreter.RunAsync().ConfigureAwait(false);

					//var result = await stateMachineInterpreter.RunAsync(SessionId, _stateMachine, EventChannel.Reader, GetOptions()).ConfigureAwait(false);
					//await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
					_acceptedTcs.TrySetResult();
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

	//TODO: move to separate class
	/*
#region Interface IExternalCommunication

	ValueTask<SendStatus> IExternalCommunication.
	TrySendEvent(IOutgoingEvent outgoingEvent) => _stateMachineHost.DispatchEvent(SessionId, outgoingEvent, CancellationToken.None);

	ValueTask IExternalCommunication.CancelEvent(SendId sendId) => _stateMachineHost.CancelEvent(SessionId, sendId, CancellationToken.None);

	ValueTask IExternalCommunication.StartInvoke(InvokeData invokeData) => _stateMachineHost.StartInvoke(SessionId, invokeData, /*_securityContext,* / CancellationToken.None);

	ValueTask IExternalCommunication.CancelInvoke(InvokeId invokeId) => _stateMachineHost.CancelInvoke(SessionId, invokeId, CancellationToken.None);

	ValueTask IExternalCommunication.ForwardEvent(IEvent evt, InvokeId invokeId) => _stateMachineHost.ForwardEvent(SessionId, evt, invokeId, CancellationToken.None);

#endregion*/
	/*
	ValueTask IInvokeController.Start(InvokeData invokeData) => _stateMachineHost.StartInvoke(SessionId, StateMachineLocation, invokeData, /*_securityContext, *CancellationToken.None);

	ValueTask IInvokeController.Cancel(InvokeId invokeId) => _stateMachineHost.CancelInvoke(SessionId, invokeId, CancellationToken.None);

	ValueTask IInvokeController.Forward(InvokeId invokeId, IEvent evt) => _stateMachineHost.ForwardEvent(SessionId, evt, invokeId, default);
	*/
}