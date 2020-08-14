#region Copyright © 2019-2020 Sergii Artemenko

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

namespace Xtate
{
	internal class ServiceCommunication : IServiceCommunication
	{
		private readonly StateMachineController _creator;
		private readonly InvokeId               _invokeId;
		private readonly Uri                    _originType;
		private          Uri?                   _origin;

		public ServiceCommunication(StateMachineController creator, Uri originType, InvokeId invokeId)
		{
			_creator = creator;
			_originType = originType;
			_invokeId = invokeId;
		}

	#region Interface IServiceCommunication

		public ValueTask SendToCreator(IOutgoingEvent evt, CancellationToken token)
		{
			if (evt.Type is { } || evt.SendId is { } || evt.DelayMs != 0)
			{
				throw new ProcessorException(Resources.Exception_Type__SendId__DelayMs_can_t_be_specified_for_this_event);
			}

			if (evt.Target != EventEntity.ParentTarget && evt.Target is { })
			{
				throw new ProcessorException(Resources.Exception_Target_should_be_equal_to___parent__or_null);
			}

			_origin ??= new Uri("#_" + _invokeId.Value);

			var eventObject = new EventObject(EventType.External, evt, _origin, _originType, _invokeId);

			return _creator.Send(eventObject, token);
		}

	#endregion
	}
}