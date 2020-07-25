using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class LogNode : ExecutableEntityNode, ILog, IAncestorProvider, IDebugEntityId
	{
		private readonly LogEntity _entity;

		public LogNode(in DocumentIdRecord documentIdNode, in LogEntity entity) : base(documentIdNode, (ILog?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface ILog

		public string? Label => _entity.Label;

		public IValueExpression? Expression => _entity.Expression;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.LogNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.Label, Label);
			bucket.AddEntity(Key.Expression, Expression);
		}
	}
}