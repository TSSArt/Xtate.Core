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
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate.Core
{
	public partial class StateMachineInterpreter : IExternalCommunication2
	{
	#region Interface IExternalCommunication

		async ValueTask IExternalCommunication2.StartInvoke(InvokeData invokeData)
		{
			try
			{
				if (_externalCommunication is null)
				{
					throw NoExternalCommunication();
				}

				await _externalCommunication.StartInvoke(invokeData).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex) { Token = _stateMachineToken };
			}
		}

		async ValueTask IExternalCommunication2.CancelInvoke(InvokeId invokeId)
		{
			try
			{
				if (_externalCommunication is null)
				{
					throw NoExternalCommunication();
				}

				await _externalCommunication.CancelInvoke(invokeId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex) { Token = _stateMachineToken };
			}
		}

		async ValueTask<SendStatus> IExternalCommunication2.TrySendEvent(IOutgoingEvent outgoingEvent)
		{
			if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

			try
			{
				if (_externalCommunication is null)
				{
					throw NoExternalCommunication();
				}

				return await _externalCommunication.TrySendEvent(outgoingEvent).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, outgoingEvent.SendId) { Token = _stateMachineToken };
			}
		}

		async ValueTask IExternalCommunication2.CancelEvent(SendId sendId)
		{
			try
			{
				if (_externalCommunication is null)
				{
					throw NoExternalCommunication();
				}

				await _externalCommunication.CancelEvent(sendId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex, sendId) { Token = _stateMachineToken };
			}
		}

	#endregion

		private async ValueTask ForwardEvent(IEvent evt, InvokeId invokeId)
		{
			try
			{
				if (_externalCommunication is null)
				{
					throw NoExternalCommunication();
				}

				await _externalCommunication.ForwardEvent(evt, invokeId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommunicationException(ex) {Token = _stateMachineToken};
			}
		}

		private static NotSupportedException NoExternalCommunication() => new(Resources.Exception_ExternalCommunicationDoesNotConfiguredForStateMachineInterpreter);
	}
}