#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xtate.IoProcessor;
using Xtate.Persistence;
using Xtate.Service;

namespace Xtate.Core
{
	internal class StateMachineController : IStateMachineController, IService, IExternalCommunication, INotifyStateChanged, ILoggerContext, IAsyncDisposable
	{
		private static readonly UnboundedChannelOptions UnboundedSynchronousChannelOptions  = new() { SingleReader = true, AllowSynchronousContinuations = true };
		private static readonly UnboundedChannelOptions UnboundedAsynchronousChannelOptions = new() { SingleReader = true, AllowSynchronousContinuations = false };

		private readonly TaskCompletionSource<int>            _acceptedTcs  = new();
		private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();
		private readonly InterpreterOptions                   _defaultOptions;
		private readonly CancellationTokenSource              _destroyTokenSource;
		private readonly DeferredFinalizer                    _finalizer;
		private readonly TimeSpan                             _idlePeriod;
		private readonly ILogger                              _logger;
		private readonly IStateMachineOptions?                _options;
		private readonly HashSet<ScheduledEvent>              _scheduledEvents = new();
		private readonly ISecurityContext                     _securityContext;
		private readonly IStateMachine?                       _stateMachine;
		private readonly IStateMachineHost                    _stateMachineHost;
		private readonly ConcurrentQueue<ScheduledEvent>      _toDelete = new();

		private bool                     _disposed;
		private CancellationTokenSource? _suspendOnIdleTokenSource;
		private CancellationTokenSource? _suspendTokenSource;

		public StateMachineController(SessionId sessionId, IStateMachineOptions? options, IStateMachine? stateMachine, Uri? stateMachineLocation, IStateMachineHost stateMachineHost,
									  TimeSpan idlePeriod, InterpreterOptions defaultOptions, ISecurityContext securityContext, DeferredFinalizer finalizer)
		{
			SessionId = sessionId;
			StateMachineLocation = stateMachineLocation;
			_options = options;
			_stateMachine = stateMachine;
			_stateMachineHost = stateMachineHost;
			_defaultOptions = defaultOptions;
			_securityContext = securityContext;
			_finalizer = finalizer;
			_idlePeriod = idlePeriod;
			_logger = _defaultOptions.Logger ?? DefaultLogger.Instance;

			_destroyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_defaultOptions.DestroyToken, token2: default);

			Channel = CreateChannel(options);
		}

		protected virtual Channel<IEvent> Channel { get; }

		public Uri? StateMachineLocation { get; }

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore().ConfigureAwait(false);

			GC.SuppressFinalize(this);
		}

	#endregion

	#region Interface IEventDispatcher

		public ValueTask Send(IEvent evt, CancellationToken token) => Channel.Writer.WriteAsync(evt, token);

	#endregion

	#region Interface IExternalCommunication

		ImmutableArray<IIoProcessor> IExternalCommunication.GetIoProcessors() => _stateMachineHost.GetIoProcessors();

		async ValueTask<SendStatus> IExternalCommunication.TrySendEvent(IOutgoingEvent evt, CancellationToken token)
		{
			var sendStatus = await _stateMachineHost.DispatchEvent(SessionId, evt, skipDelay: false, token).ConfigureAwait(false);

			if (sendStatus == SendStatus.ToSchedule)
			{
				await ScheduleEvent(evt, token).ConfigureAwait(false);

				return SendStatus.Sent;
			}

			return sendStatus;
		}

		async ValueTask IExternalCommunication.CancelEvent(SendId sendId, CancellationToken token)
		{
			CleanScheduledEvents();

			foreach (var evt in _scheduledEvents)
			{
				if (evt.Event.SendId == sendId)
				{
					await CancelEvent(evt, token).ConfigureAwait(false);
				}
			}
		}

		ValueTask IExternalCommunication.StartInvoke(InvokeData invokeData, CancellationToken token) => _stateMachineHost.StartInvoke(SessionId, invokeData, _securityContext, token);

		ValueTask IExternalCommunication.CancelInvoke(InvokeId invokeId, CancellationToken token) => _stateMachineHost.CancelInvoke(SessionId, invokeId, token);

		bool IExternalCommunication.IsInvokeActive(InvokeId invokeId) => _stateMachineHost.IsInvokeActive(SessionId, invokeId);

		ValueTask IExternalCommunication.ForwardEvent(IEvent evt, InvokeId invokeId, CancellationToken token) => _stateMachineHost.ForwardEvent(SessionId, evt, invokeId, token);

	#endregion

	#region Interface ILoggerContext

		public string? GetDataModelAsText() => default;

		public string ConvertToText(in DataModelValue dataModelValue) => dataModelValue.ToString(CultureInfo.InvariantCulture);

		DataModelList? ILoggerContext.GetDataModel() => default;

		ImmutableArray<string> ILoggerContext.GetActiveStates() => default;

		public SessionId SessionId { get; }

		IStateMachine? ILoggerContext.StateMachine => _stateMachine;

		string? ILoggerContext.StateMachineName => _options?.Name;

	#endregion

	#region Interface INotifyStateChanged

		ValueTask INotifyStateChanged.OnChanged(StateMachineInterpreterState state)
		{
			if (state == StateMachineInterpreterState.Accepted)
			{
				_acceptedTcs.TrySetResult(0);
			}
			else if (state == StateMachineInterpreterState.Waiting)
			{
				_suspendOnIdleTokenSource?.CancelAfter(_idlePeriod);
			}

			return default;
		}

	#endregion

	#region Interface IService

		ValueTask IService.Destroy(CancellationToken token)
		{
			TriggerDestroySignal();

			return default;
		}

	#endregion

	#region Interface IStateMachineController

		public ValueTask<DataModelValue> GetResult(CancellationToken token) => _completedTcs.WaitAsync(token);

	#endregion

		protected virtual ValueTask DisposeAsyncCore()
		{
			if (_disposed)
			{
				return default;
			}

			_destroyTokenSource.Dispose();
			_suspendTokenSource?.Dispose();
			_suspendOnIdleTokenSource?.Dispose();

			_disposed = true;

			return default;
		}

		private static Channel<IEvent> CreateChannel(IStateMachineOptions? options)
		{
			if (options is null)
			{
				return System.Threading.Channels.Channel.CreateUnbounded<IEvent>(UnboundedAsynchronousChannelOptions);
			}

			var sync = options.SynchronousEventProcessing ?? false;
			var queueSize = options.ExternalQueueSize ?? 0;

			if (options.IsStateMachinePersistable() || queueSize <= 0)
			{
				return System.Threading.Channels.Channel.CreateUnbounded<IEvent>(sync ? UnboundedSynchronousChannelOptions : UnboundedAsynchronousChannelOptions);
			}

			var channelOptions = new BoundedChannelOptions(queueSize) { AllowSynchronousContinuations = sync, SingleReader = true };

			return System.Threading.Channels.Channel.CreateBounded<IEvent>(channelOptions);
		}

		public async ValueTask StartAsync(CancellationToken token)
		{
			ExecuteAsync().Forget();

			await _acceptedTcs.WaitAsync(token).ConfigureAwait(false);
		}

		private InterpreterOptions GetOptions() =>
				_defaultOptions with
				{
						ExternalCommunication = this,
						StorageProvider = this as IStorageProvider,
						NotifyStateChanged = this,
						SecurityContext = _securityContext,
						DestroyToken = _destroyTokenSource.Token,
						SuspendToken = GetSuspendToken(),
						UnhandledErrorBehaviour = _options?.UnhandledErrorBehaviour is { } behaviour ? behaviour : _defaultOptions.UnhandledErrorBehaviour
				};

		private CancellationToken GetSuspendToken()
		{
			if (_idlePeriod == TimeSpan.Zero)
			{
				return _defaultOptions.SuspendToken;
			}

			_suspendTokenSource?.Dispose();
			_suspendOnIdleTokenSource?.Dispose();

			_suspendOnIdleTokenSource = new CancellationTokenSource(_idlePeriod);

			_suspendTokenSource = _defaultOptions.SuspendToken.CanBeCanceled
					? CancellationTokenSource.CreateLinkedTokenSource(_defaultOptions.SuspendToken, _suspendOnIdleTokenSource.Token)
					: _suspendOnIdleTokenSource;

			return _suspendTokenSource.Token;
		}

		protected virtual ValueTask Initialize() => default;

		private async ValueTask<DataModelValue> ExecuteAsync()
		{
			_finalizer.DefferFinalization();
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
						var result = await StateMachineInterpreter.RunAsync(SessionId, _stateMachine, Channel.Reader, GetOptions()).ConfigureAwait(false);
						await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
						_acceptedTcs.TrySetResult(0);
						_completedTcs.TrySetResult(result);

						return result;
					}
					catch (StateMachineSuspendedException) when (!_defaultOptions.SuspendToken.IsCancellationRequested) { }

					await WaitForResume().ConfigureAwait(false);
				}
				catch (OperationCanceledException ex)
				{
					await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
					_acceptedTcs.TrySetCanceled(ex.CancellationToken);
					_completedTcs.TrySetCanceled(ex.CancellationToken);

					throw;
				}
				catch (Exception ex)
				{
					await _finalizer.ExecuteDeferredFinalization().ConfigureAwait(false);
					_acceptedTcs.TrySetException(ex);
					_completedTcs.TrySetException(ex);

					throw;
				}
			}
		}

		public void TriggerDestroySignal()
		{
			_destroyTokenSource.Cancel();
		}

		private async ValueTask WaitForResume()
		{
			var anyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_defaultOptions.StopToken, _defaultOptions.DestroyToken, _defaultOptions.SuspendToken);
			try
			{
				if (await Channel.Reader.WaitToReadAsync(anyTokenSource.Token).ConfigureAwait(false))
				{
					return;
				}

				await Channel.Reader.ReadAsync(anyTokenSource.Token).ConfigureAwait(false);
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

		protected virtual ValueTask ScheduleEvent(IOutgoingEvent evt, CancellationToken token)
		{
			if (evt is null) throw new ArgumentNullException(nameof(evt));

			var scheduledEvent = new ScheduledEvent(evt);

			_scheduledEvents.Add(scheduledEvent);

			DelayedFire(scheduledEvent, evt.DelayMs).Forget();

			CleanScheduledEvents();

			return default;
		}

		private void CleanScheduledEvents()
		{
			while (_toDelete.TryDequeue(out var scheduledEvent))
			{
				_scheduledEvents.Remove(scheduledEvent);
			}
		}

		protected async ValueTask DelayedFire(ScheduledEvent scheduledEvent, int delayMs)
		{
			if (scheduledEvent is null) throw new ArgumentNullException(nameof(scheduledEvent));

			try
			{
				await Task.Delay(delayMs, scheduledEvent.CancellationToken).ConfigureAwait(false);

				await CancelEvent(scheduledEvent, token: default).ConfigureAwait(false);

				try
				{
					await _stateMachineHost.DispatchEvent(SessionId, scheduledEvent.Event, skipDelay: true, token: default).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					await _logger.LogError(this, ErrorType.Communication, ex, scheduledEvent.Event.SendId?.Value, token: default).ConfigureAwait(false);
				}
			}
			finally
			{
				scheduledEvent.Dispose();
			}
		}

		protected virtual ValueTask CancelEvent(ScheduledEvent scheduledEvent, CancellationToken token)
		{
			if (scheduledEvent is null) throw new ArgumentNullException(nameof(scheduledEvent));

			scheduledEvent.Cancel();

			_toDelete.Enqueue(scheduledEvent);

			return default;
		}

		protected class ScheduledEvent
		{
			private readonly CancellationTokenSource _cancellationTokenSource = new();

			public ScheduledEvent(IOutgoingEvent evt) => Event = evt;

			public IOutgoingEvent Event { get; }

			public CancellationToken CancellationToken => _cancellationTokenSource.Token;

			public void Cancel() => _cancellationTokenSource.Cancel();

			public void Dispose() => _cancellationTokenSource.Dispose();
		}
	}
}