using Xtate.IoC;

namespace Xtate.Core;

public class StateMachineServiceModule : Module
{
	protected override void AddServices()
	{
		Services.AddSharedFactory<StateMachineGetter>(SharedWithin.Scope).For<IStateMachine>();
		Services.AddImplementation<StateMachineService>().For<IStateMachineService>();
	}
}