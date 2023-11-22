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
	public sealed class IfNode : ExecutableEntityNode, IIf, IAncestorProvider, IDebugEntityId
	{
		private readonly IIf _if;

		public IfNode(DocumentIdNode documentIdNode, IIf @if) : base(documentIdNode, @if) => _if = @if;

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _if;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IIf

		public IConditionExpression? Condition => _if.Condition;

		public ImmutableArray<IExecutableEntity> Action => _if.Action;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.IfNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Condition, Condition);
			bucket.AddEntityList(Key.Action, Action);
		}
	}
}