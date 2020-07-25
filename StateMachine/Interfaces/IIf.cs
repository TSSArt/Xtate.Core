using System.Collections.Immutable;

namespace Xtate
{
	public interface IIf : IExecutableEntity
	{
		IConditionExpression?             Condition { get; }
		ImmutableArray<IExecutableEntity> Action    { get; }
	}
}