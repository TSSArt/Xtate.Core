using System.Collections.Immutable;

namespace Xtate
{
	public interface IDataModel : IEntity
	{
		ImmutableArray<IData> Data { get; }
	}
}