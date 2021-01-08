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

namespace Xtate.Core
{
	internal sealed class AssignNode : ExecutableEntityNode, IAssign, IAncestorProvider, IDebugEntityId
	{
		private readonly AssignEntity _entity;

		public AssignNode(in DocumentIdRecord documentIdNode, in AssignEntity entity) : base(documentIdNode, (IAssign?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IAssign

		public ILocationExpression? Location => _entity.Location;

		public IValueExpression? Expression => _entity.Expression;

		public IInlineContent? InlineContent => _entity.InlineContent;

		public string? Type => _entity.Type;

		public string? Attribute => _entity.Attribute;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.AssignNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Location, Location);
			bucket.AddEntity(Key.Expression, Expression);
			bucket.Add(Key.InlineContent, InlineContent?.Value);
			bucket.Add(Key.Type, Type);
			bucket.Add(Key.Attribute, Attribute);
		}
	}
}