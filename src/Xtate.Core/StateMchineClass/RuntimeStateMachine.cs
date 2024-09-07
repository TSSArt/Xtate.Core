using Xtate.IoC;

namespace Xtate.Core;

public class RuntimeStateMachine(IStateMachine stateMachine) : StateMachineClass, IStateMachineArguments, IStateMachineLocation
{
	public Uri Location { get; init; } = default!;

	public DataModelValue Arguments { get; init; }

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddConstant(stateMachine);
		services.AddConstant<IStateMachineArguments>(this);

		if (Location is not null)
		{
			services.AddConstant<IStateMachineLocation>(this);
		}
	}
}