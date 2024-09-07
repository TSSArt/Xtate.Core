using Xtate.IoC;
using Xtate.Scxml;

namespace Xtate.Core;

public class SourceStateMachineModule : Module<StateMachineServiceModule, ScxmlModule>
{
	protected override void AddServices()
	{
		Services.AddType<ScxmlLocationStateMachineGetter>();
		Services.AddImplementation<SourceStateMachineProvider>().For<IStateMachineProvider>();
	}
}