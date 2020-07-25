using System;
using System.Collections.Immutable;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class OnEntryNode : IOnEntry, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly OnEntryEntity    _onEntry;
		private          DocumentIdRecord _documentIdNode;

		public OnEntryNode(in DocumentIdRecord documentIdNode, in OnEntryEntity onEntry)
		{
			_onEntry = onEntry;
			_documentIdNode = documentIdNode;
			ActionEvaluators = onEntry.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>();
		}

		public ImmutableArray<IExecEvaluator> ActionEvaluators { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _onEntry.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IOnEntry

		public ImmutableArray<IExecutableEntity> Action => _onEntry.Action;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.OnEntryNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntityList(Key.Action, Action);
		}

	#endregion
	}
}