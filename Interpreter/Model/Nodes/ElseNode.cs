using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ElseNode : IElse, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly ElseEntity       _entity;
		private          DocumentIdRecord _documentIdNode;

		public ElseNode(in DocumentIdRecord documentIdNode, in ElseEntity entity)
		{
			_documentIdNode = documentIdNode;
			_entity = entity;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ElseNode);
			bucket.Add(Key.DocumentId, DocumentId);
		}

	#endregion
	}
}