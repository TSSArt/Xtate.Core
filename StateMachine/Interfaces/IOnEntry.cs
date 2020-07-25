using System.Collections.Immutable;

namespace Xtate
{
	public interface IOnEntry : IEntity
	{
		ImmutableArray<IExecutableEntity> Action { get; }
	}
}