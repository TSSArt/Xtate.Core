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

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class DoneDataNode : IDoneData, IStoreSupport, IAncestorProvider
	{
		private readonly DoneDataEntity    _doneData;
		private readonly IObjectEvaluator _doneDataEvaluator;

		public DoneDataNode(in DoneDataEntity doneData)
		{
			_doneData = doneData;

			Infrastructure.Assert(doneData.Ancestor != null);

			_doneDataEvaluator = doneData.Ancestor.As<IObjectEvaluator>();
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _doneData.Ancestor;

	#endregion

	#region Interface IDoneData

		public IContent? Content => _doneData.Content;

		public ImmutableArray<IParam> Parameters => _doneData.Parameters;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.DoneDataNode);
			bucket.AddEntity(Key.Content, Content);
			bucket.AddEntityList(Key.Parameters, Parameters);
		}

	#endregion

		public async ValueTask<DataModelValue> Evaluate(IExecutionContext executionContext, CancellationToken token)
		{
			var obj = await _doneDataEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

			return DataModelValue.FromObject(obj);
		}
	}
}