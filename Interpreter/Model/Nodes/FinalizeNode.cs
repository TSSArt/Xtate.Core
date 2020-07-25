using System.Collections.Immutable;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class FinalizeNode : IFinalize, IStoreSupport, IAncestorProvider
	{
		private readonly FinalizeEntity _finalize;

		public FinalizeNode(in FinalizeEntity finalize)
		{
			_finalize = finalize;
			ActionEvaluators = finalize.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>();
		}

		public ImmutableArray<IExecEvaluator> ActionEvaluators { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _finalize.Ancestor;

	#endregion

	#region Interface IFinalize

		public ImmutableArray<IExecutableEntity> Action => _finalize.Action;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.FinalizeNode);
			bucket.AddEntityList(Key.Action, Action);
		}

	#endregion
	}
}