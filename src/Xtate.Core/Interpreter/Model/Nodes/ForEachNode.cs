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

public sealed class ForEachNode(DocumentIdNode documentIdNode, IForEach forEach) : ExecutableEntityNode(documentIdNode, forEach), IForEach, IAncestorProvider, IDebugEntityId
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => forEach;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

#endregion

#region Interface IForEach

	public IValueExpression? Array => forEach.Array;

	public ILocationExpression? Item => forEach.Item;

	public ILocationExpression? Index => forEach.Index;

	public ImmutableArray<IExecutableEntity> Action => forEach.Action;

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.ForEachNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntity(Key.Array, forEach.Array);
		bucket.AddEntity(Key.Item, forEach.Item);
		bucket.AddEntity(Key.Index, forEach.Index);
		bucket.AddEntityList(Key.Action, Action);
	}
}