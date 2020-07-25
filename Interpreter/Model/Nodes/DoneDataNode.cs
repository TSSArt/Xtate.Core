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