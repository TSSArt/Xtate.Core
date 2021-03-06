﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate.Core
{
	public sealed partial class StateMachineInterpreter : IExternalCommunication
	{
	#region Interface IExternalCommunication

		ImmutableArray<IIoProcessor> IExternalCommunication.GetIoProcessors() => _options.ExternalCommunication?.GetIoProcessors() ?? default;

		async ValueTask IExternalCommunication.StartInvoke(InvokeData invokeData, CancellationToken token)
		{
			try
			{
				if (_options.ExternalCommunication is not { } externalCommunication)
				{
					throw NoExternalCommunication();
				}

				await externalCommunication.StartInvoke(invokeData, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, _sessionId);
			}
		}

		async ValueTask IExternalCommunication.CancelInvoke(InvokeId invokeId, CancellationToken token)
		{
			try
			{
				if (_options.ExternalCommunication is not { } externalCommunication)
				{
					throw NoExternalCommunication();
				}

				await externalCommunication.CancelInvoke(invokeId, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, _sessionId);
			}
		}

		async ValueTask<SendStatus> IExternalCommunication.TrySendEvent(IOutgoingEvent outgoingEvent, CancellationToken token)
		{
			if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

			try
			{
				if (_options.ExternalCommunication is not { } externalCommunication)
				{
					throw NoExternalCommunication();
				}

				return await externalCommunication.TrySendEvent(outgoingEvent, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, _sessionId, outgoingEvent.SendId);
			}
		}

		ValueTask IExternalCommunication.ForwardEvent(IEvent evt, InvokeId invokeId, CancellationToken token) => ForwardEvent(evt, invokeId, token);

		async ValueTask IExternalCommunication.CancelEvent(SendId sendId, CancellationToken token)
		{
			try
			{
				if (_options.ExternalCommunication is not { } externalCommunication)
				{
					throw NoExternalCommunication();
				}

				await externalCommunication.CancelEvent(sendId, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, _sessionId, sendId);
			}
		}

	#endregion

		private bool IsCommunicationError(Exception exception, out SendId? sendId)
		{
			for (var ex = exception; ex is not null; ex = ex.InnerException)
			{
				if (ex is CommunicationException communicationException && communicationException.SessionId == _sessionId)
				{
					sendId = communicationException.SendId;

					return true;
				}
			}

			sendId = default;

			return false;
		}

		private async ValueTask ForwardEvent(IEvent evt, InvokeId invokeId, CancellationToken token)
		{
			try
			{
				if (_options.ExternalCommunication is not { } externalCommunication)
				{
					throw NoExternalCommunication();
				}

				await externalCommunication.ForwardEvent(evt, invokeId, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, _sessionId);
			}
		}

		private static NotSupportedException NoExternalCommunication() => new(Resources.Exception_ExternalCommunicationDoesNotConfiguredForStateMachineInterpreter);
	}
}