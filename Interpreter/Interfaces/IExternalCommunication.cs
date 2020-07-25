using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate
{
	public enum SendStatus
	{
		Sent,
		ToSchedule,
		ToInternalQueue
	}

	public class InvokeData
	{
		public InvokeData(InvokeId invokeId, Uri type, Uri? source, string? rawContent, DataModelValue content, DataModelValue parameters)
		{
			InvokeId = invokeId;
			Type = type;
			Source = source;
			RawContent = rawContent;
			Content = content;
			Parameters = parameters;
		}

		public InvokeId       InvokeId   { get; }
		public Uri            Type       { get; }
		public Uri?           Source     { get; }
		public string?        RawContent { get; }
		public DataModelValue Content    { get; }
		public DataModelValue Parameters { get; }
	}

	public interface IExternalCommunication
	{
		ImmutableArray<IIoProcessor> GetIoProcessors();

		ValueTask             StartInvoke(InvokeData invokeData, CancellationToken token);
		ValueTask             CancelInvoke(InvokeId invokeId, CancellationToken token);
		bool                  IsInvokeActive(InvokeId invokeId);
		ValueTask<SendStatus> TrySendEvent(IOutgoingEvent evt, CancellationToken token);
		ValueTask             ForwardEvent(IEvent evt, InvokeId invokeId, CancellationToken token);
		ValueTask             CancelEvent(SendId sendId, CancellationToken token);
	}
}