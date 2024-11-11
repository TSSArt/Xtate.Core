using Xtate.IoC;

namespace Xtate.Core;

public class LocationChildStateMachine(Uri location) : LocationStateMachine(location), IParentEventDispatcher
{
	public IEventDispatcher? ParentEventDispatcher { get; init; }

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddConstant<IParentEventDispatcher>(this);
	}

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => ParentEventDispatcher?.Dispatch(incomingEvent) ?? default;
}