using System.Collections.Immutable;

namespace Xtate
{
	public interface IFinalize : IEntity
	{
		ImmutableArray<IExecutableEntity> Action { get; }
	}
}