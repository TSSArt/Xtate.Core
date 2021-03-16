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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.DataModel;
using Xtate.IoProcessor;
using Xtate.Service;

namespace Xtate
{
	public sealed partial class StateMachineHost : IStateMachineHost
	{
		private static readonly Uri InternalTarget = new(uriString: @"#_internal", UriKind.Relative);

		private ImmutableArray<IIoProcessor>    _ioProcessors;
		private ImmutableArray<IServiceFactory> _serviceFactories;

	#region Interface IHostEventDispatcher

		public async ValueTask DispatchEvent(IHostEvent hostEvent, CancellationToken token)
		{
			if (hostEvent is null) throw new ArgumentNullException(nameof(hostEvent));

			if (hostEvent.OriginType is not { } originType)
			{
				throw new PlatformException(Resources.Exception_OriginTypeMustBeProvidedInIoProcessorEvent);
			}

			var ioProcessor = GetIoProcessorById(originType);

			await ioProcessor.Dispatch(hostEvent, token).ConfigureAwait(false);
		}

	#endregion

	#region Interface IStateMachineHost

		async ValueTask<SendStatus> IStateMachineHost.DispatchEvent(ServiceId senderServiceId, IOutgoingEvent outgoingEvent, CancellationToken token)
		{
			if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

			var context = GetCurrentContext();

			var ioProcessor = GetIoProcessorByType(outgoingEvent.Type);

			if (ioProcessor == this)
			{
				if (outgoingEvent.Target == InternalTarget)
				{
					if (outgoingEvent.DelayMs != 0)
					{
						throw new ProcessorException(Resources.Exception_InternalEventsCantBeDelayed);
					}

					return SendStatus.ToInternalQueue;
				}
			}

			var ioProcessorEvent = await ioProcessor.GetHostEvent(senderServiceId, outgoingEvent, token).ConfigureAwait(false);

			if (outgoingEvent.DelayMs > 0)
			{
				await context.ScheduleEvent(ioProcessorEvent, token).ConfigureAwait(false);

				return SendStatus.Scheduled;
			}

			await ioProcessor.Dispatch(ioProcessorEvent, token).ConfigureAwait(false);

			return SendStatus.Sent;
		}

		ImmutableArray<IIoProcessor> IStateMachineHost.GetIoProcessors() => !_ioProcessors.IsDefault ? _ioProcessors : ImmutableArray<IIoProcessor>.Empty;

		async ValueTask IStateMachineHost.StartInvoke(SessionId sessionId, InvokeData data, ISecurityContext securityContext, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out var service);

			var finalizer = new DeferredFinalizer();
			await using (finalizer.ConfigureAwait(false))
			{
				securityContext = securityContext.CreateNested(SecurityContextType.InvokedService, finalizer);
				var factoryContext = new FactoryContext(_options.ResourceLoaderFactories, securityContext);
				var activator = await FindServiceFactoryActivator(factoryContext, data.Type, token).ConfigureAwait(false);
				var serviceCommunication = new ServiceCommunication(this, GetTarget(sessionId), IoProcessorId, data.InvokeId);
				var invokedService = await activator.StartService(factoryContext, service.StateMachineLocation, data, serviceCommunication, token).ConfigureAwait(false);

				await context.AddService(sessionId, data.InvokeId, invokedService, token).ConfigureAwait(false);

				CompleteAsync(context, invokedService, service, sessionId, data.InvokeId, finalizer).Forget();
			}

			static async ValueTask CompleteAsync(StateMachineHostContext context, IService invokedService, IEventDispatcher service,
												 SessionId sessionId, InvokeId invokeId, DeferredFinalizer finalizer)
			{
				await using (finalizer.ConfigureAwait(false))
				{
					finalizer.DefferFinalization();

					try
					{
						var result = await invokedService.GetResult(default).ConfigureAwait(false);

						var nameParts = EventName.GetDoneInvokeNameParts(invokeId);
						var evt = new EventObject { Type = EventType.External, NameParts = nameParts, Data = result, InvokeId = invokeId };
						await service.Send(evt, token: default).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						var evt = new EventObject
								  {
										  Type = EventType.External,
										  NameParts = EventName.ErrorExecution,
										  Data = DataConverter.FromException(ex, caseInsensitive: false),
										  InvokeId = invokeId
								  };
						await service.Send(evt, token: default).ConfigureAwait(false);
					}
					finally
					{
						if (await context.TryCompleteService(sessionId, invokeId).ConfigureAwait(false) is { } invokedService2)
						{
							await DisposeInvokedService(invokedService2).ConfigureAwait(false);
						}
					}
				}
			}
		}

		async ValueTask IStateMachineHost.CancelInvoke(SessionId sessionId, InvokeId invokeId, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out _);

			if (await context.TryRemoveService(sessionId, invokeId).ConfigureAwait(false) is { } service)
			{
				await service.Destroy(token).ConfigureAwait(false);

				await DisposeInvokedService(service).ConfigureAwait(false);
			}
		}

		ValueTask IStateMachineHost.CancelEvent(SessionId sessionId, SendId sendId, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out _);

			return context.CancelEvent(sessionId, sendId, token);
		}

		ValueTask IStateMachineHost.ForwardEvent(SessionId sessionId, IEvent evt, InvokeId invokeId, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out _);

			if (!context.TryGetService(invokeId, out var service))
			{
				throw new ProcessorException(Resources.Exception_InvalidInvokeId);
			}

			return service.Send(evt, token);
		}

	#endregion

		private bool IsCurrentContextExists([NotNullWhen(true)] out StateMachineHostContext? context)
		{
			context = _context;

			return context is not null;
		}

		private IErrorProcessor CreateErrorProcessor(SessionId sessionId, StateMachineOrigin origin) =>
				_options.ValidationMode switch
				{
						ValidationMode.Default => DefaultErrorProcessor.Instance,
						ValidationMode.Verbose => new DetailedErrorProcessor(sessionId, origin),
						_ => Infrastructure.UnexpectedValue<IErrorProcessor>(_options.ValidationMode)
				};

		private StateMachineHostContext GetCurrentContext() => _context ?? throw new InvalidOperationException(Resources.Exception_IOProcessorHasNotBeenStarted);

		private async ValueTask<IServiceFactoryActivator> FindServiceFactoryActivator(IFactoryContext factoryContext, Uri type, CancellationToken token)
		{
			if (!_serviceFactories.IsDefaultOrEmpty)
			{
				foreach (var serviceFactory in _serviceFactories)
				{
					var activator = await serviceFactory.TryGetActivator(factoryContext, type, token).ConfigureAwait(false);

					if (activator is not null)
					{
						return activator;
					}
				}
			}

			throw new ProcessorException(Resources.Exception_InvalidType);
		}

		private void StateMachineHostInit()
		{
			var factories = _options.ServiceFactories;
			var length = !factories.IsDefault ? factories.Length + 1 : 1;
			var serviceFactories = ImmutableArray.CreateBuilder<IServiceFactory>(length);

			serviceFactories.Add(this);

			if (!factories.IsDefaultOrEmpty)
			{
				foreach (var serviceFactory in factories)
				{
					serviceFactories.Add(serviceFactory);
				}
			}

			_serviceFactories = serviceFactories.MoveToImmutable();
		}

		private async ValueTask StateMachineHostStartAsync(CancellationToken token)
		{
			var factories = _options.IoProcessorFactories;
			var length = !factories.IsDefault ? factories.Length + 1 : 1;

			var ioProcessors = ImmutableArray.CreateBuilder<IIoProcessor>(length);

			ioProcessors.Add(this);

			if (!_options.IoProcessorFactories.IsDefaultOrEmpty)
			{
				foreach (var ioProcessorFactory in _options.IoProcessorFactories)
				{
					ioProcessors.Add(await ioProcessorFactory.Create(this, token).ConfigureAwait(false));
				}
			}

			_ioProcessors = ioProcessors.MoveToImmutable();
		}

		private async ValueTask StateMachineHostStopAsync()
		{
			var ioProcessors = _ioProcessors;
			_ioProcessors = default;

			if (ioProcessors.IsDefaultOrEmpty)
			{
				return;
			}

			foreach (var ioProcessor in ioProcessors)
			{
				if (ioProcessor == this)
				{
					continue;
				}

				if (ioProcessor is IAsyncDisposable asyncDisposable)
				{
					await asyncDisposable.DisposeAsync().ConfigureAwait(false);
				}

				else if (ioProcessor is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}

		private static ValueTask DisposeInvokedService(IService service)
		{
			if (service is IAsyncDisposable asyncDisposable)
			{
				return asyncDisposable.DisposeAsync();
			}

			if (service is IDisposable disposable)
			{
				disposable.Dispose();
			}

			return default;
		}

		private IIoProcessor GetIoProcessorByType(Uri? type)
		{
			var ioProcessors = _ioProcessors;

			if (ioProcessors.IsDefault)
			{
				throw new ProcessorException(Resources.Exception_StateMachineHostStopped);
			}

			foreach (var ioProcessor in ioProcessors)
			{
				if (ioProcessor.CanHandle(type))
				{
					return ioProcessor;
				}
			}

			throw new ProcessorException(Resources.Exception_InvalidType);
		}

		private IIoProcessor GetIoProcessorById(Uri ioProcessorsId)
		{
			var ioProcessors = _ioProcessors;

			if (ioProcessors.IsDefault)
			{
				throw new ProcessorException(Resources.Exception_StateMachineHostStopped);
			}

			foreach (var ioProcessor in ioProcessors)
			{
				if (FullUriComparer.Instance.Equals(ioProcessor.Id, ioProcessorsId))
				{
					return ioProcessor;
				}
			}

			throw new ProcessorException(Resources.Exception_InvalidType);
		}
	}
}