using Xtate.IoC;

namespace Xtate.Core;

public class ScxmlStringChildStateMachine(string scxml) : ScxmlStringStateMachine(scxml), IParentEventDispatcher
{
	public new required Uri? Location { init => base.Location = value; }

	public new required DataModelValue Arguments { init => base.Arguments = value; }

	public required IEventDispatcher? ParentEventDispatcher { private get; [UsedImplicitly] init; }

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddConstant<IParentEventDispatcher>(this);
	}

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => ParentEventDispatcher?.Dispatch(incomingEvent) ?? default;
}