// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.Persistence;

namespace Xtate.Core;

public sealed class EventDescriptorNode(IEventDescriptor eventDescriptor) : IEventDescriptor, IStoreSupport, IAncestorProvider, IDebugEntityId
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => eventDescriptor;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{eventDescriptor}";

#endregion

#region Interface IEventDescriptor

	public bool IsEventMatch(IEvent evt) => eventDescriptor.IsEventMatch(evt);

	public string Value => eventDescriptor.Value;

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.EventDescriptorNode);
		bucket.Add(Key.Id, eventDescriptor.Value);
	}

#endregion

	public override bool Equals(object? obj) => eventDescriptor.Equals(obj);

	public override int GetHashCode() => eventDescriptor.GetHashCode();

	public override string ToString() => eventDescriptor.ToString() ?? string.Empty;
}