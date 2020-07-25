using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class IdentifierNode : IIdentifier, IStoreSupport, IAncestorProvider, IDebugEntityId
	{
		private readonly IIdentifier _identifier;

		public IdentifierNode(IIdentifier id) => _identifier = id ?? throw new ArgumentNullException(nameof(id));

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _identifier;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{_identifier}";

	#endregion

	#region Interface IIdentifier

		public string Value => _identifier.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.IdentifierNode);
			bucket.Add(Key.Id, _identifier.Value);
		}

	#endregion
	}
}