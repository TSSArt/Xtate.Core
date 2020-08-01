#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ForEachNode : ExecutableEntityNode, IForEach, IAncestorProvider, IDebugEntityId
	{
		private readonly ForEachEntity _entity;

		public ForEachNode(in DocumentIdRecord documentIdNode, in ForEachEntity entity) : base(documentIdNode, (IForEach?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IForEach

		public IValueExpression? Array => _entity.Array;

		public ILocationExpression? Item => _entity.Item;

		public ILocationExpression? Index => _entity.Index;

		public ImmutableArray<IExecutableEntity> Action => _entity.Action;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ForEachNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Array, _entity.Array);
			bucket.AddEntity(Key.Item, _entity.Item);
			bucket.AddEntity(Key.Index, _entity.Index);
			bucket.AddEntityList(Key.Action, Action);
		}
	}
}