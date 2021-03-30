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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Service;

namespace Xtate.Core
{
	internal class ServiceCommunication : IServiceCommunication
	{
		private readonly IStateMachineHost _host;
		private readonly InvokeId          _invokeId;
		private readonly Uri?              _target;
		private readonly Uri               _type;

		public ServiceCommunication(IStateMachineHost host,
									Uri? target,
									Uri type,
									InvokeId invokeId)
		{
			_host = host;
			_target = target;
			_type = type;
			_invokeId = invokeId;
		}

	#region Interface IServiceCommunication

		public async ValueTask SendToCreator(IOutgoingEvent outgoingEvent, CancellationToken token)
		{
			if (outgoingEvent.Type is not null || outgoingEvent.SendId is not null || outgoingEvent.DelayMs != 0)
			{
				throw new ProcessorException(Resources.Exception_TypeSendIdDelayMsCantBeSpecifiedForThisEvent);
			}

			if (outgoingEvent.Target != EventEntity.ParentTarget && outgoingEvent.Target is not null)
			{
				throw new ProcessorException(Resources.Exception_TargetShouldBeEqualToParentOrNull);
			}

			var newOutgoingEvent = new EventEntity
								   {
									   NameParts = outgoingEvent.NameParts,
									   Data = outgoingEvent.Data,
									   Type = _type,
									   Target = _target
								   };

			await _host.DispatchEvent(_invokeId, newOutgoingEvent, token).ConfigureAwait(false);
		}

	#endregion
	}
}