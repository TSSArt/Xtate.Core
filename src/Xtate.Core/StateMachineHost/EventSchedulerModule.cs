using Xtate.IoC;

namespace Xtate.Core;

public class EventSchedulerModule : Module
{
	protected override void AddServices()
	{
		Services.AddImplementation<InProcEventScheduler>().For<IEventScheduler>();
	}
}