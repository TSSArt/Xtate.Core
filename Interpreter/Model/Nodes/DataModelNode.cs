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

using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class DataModelNode : IDataModel, IStoreSupport, IAncestorProvider, IDocumentId
	{
		private readonly DataModelEntity  _dataModel;
		private          DocumentIdRecord _documentIdNode;

		public DataModelNode(in DocumentIdRecord documentIdNode, in DataModelEntity dataModel)
		{
			_documentIdNode = documentIdNode;
			_dataModel = dataModel;
			Data = dataModel.Data.AsArrayOf<IData, DataNode>(true);
		}

		public ImmutableArray<DataNode> Data { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _dataModel.Ancestor;

	#endregion

	#region Interface IDataModel

		ImmutableArray<IData> IDataModel.Data => _dataModel.Data;

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.DataModelNode);
			bucket.AddEntityList(Key.DataList, Data);
		}

	#endregion
	}
}