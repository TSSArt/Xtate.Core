// Copyright © 2019-2023 Sergii Artemenko
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

public sealed class IdentifierNode(IIdentifier id) : IIdentifier, IStoreSupport, IAncestorProvider, IDebugEntityId
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => id;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{id}";

#endregion

#region Interface IIdentifier

	public string Value => id.Value;

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.IdentifierNode);
		bucket.Add(Key.Id, id.Value);
	}

#endregion

	public override string ToString() => id.ToString();

	public override bool Equals(object? obj) => id.Equals(obj);

	public override int GetHashCode() => id.GetHashCode();
}