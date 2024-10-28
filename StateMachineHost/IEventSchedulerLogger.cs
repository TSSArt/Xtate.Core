namespace Xtate.Core;

public interface IEventSchedulerLogger
{
	bool IsEnabled { get; }

	ValueTask LogError(string message, Exception exception, IHostEvent scheduledEvent);
}