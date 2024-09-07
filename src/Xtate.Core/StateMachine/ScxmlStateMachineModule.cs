using Xtate.IoC;
using Xtate.Scxml;

namespace Xtate.Core;

public class ScxmlStateMachineModule : Module<StateMachineServiceModule, ScxmlModule>
{
	protected override void AddServices()
	{
		Services.AddType<ScxmlReaderStateMachineGetter>();
		Services.AddImplementation<ScxmlStateMachineProvider>().For<IStateMachineProvider>();
	}
}