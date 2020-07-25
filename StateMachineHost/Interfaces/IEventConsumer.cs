using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public interface IEventConsumer
	{
		ValueTask Dispatch(SessionId sessionId, IEvent evt, CancellationToken token);
	}
}