using System.Collections.Immutable;

namespace Xtate
{
	public interface IDoneData : IEntity
	{
		IContent?              Content    { get; }
		ImmutableArray<IParam> Parameters { get; }
	}
}