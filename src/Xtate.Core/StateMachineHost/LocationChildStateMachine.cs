using Xtate.IoC;

namespace Xtate.Core;

public class LocationChildStateMachine(Uri location) : LocationStateMachine(location), IParentEventDispatcher
{
	public new required DataModelValue Arguments { init => base.Arguments = value; }

	public required IEventDispatcher? ParentEventDispatcher { private get; [UsedImplicitly] init; }

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddConstant<IParentEventDispatcher>(this);
	}

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => ParentEventDispatcher?.Dispatch(incomingEvent) ?? default;
}