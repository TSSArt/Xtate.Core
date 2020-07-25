using System;
using System.Collections.Immutable;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class OnExitNode : IOnExit, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly OnExitEntity     _onExit;
		private          DocumentIdRecord _documentIdNode;

		public OnExitNode(in DocumentIdRecord documentIdNode, in OnExitEntity onExit)
		{
			_onExit = onExit;
			_documentIdNode = documentIdNode;
			ActionEvaluators = onExit.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>();
		}

		public ImmutableArray<IExecEvaluator> ActionEvaluators { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _onExit.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IOnExit

		public ImmutableArray<IExecutableEntity> Action => _onExit.Action;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.OnExitNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntityList(Key.Action, Action);
		}

	#endregion
	}
}