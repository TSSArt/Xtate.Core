using Xtate.Service;

namespace Xtate;

public class StateMachineExternalServiceProvider : IExternalServiceProvider, IExternalServiceActivator
{
	private static readonly Uri ServiceFactoryTypeId      = new(@"http://www.w3.org/TR/scxml/");
	private static readonly Uri ServiceFactoryAliasTypeId = new(uriString: @"scxml", UriKind.Relative);

	public required IExternalServiceDefinition    ExternalServiceDefinition    { private get; [UsedImplicitly] init; }
	public required IStateMachineLocation StateMachineLocation { private get; [UsedImplicitly] init; }
	public required IHostController       HostController       { private get; [UsedImplicitly] init; }

#region Interface IServiceFactory

	ValueTask<IExternalServiceActivator?> IExternalServiceProvider.TryGetActivator(Uri type) => new(CanHandle(type) ? this : null);

#endregion

#region Interface IServiceActivator

	public async ValueTask<IExternalService> StartService()
	{
		Infra.Assert(CanHandle(ExternalServiceDefinition.Type));

		var sessionId = SessionId.New();
		var scxml = ExternalServiceDefinition.RawContent ?? ExternalServiceDefinition.Content.AsStringOrDefault();
		var parameters = ExternalServiceDefinition.Parameters;
		var source = ExternalServiceDefinition.Source;

		Infra.Assert(scxml is not null || source is not null);

		var stateMachineClass = scxml is not null
			? (StateMachineClass) new ScxmlStringStateMachine(scxml) { Location = StateMachineLocation.Location!, Arguments = parameters }
			: new LocationStateMachine(StateMachineLocation.Location.CombineWith(source!)) { Arguments = parameters };

		return await HostController.StartStateMachine(stateMachineClass, SecurityContextType.InvokedService).ConfigureAwait(false);
	}

	[Obsolete]
	ValueTask<IExternalService> IExternalServiceActivator.StartService(Uri? baseUri,
													   InvokeData invokeData,
													   IServiceCommunication serviceCommunication)
	{
		throw new NotImplementedException();
	}

#endregion

	private static bool CanHandle(Uri type) => FullUriComparer.Instance.Equals(type, ServiceFactoryTypeId) || FullUriComparer.Instance.Equals(type, ServiceFactoryAliasTypeId);
}