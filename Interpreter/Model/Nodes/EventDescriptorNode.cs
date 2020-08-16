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
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class EventDescriptorNode : IEventDescriptor, IStoreSupport, IAncestorProvider, IDebugEntityId
	{
		private readonly IEventDescriptor _eventDescriptor;

		public EventDescriptorNode(IEventDescriptor eventDescriptor) => _eventDescriptor = eventDescriptor;

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _eventDescriptor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{_eventDescriptor}";

	#endregion

	#region Interface IEventDescriptor

		public bool IsEventMatch(IEvent evt) => _eventDescriptor.IsEventMatch(evt);

		public string Value => _eventDescriptor.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.EventDescriptorNode);
			bucket.Add(Key.Id, _eventDescriptor.Value);
		}

	#endregion
	}
}