namespace Xtate.Core;

public class DeadLetterQueue<TSource> : IDeadLetterQueue<TSource>
{
	public required ILogger<IDeadLetterQueue<TSource>> Logger { private get; [UsedImplicitly] init; }

	public ValueTask Enqueue(ServiceId recipientServiceId, IIncomingEvent incomingEvent) =>
		Logger.Write(Level.Warning, 1, $@"Event can't be delivered to {recipientServiceId.ServiceType} [{recipientServiceId}].", incomingEvent);
}