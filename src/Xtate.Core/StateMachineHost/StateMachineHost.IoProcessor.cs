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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.IoProcessor;
using Xtate.Service;

namespace Xtate
{
	public sealed partial class StateMachineHost : IIoProcessor, IEventConsumer
	{
		private const string ParentTarget    = "#_parent";
		private const string SessionIdPrefix = "#_scxml_";
		private const string InvokeIdPrefix  = "#_";

		private static readonly Uri BaseUri            = new(@"ioprocessor:///");
		private static readonly Uri IoProcessorId      = new(@"http://www.w3.org/TR/scxml/#SCXMLEventProcessor");
		private static readonly Uri IoProcessorAliasId = new(uriString: @"scxml", UriKind.Relative);

	#region Interface IEventConsumer

		public async ValueTask<IEventDispatcher?> TryGetEventDispatcher(ServiceId serviceId, CancellationToken token) =>
				serviceId switch
				{
						SessionId sessionId => await GetCurrentContext().FindStateMachineController(sessionId, token).ConfigureAwait(false),
						InvokeId invokeId when GetCurrentContext().TryGetService(invokeId, out var service) => service,
						_ => default
				};

	#endregion

	#region Interface IIoProcessor

		Uri? IIoProcessor.GetTarget(ServiceId serviceId) => GetTarget(serviceId);

		ValueTask<IHostEvent> IIoProcessor.GetHostEvent(ServiceId senderServiceId, IOutgoingEvent outgoingEvent, CancellationToken token)
		{
			if (senderServiceId is null) throw new ArgumentNullException(nameof(senderServiceId));

			var target = outgoingEvent.Target;

			if (target is null)
			{
				throw new ProcessorException(Resources.Exception_EventTargetDidNotSpecified);
			}

			if (senderServiceId is SessionId sessionId && IsTargetParent(target))
			{
				if (GetCurrentContext().TryGetParentSessionId(sessionId, out var parentSessionId))
				{
					return new ValueTask<IHostEvent>(new HostEvent(this, senderServiceId, parentSessionId, outgoingEvent));
				}
			}
			else if (IsTargetSessionId(target, out var targetSessionId))
			{
				return new ValueTask<IHostEvent>(new HostEvent(this, senderServiceId, targetSessionId, outgoingEvent));
			}
			else if (IsTargetInvokeId(target, out var targetInvokeId))
			{
				return new ValueTask<IHostEvent>(new HostEvent(this, senderServiceId, targetInvokeId, outgoingEvent));
			}

			throw new ProcessorException(Resources.Exception_CannotFindTarget);
		}

		async ValueTask IIoProcessor.Dispatch(IHostEvent hostEvent, CancellationToken token)
		{
			Infrastructure.NotNull(hostEvent.TargetServiceId);

			var service = await GetService(hostEvent.TargetServiceId, token).ConfigureAwait(false);
			await service.Send(hostEvent, token).ConfigureAwait(false);
		}

		bool IIoProcessor.CanHandle(Uri? type) => CanHandleType(type);

		Uri IIoProcessor.Id => IoProcessorId;

	#endregion

		private async ValueTask<IService> GetService(ServiceId serviceId, CancellationToken token) =>
				serviceId switch
				{
						SessionId sessionId
								when await GetCurrentContext().FindStateMachineController(sessionId, token).ConfigureAwait(false) is { } controller => controller,
						InvokeId invokeId
								when GetCurrentContext().TryGetService(invokeId, out var service) => service,
						_ => throw new ProcessorException(Resources.Exception_CannotFindTarget)
				};

		private static bool CanHandleType(Uri? type) => type is null || FullUriComparer.Instance.Equals(type, IoProcessorId) || FullUriComparer.Instance.Equals(type, IoProcessorAliasId);

		private static Uri? GetTarget(ServiceId serviceId) =>
				serviceId switch
				{
						SessionId sessionId => new Uri(BaseUri, SessionIdPrefix + sessionId.Value),
						InvokeId invokeId => new Uri(BaseUri, InvokeIdPrefix + invokeId.Value),
						_ => default
				};

		private static string GetTargetString(Uri target) => target.IsAbsoluteUri ? target.Fragment : target.OriginalString;

		private static bool IsTargetParent(Uri target) => GetTargetString(target) == ParentTarget;

		private static bool IsTargetSessionId(Uri target, [NotNullWhen(true)] out SessionId? sessionId)
		{
			var val = GetTargetString(target);

			if (val.StartsWith(SessionIdPrefix, StringComparison.Ordinal))
			{
				sessionId = SessionId.FromString(val[SessionIdPrefix.Length..]);

				return true;
			}

			sessionId = default;

			return false;
		}

		private static bool IsTargetInvokeId(Uri target, [NotNullWhen(true)] out InvokeId? invokeId)
		{
			var val = GetTargetString(target);

			if (val.StartsWith(InvokeIdPrefix, StringComparison.Ordinal))
			{
				invokeId = InvokeId.FromString(val[InvokeIdPrefix.Length..]);

				return true;
			}

			invokeId = default;

			return false;
		}
	}
}