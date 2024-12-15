namespace Xtate.Core;

public interface IDeadLetterQueue<TSource>
{
	ValueTask Enqueue(ServiceId recipientServiceId, IIncomingEvent incomingEvent);
}