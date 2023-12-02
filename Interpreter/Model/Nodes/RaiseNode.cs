#region Copyright © 2019-2023 Sergii Artemenko

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

using Xtate.Persistence;

namespace Xtate.Core;

public sealed class RaiseNode : ExecutableEntityNode, IRaise, IAncestorProvider, IDebugEntityId
{
	private readonly IRaise _raise;

	public RaiseNode(DocumentIdNode documentIdNode, IRaise raise) : base(documentIdNode, raise) => _raise = raise;

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _raise;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

#endregion

#region Interface IRaise

	public IOutgoingEvent? OutgoingEvent => _raise.OutgoingEvent;

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.RaiseNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntity(Key.Event, OutgoingEvent);
	}
}