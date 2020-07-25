using System.Collections.Immutable;

namespace Xtate
{
	public interface IOnExit : IEntity
	{
		ImmutableArray<IExecutableEntity> Action { get; }
	}
}