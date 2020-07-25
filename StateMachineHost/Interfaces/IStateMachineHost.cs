using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate
{
	internal interface IStateMachineHost
	{
		ImmutableArray<IIoProcessor> GetIoProcessors();
		ValueTask<SendStatus>        DispatchEvent(SessionId sessionId, IOutgoingEvent evt, bool skipDelay, CancellationToken token);
		ValueTask                    StartInvoke(SessionId sessionId, InvokeData invokeData, CancellationToken token);
		ValueTask                    CancelInvoke(SessionId sessionId, InvokeId invokeId, CancellationToken token);
		bool                         IsInvokeActive(SessionId sessionId, InvokeId invokeId);
		ValueTask                    ForwardEvent(SessionId sessionId, IEvent evt, InvokeId invokeId, CancellationToken token);
	}
}