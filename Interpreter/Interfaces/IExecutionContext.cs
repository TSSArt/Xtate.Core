using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public interface IExecutionContext
	{
		IContextItems RuntimeItems { get; }

		DataModelObject DataModel { get; }

		bool InState(IIdentifier id);

		ValueTask Cancel(SendId sendId, CancellationToken token = default);

		ValueTask Log(string? label, DataModelValue arguments = default, CancellationToken token = default);

		ValueTask Send(IOutgoingEvent evt, CancellationToken token = default);

		ValueTask StartInvoke(InvokeData invokeData, CancellationToken token = default);

		ValueTask CancelInvoke(InvokeId invokeId, CancellationToken token = default);
	}

	public interface IContextItems
	{
		object? this[object key] { get; set; }
	}
}