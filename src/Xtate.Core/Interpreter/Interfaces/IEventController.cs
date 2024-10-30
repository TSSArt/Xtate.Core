namespace Xtate;

public interface IEventController
{
	ValueTask Send(IOutgoingEvent outgoingEvent);

	ValueTask Cancel(SendId sendId);
}