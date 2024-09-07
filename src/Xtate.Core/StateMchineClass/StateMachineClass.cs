using Xtate.IoC;

namespace Xtate.Core;

public abstract class StateMachineClass : IStateMachineSessionId
{
	public SessionId SessionId { get; init; } = default!;

	public virtual void AddServices(IServiceCollection services)
	{
		if (SessionId is not null)
		{
			services.AddConstant<IStateMachineSessionId>(this);
		}
	}
}