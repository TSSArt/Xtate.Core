#region Copyright © 2019-2023 Sergii Artemenko

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

using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public abstract class ExecutableEntityNode : IExecutableEntity, IExecEvaluator, IStoreSupport, IDocumentId
{
<<<<<<< Updated upstream
	public abstract class ExecutableEntityNode : IExecutableEntity, IExecEvaluator, IStoreSupport, IDocumentId
	{
		private readonly IExecEvaluator _execEvaluator;
		private          DocumentIdSlot _documentIdSlot;

		protected ExecutableEntityNode(DocumentIdNode documentIdNode, IExecutableEntity entity)
		{
			_execEvaluator = entity.As<IExecEvaluator>();
			documentIdNode.SaveToSlot(out _documentIdSlot);
		}

	#region Interface IDocumentId

		public int DocumentId => _documentIdSlot.Value;

	#endregion

	#region Interface IExecEvaluator

		public ValueTask Execute() => _execEvaluator.Execute();

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket) => Store(bucket);

	#endregion

		protected abstract void Store(Bucket bucket);
=======
	private readonly IExecEvaluator _execEvaluator;
	private          DocumentIdSlot _documentIdSlot;

	protected ExecutableEntityNode(DocumentIdNode documentIdNode, IExecutableEntity entity)
	{
		_execEvaluator = entity.As<IExecEvaluator>();
		documentIdNode.SaveToSlot(out _documentIdSlot);
>>>>>>> Stashed changes
	}

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IExecEvaluator

	public ValueTask Execute() => _execEvaluator.Execute();

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket) => Store(bucket);

#endregion

	protected abstract void Store(Bucket bucket);
}