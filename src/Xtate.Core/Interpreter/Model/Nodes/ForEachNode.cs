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
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class ForEachNode : ExecutableEntityNode, IForEach, IAncestorProvider, IDebugEntityId
	{
		private readonly IForEach _forEach;

		public ForEachNode(DocumentIdNode documentIdNode, IForEach forEach) : base(documentIdNode, forEach) => _forEach = forEach;

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _forEach;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IForEach

		public IValueExpression? Array => _forEach.Array;

		public ILocationExpression? Item => _forEach.Item;

		public ILocationExpression? Index => _forEach.Index;

		public ImmutableArray<IExecutableEntity> Action => _forEach.Action;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ForEachNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Array, _forEach.Array);
			bucket.AddEntity(Key.Item, _forEach.Item);
			bucket.AddEntity(Key.Index, _forEach.Index);
			bucket.AddEntityList(Key.Action, Action);
		}
	}
}