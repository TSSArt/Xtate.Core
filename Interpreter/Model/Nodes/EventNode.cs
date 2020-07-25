using System;
using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class EventNode : IOutgoingEvent, IStoreSupport, IAncestorProvider
	{
		private readonly IOutgoingEvent _event;

		public EventNode(IOutgoingEvent evt) => _event = evt;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _event;

	#endregion

	#region Interface IOutgoingEvent

		public ImmutableArray<IIdentifier> NameParts => _event.NameParts;
		public SendId?                     SendId    => _event.SendId;
		public DataModelValue              Data      => _event.Data;
		public Uri?                        Target    => _event.Target;
		public Uri?                        Type      => _event.Type;
		public int                         DelayMs   => _event.DelayMs;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.Id, EventName.ToName(_event.NameParts));
		}

	#endregion
	}
}