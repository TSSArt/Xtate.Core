using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class CancelNode : ExecutableEntityNode, ICancel, IAncestorProvider, IDebugEntityId
	{
		private readonly CancelEntity _entity;

		public CancelNode(in DocumentIdRecord documentIdNode, in CancelEntity entity) : base(documentIdNode, (ICancel?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface ICancel

		public string? SendId => _entity.SendId;

		public IValueExpression? SendIdExpression => _entity.SendIdExpression;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.CancelNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.SendId, SendId);
			bucket.AddEntity(Key.SendIdExpression, SendIdExpression);
		}
	}
}