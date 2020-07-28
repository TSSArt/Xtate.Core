#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.IoProcessor;
using Xtate.Service;

namespace Xtate
{
	public sealed partial class StateMachineHost : IStateMachineHost
	{
		private static readonly Uri                          InternalTarget = new Uri(uriString: "#_internal", UriKind.Relative);
		private                 ImmutableArray<IIoProcessor> _ioProcessors;

		private ImmutableArray<IServiceFactory> _serviceFactories;

	#region Interface IStateMachineHost

		ImmutableArray<IIoProcessor> IStateMachineHost.GetIoProcessors() => _ioProcessors;

		async ValueTask IStateMachineHost.StartInvoke(SessionId sessionId, InvokeData data, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out var service);

			var factory = FindServiceFactory(data.Type, data.Source);
			var serviceCommunication = new ServiceCommunication(service, IoProcessorId, data.InvokeId);
			var invokedService = await factory.StartService(service.StateMachineLocation, data, serviceCommunication, token).ConfigureAwait(false);

			await context.AddService(sessionId, data.InvokeId, invokedService, token).ConfigureAwait(false);

			CompleteAsync(context, invokedService, service, sessionId, data.InvokeId).Forget();
		}

		async ValueTask IStateMachineHost.CancelInvoke(SessionId sessionId, InvokeId invokeId, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out _);

			var service = await context.TryRemoveService(sessionId, invokeId).ConfigureAwait(false);

			if (service != null)
			{
				await service.Destroy(token).ConfigureAwait(false);

				await DisposeInvokedService(service).ConfigureAwait(false);
			}
		}

		bool IStateMachineHost.IsInvokeActive(SessionId sessionId, InvokeId invokeId) => IsCurrentContextExists(out var context) && context.TryGetService(invokeId, out _);

		async ValueTask<SendStatus> IStateMachineHost.DispatchEvent(SessionId sessionId, IOutgoingEvent evt, bool skipDelay, CancellationToken token)
		{
			if (evt == null) throw new ArgumentNullException(nameof(evt));

			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out _);

			var ioProcessor = GetIoProcessor(evt.Type, evt.Target);

			if (ioProcessor == this)
			{
				if (evt.Target == InternalTarget)
				{
					if (evt.DelayMs != 0)
					{
						throw new ProcessorException(Resources.Exception_Internal_events_can_t_be_delayed_);
					}

					return SendStatus.ToInternalQueue;
				}
			}

			if (!skipDelay && evt.DelayMs != 0)
			{
				return SendStatus.ToSchedule;
			}

			await ioProcessor.Dispatch(sessionId, evt, token).ConfigureAwait(false);

			return SendStatus.Sent;
		}

		ValueTask IStateMachineHost.ForwardEvent(SessionId sessionId, IEvent evt, InvokeId invokeId, CancellationToken token)
		{
			var context = GetCurrentContext();

			context.ValidateSessionId(sessionId, out _);

			if (!context.TryGetService(invokeId, out var service))
			{
				throw new ProcessorException(Resources.Exception_Invalid_InvokeId);
			}

			return service?.Send(evt, token) ?? default;
		}

	#endregion

		private static async ValueTask CompleteAsync(StateMachineHostContext context, IService invokedService, StateMachineController service, SessionId sessionId, InvokeId invokeId)
		{
			try
			{
				var result = await invokedService.Result.ConfigureAwait(false);

				var nameParts = EventName.GetDoneInvokeNameParts(invokeId);
				var evt = new EventObject(EventType.External, nameParts, result, sendId: null, invokeId);
				await service.Send(evt, token: default).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var evt = new EventObject(EventType.External, EventName.ErrorExecution, DataConverter.FromException(ex, caseInsensitive: false), sendId: null, invokeId);
				await service.Send(evt, token: default).ConfigureAwait(false);
			}
			finally
			{
				var invokedService2 = await context.TryCompleteService(sessionId, invokeId).ConfigureAwait(false);

				if (invokedService2 != null)
				{
					await DisposeInvokedService(invokedService2).ConfigureAwait(false);
				}
			}
		}

		private bool IsCurrentContextExists([NotNullWhen(true)] out StateMachineHostContext? context)
		{
			context = _context;

			return context != null;
		}

		private IErrorProcessor CreateErrorProcessor(SessionId sessionId, StateMachineOrigin origin) =>
				_options.VerboseValidation ? new DetailedErrorProcessor(sessionId, origin) : DefaultErrorProcessor.Instance;

		private StateMachineHostContext GetCurrentContext() => _context ?? throw new InvalidOperationException(Resources.Exception_IO_Processor_has_not_been_started);

		private IServiceFactory FindServiceFactory(Uri type, Uri? source)
		{
			if (!_serviceFactories.IsDefaultOrEmpty)
			{
				foreach (var serviceFactory in _serviceFactories)
				{
					if (serviceFactory.CanHandle(type, source))
					{
						return serviceFactory;
					}
				}
			}

			throw new ProcessorException(Resources.Exception_Invalid_type);
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

		[SuppressMessage(category: "ReSharper", checkId: "SuspiciousTypeConversion.Global", Justification = "Disposing")]
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

		private IIoProcessor GetIoProcessor(Uri? type, Uri? target)
		{
			if (_ioProcessors == null)
			{
				throw new ProcessorException(Resources.Exception_StateMachineHost_stopped);
			}

			foreach (var ioProcessor in _ioProcessors)
			{
				if (ioProcessor.CanHandle(type, target))
				{
					return ioProcessor;
				}
			}

			throw new ProcessorException(Resources.Exception_Invalid_type);
		}
	}
}