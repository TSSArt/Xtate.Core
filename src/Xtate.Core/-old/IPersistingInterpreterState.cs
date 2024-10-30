using Xtate.Persistence;

namespace Xtate.Core;

public interface IPersistingInterpreterState
{
	public Bucket StateBucket { get; }

	public ValueTask CheckPoint(int level);
}