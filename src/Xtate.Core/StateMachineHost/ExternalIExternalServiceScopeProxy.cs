using Xtate.DataModel;

namespace Xtate.Core;

public class ExternalIExternalServiceScopeProxy(
	InvokeId invokeId,
	InvokeData invokeData,
	IEventDispatcher eventDispatcher,
	IStateMachineSessionId stateMachineSessionId,
	IStateMachineLocation stateMachineLocation,
	ICaseSensitivity caseSensitivity)
	: IStateMachineInvokeId, IExternalServiceDefinition, ICaseSensitivity, IStateMachineSessionId, IStateMachineLocation, IEventDispatcher
{
	public InvokeId InvokeId => invokeId;

	public bool CaseInsensitive { get; } = caseSensitivity.CaseInsensitive;

	public SessionId SessionId { get; } = stateMachineSessionId.SessionId;

	public Uri? Location { get; } = stateMachineLocation.Location;

	public ValueTask Send(IEvent evt, CancellationToken token) => eventDispatcher.Send(evt, token);

	public Uri Type { get; } = invokeData.Type;

	public Uri? Source { get; } = invokeData.Source;

	public string? RawContent { get; } = invokeData.RawContent;

	public DataModelValue Content    { get; } = invokeData.Content;
	
	public DataModelValue Parameters { get; } = invokeData.Parameters;
}