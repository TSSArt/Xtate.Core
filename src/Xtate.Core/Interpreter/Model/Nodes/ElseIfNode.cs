#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ElseIfNode : IElseIf, IAncestorProvider, IStoreSupport, IDocumentId, IDebugEntityId
	{
		private readonly ElseIfEntity     _entity;
		private          DocumentIdRecord _documentIdNode;

		public ElseIfNode(in DocumentIdRecord documentIdNode, in ElseIfEntity entity)
		{
			Infrastructure.Assert(entity.Condition != null);

			_documentIdNode = documentIdNode;
			_entity = entity;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IElseIf

		public IConditionExpression Condition => _entity.Condition!;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ElseIfNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Condition, Condition);
		}

	#endregion
	}
}