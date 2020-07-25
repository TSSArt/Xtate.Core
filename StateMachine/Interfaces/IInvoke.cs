using System;
using System.Collections.Immutable;

namespace Xtate
{
	public interface IInvoke : IEntity
	{
		Uri?                                Type             { get; }
		IValueExpression?                   TypeExpression   { get; }
		Uri?                                Source           { get; }
		IValueExpression?                   SourceExpression { get; }
		string?                             Id               { get; }
		ILocationExpression?                IdLocation       { get; }
		ImmutableArray<ILocationExpression> NameList         { get; }
		bool                                AutoForward      { get; }
		ImmutableArray<IParam>              Parameters       { get; }
		IFinalize?                          Finalize         { get; }
		IContent?                           Content          { get; }
	}
}