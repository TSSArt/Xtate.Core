﻿// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.DataModel;
using Xtate.ExternalService;
using Xtate.IoProcessor;

namespace Xtate;

public sealed partial class StateMachineHost : IStateMachineHost
{
	private ImmutableArray<IEventRouter> _ioProcessors;

	public required DataConverter _dataConverter { private get; [UsedImplicitly] init; }

	//private ImmutableArray<IServiceFactory> _serviceFactories;
	public required IAsyncEnumerable<IIoProcessorFactory> _ioProcessorFactories { private get; [UsedImplicitly] init; }

	public required IAsyncEnumerable<IExternalServiceProvider> ServiceFactories { private get; [UsedImplicitly] init; }

#region Interface IHostEventDispatcher

	public async ValueTask DispatchEvent(IRouterEvent routerEvent, CancellationToken token)
	{
		if (routerEvent is null) throw new ArgumentNullException(nameof(routerEvent));

		if (routerEvent.OriginType is not { } originType)
		{
			throw new PlatformException(Resources.Exception_OriginTypeMustBeProvidedInRouterEvent);
		}

		var ioProcessor = GetIoProcessorById(originType);

		await ioProcessor.Dispatch(routerEvent, token).ConfigureAwait(false);
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
			if (outgoingEvent.Target == Const.ScxmlIoProcessorInternalTarget)
			{
				return SendStatus.ToInternalQueue;
			}
		}

		var routerEvent = await ioProcessor.GetRouterEvent(outgoingEvent, token).ConfigureAwait(false);

		if (outgoingEvent.DelayMs > 0)
		{
			await context.ScheduleEvent(routerEvent, token).ConfigureAwait(false);

			return SendStatus.Scheduled;
		}

		await ioProcessor.Dispatch(routerEvent, token).ConfigureAwait(false);

		return SendStatus.Sent;
	}

	ImmutableArray<IEventRouter> IStateMachineHost.GetIoProcessors() => !_ioProcessors.IsDefault ? _ioProcessors : [];

	async ValueTask IStateMachineHost.StartInvoke(SessionId sessionId,
												  Uri? location,
												  InvokeData data,

												  // SecurityContext securityContext,
												  CancellationToken token)
	{
		InvokeId invokeId = default;

		var context = GetCurrentContext();

		context.ValidateSessionId(sessionId, out var service);

		await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);

		{
			//var loggerContext = new StartInvokeLoggerContext(sessionId, data.Type, data.Source);

			var activator = await FindServiceFactoryActivator(data.Type).ConfigureAwait(false);
			var serviceCommunication = new ServiceCommunication(this, GetTarget(sessionId), Const.ScxmlIoProcessorId, invokeId);
			IExternalService invokedService = null; //await activator.StartService(location, data, serviceCommunication).ConfigureAwait(false);

			await context.AddService(sessionId, invokeId, invokedService, token).ConfigureAwait(false);

			//CompleteAsync(context, invokedService, service, sessionId, invokeId, _dataConverter).Forget();
		}

		async ValueTask CompleteAsync(StateMachineHostContext context,
											 IExternalService invokedService,
											 IEventDispatcher service,
											 SessionId sessionId,
											 InvokeId invokeId,
											 DataConverter dataConverter)
		{
			{
				try
				{
					var result = await invokedService.GetResult().ConfigureAwait(false);

					var name = EventName.GetDoneInvokeName(invokeId);
					var incomingEvent = new IncomingEvent { Type = EventType.External, Name = name, Data = result, InvokeId = invokeId };
					await service.Dispatch(incomingEvent, token).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					var incomingEvent = new IncomingEvent
										{
											Type = EventType.External,
											Name = EventName.ErrorExecution,
											Data = dataConverter.FromException(ex),
											InvokeId = invokeId
										};
					await service.Dispatch(incomingEvent, token).ConfigureAwait(false);
				}
				finally
				{
					if (await context.TryCompleteService(invokeId).ConfigureAwait(false) is { } invokedService2)
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

		//context.ValidateSessionId(sessionId, out _);

		if (await context.TryRemoveService(invokeId).ConfigureAwait(false) is { } service)
		{
			//await service.Destroy().ConfigureAwait(false);

			await DisposeInvokedService(service).ConfigureAwait(false);
		}
	}

	ValueTask IStateMachineHost.CancelEvent(SessionId sessionId, SendId sendId, CancellationToken token)
	{
		var context = GetCurrentContext();

		//		context.ValidateSessionId(sessionId, out _);

		return context.CancelEvent(sessionId, sendId, token);
	}

	ValueTask IStateMachineHost.ForwardEvent(SessionId sessionId,
											 InvokeId invokeId,
											 IIncomingEvent incomingEvent,
											 CancellationToken token)
	{
		var context = GetCurrentContext();

		return default; //TODO:

		//context.ValidateSessionId(sessionId, out _);

		if (!context.TryGetService(invokeId, out var service))
		{
			throw new ProcessorException(Resources.Exception_InvalidInvokeId);
		}

		//return service?.Dispatch(incomingEvent) ?? default;
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
			ValidationMode.Default => new DefaultErrorProcessor(),
			ValidationMode.Verbose => new DetailedErrorProcessor(sessionId, origin),
			_                      => throw Infra.Unmatched(_options.ValidationMode)
		};

	private StateMachineHostContext GetCurrentContext() => _context ?? throw new InvalidOperationException(Resources.Exception_IOProcessorHasNotBeenStarted);

	private async ValueTask<IExternalServiceActivator> FindServiceFactoryActivator(FullUri type)
	{
		await foreach (var serviceFactory in ServiceFactories.ConfigureAwait(false))
		{
			if (serviceFactory.TryGetActivator(type) is { } activator)
			{
				return activator;
			}
		}

		throw new ProcessorException(Res.Format(Resources.Exception_InvalidType, type));
	}
	/*
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
	}*/

	private async ValueTask StateMachineHostStartAsync()
	{
		//var factories = _options.IoProcessorFactories;
		var factories = await _ioProcessorFactories.ToImmutableArrayAsync().ConfigureAwait(false);
		var length = !factories.IsDefault ? factories.Length + 1 : 1;

		var ioProcessors = ImmutableArray.CreateBuilder<IEventRouter>(length);

		ioProcessors.Add(this);

		if (!factories.IsDefaultOrEmpty)
		{
			foreach (var ioProcessorFactory in factories)
			{
				ioProcessors.Add(await ioProcessorFactory.Create(this, token: default /*??*/).ConfigureAwait(false));
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

	private static ValueTask DisposeInvokedService(IExternalService externalService)
	{
		if (externalService is IAsyncDisposable asyncDisposable)
		{
			return asyncDisposable.DisposeAsync();
		}

		if (externalService is IDisposable disposable)
		{
			disposable.Dispose();
		}

		return default;
	}

	private IEventRouter GetIoProcessorByType(FullUri? type)
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

		throw new ProcessorException(Res.Format(Resources.Exception_InvalidType, type));
	}

	private IEventRouter GetIoProcessorById(FullUri ioProcessorsId)
	{
		var ioProcessors = _ioProcessors;

		if (ioProcessors.IsDefault)
		{
			throw new ProcessorException(Resources.Exception_StateMachineHostStopped);
		}

		foreach (var ioProcessor in ioProcessors)
		{
			//if (ioProcessor.Id == ioProcessorsId)//TODO:
			{
				return ioProcessor;
			}
		}

		throw new ProcessorException(Res.Format(Resources.Exception_InvalidType, ioProcessorsId));
	}

	private class StartInvokeLoggerContext(SessionId sessionId, Uri type, Uri? source) : IStartInvokeLoggerContext
	{
		public string LoggerContextType => nameof(IStartInvokeLoggerContext);

	#region Interface IStartInvokeLoggerContext

		public SessionId SessionId { get; } = sessionId;

		public Uri Type { get; } = type;

		public Uri? Source { get; } = source;

	#endregion

		public DataModelList GetProperties()
		{
			var properties = new DataModelList
							 {
								 { @"SessionId", SessionId },
								 { @"InvokeType", Type.ToString() }
							 };

			if (Source is { } source)
			{
				properties.Add(key: @"Source", source.ToString());
			}

			properties.MakeDeepConstant();

			return properties;
		}
	}
}