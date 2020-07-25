using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class RaiseNode : ExecutableEntityNode, IRaise, IAncestorProvider, IDebugEntityId
	{
		private readonly RaiseEntity _entity;

		public RaiseNode(in DocumentIdRecord documentIdNode, in RaiseEntity entity) : base(documentIdNode, (IRaise?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IRaise

		public IOutgoingEvent? OutgoingEvent => _entity.OutgoingEvent;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.RaiseNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Event, OutgoingEvent);
		}
	}
}