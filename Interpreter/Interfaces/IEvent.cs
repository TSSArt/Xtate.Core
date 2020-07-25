using System;
using System.Collections.Immutable;

namespace Xtate
{
	public interface IEvent : IEntity
	{
		SendId?                     SendId     { get; }
		ImmutableArray<IIdentifier> NameParts  { get; }
		EventType                   Type       { get; }
		Uri?                        Origin     { get; }
		Uri?                        OriginType { get; }
		InvokeId?                   InvokeId   { get; }
		DataModelValue              Data       { get; }
	}
}