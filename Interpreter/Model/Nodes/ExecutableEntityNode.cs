#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

using System.Threading;
using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal abstract class ExecutableEntityNode : IExecutableEntity, IExecEvaluator, IStoreSupport, IDocumentId
	{
		private readonly IExecEvaluator   _execEvaluator;
		private          DocumentIdRecord _documentIdNode;

		protected ExecutableEntityNode(in DocumentIdRecord documentIdNode, IExecutableEntity? entity)
		{
			Infrastructure.Assert(entity != null);

			_execEvaluator = entity.As<IExecEvaluator>();
			_documentIdNode = documentIdNode;
		}

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IExecEvaluator

		public ValueTask Execute(IExecutionContext executionContext, CancellationToken token) => _execEvaluator.Execute(executionContext, token);

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket) => Store(bucket);

	#endregion

		protected abstract void Store(Bucket bucket);
	}
}