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
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class ParamNode : IParam, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly ParamEntity      _param;
		private          DocumentIdRecord _documentIdNode;

		public ParamNode(in DocumentIdRecord documentIdNode, in ParamEntity param)
		{
			Infrastructure.NotNull(param.Name);

			_documentIdNode = documentIdNode;
			_param = param;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _param.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Name}(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IParam

		public string Name => _param.Name!;

		public IValueExpression? Expression => _param.Expression;

		public ILocationExpression? Location => _param.Location;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ParamNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.Name, Name);
			bucket.AddEntity(Key.Expression, Expression);
			bucket.AddEntity(Key.Location, Location);
		}

	#endregion
	}
}