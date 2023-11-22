using Xtate.IoC;
using Xtate.Scxml;

namespace Xtate.Core
{
	public static class StateMachineExtensions
	{
		public static void RegisterStateMachineFactory(this IServiceCollection services)
		{
			if (!services.IsRegistered<IStateMachine>())
			{
				services.RegisterScxml();

				services.AddSharedFactory<StateMachineGetter>(SharedWithin.Scope).For<IStateMachine>();
				services.AddImplementation<StateMachineService>().For<IStateMachineService>();

				services.AddType<ScxmlReaderStateMachineGetter>();
				services.AddImplementation<ScxmlStateMachineProvider>().For<IStateMachineProvider>();

				services.AddType<ScxmlLocationStateMachineGetter>();
				services.AddImplementation<SourceStateMachineProvider>().For<IStateMachineProvider>();
			}
		}
	}
}