using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Xtate.Core;

public interface IEventQueueReader
{
	bool TryReadEvent([MaybeNullWhen(false)] out IEvent evt);

	ValueTask<bool> WaitToEvent();

	void Complete();
}