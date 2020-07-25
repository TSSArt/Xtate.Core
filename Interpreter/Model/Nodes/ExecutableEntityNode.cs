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