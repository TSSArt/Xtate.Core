#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate
{
	public sealed partial class StateMachineHost : IIoProcessor, IEventConsumer
	{
		private static readonly Uri BaseUri            = new Uri("ioprocessor:///");
		private static readonly Uri IoProcessorId      = new Uri("http://www.w3.org/TR/scxml/#SCXMLEventProcessor");
		private static readonly Uri IoProcessorAliasId = new Uri(uriString: "scxml", UriKind.Relative);

	#region Interface IEventConsumer

		public ValueTask Dispatch(SessionId sessionId, IEvent evt, CancellationToken token = default)
		{
			GetCurrentContext().ValidateSessionId(sessionId, out var controller);

			return controller.Send(evt, token);
		}

	#endregion

	#region Interface IIoProcessor

		Uri IIoProcessor.GetTarget(SessionId sessionId) => GetTarget(sessionId);

		ValueTask IIoProcessor.Dispatch(SessionId sessionId, IOutgoingEvent evt, CancellationToken token)
		{
			if (evt.Target == null)
			{
				throw new ProcessorException(Resources.Exception_Event_Target_did_not_specified);
			}

			var service = GetCurrentContext().GetService(sessionId, new Uri(evt.Target.Fragment));

			var serviceEvent = new EventObject(EventType.External, evt, GetTarget(sessionId), IoProcessorId);

			return service.Send(serviceEvent, token);
		}

		bool IIoProcessor.CanHandle(Uri? type, Uri? target) => CanHandleType(type) && CanHandleTarget(target);

		Uri IIoProcessor.Id => IoProcessorId;

	#endregion

		private static bool CanHandleType(Uri? type) => type == null || FullUriComparer.Instance.Equals(type, IoProcessorId) || FullUriComparer.Instance.Equals(type, IoProcessorAliasId);

		private static bool CanHandleTarget(Uri? target)
		{
			if (target == null)
			{
				return true;
			}

			if (target.IsAbsoluteUri && target.IsLoopback && target.GetComponents(UriComponents.Path, UriFormat.Unescaped).Length == 0)
			{
				return true;
			}

			return !target.IsAbsoluteUri;
		}

		private static Uri GetTarget(SessionId sessionId) => new Uri(BaseUri, "#_scxml_" + sessionId.Value);
	}
}