using System.Collections.Immutable;

namespace Xtate
{
	public interface IForEach : IExecutableEntity
	{
		IValueExpression?                 Array  { get; }
		ILocationExpression?              Item   { get; }
		ILocationExpression?              Index  { get; }
		ImmutableArray<IExecutableEntity> Action { get; }
	}
}