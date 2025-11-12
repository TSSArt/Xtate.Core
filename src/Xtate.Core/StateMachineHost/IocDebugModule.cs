using Xtate.IoC;

namespace Xtate;

public class IocDebugModule : Module<LoggingModule>
{
	protected override void AddServices()
	{
		AddDebugServices();
	}

	private void AddDebugServices()
	{
		Services.AddSharedImplementationSync<IocDebugLogger>(SharedWithin.Container).For<IServiceProviderActions>();
	}
}