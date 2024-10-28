namespace Xtate.Core;

public class StateMachineHostExternalCommunication : IExternalCommunication
{
	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	public required IStateMachineLocation?  StateMachineLocation  { private get; [UsedImplicitly] init; }

	public required IStateMachineHost    StateMachineHost     { private get; [UsedImplicitly] init; }
	
	public required IServiceScopeManager ServiceScopeManager { private get; [UsedImplicitly] init; }

	//public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

	private SessionId SessionId => StateMachineSessionId.SessionId;

#region Interface IExternalCommunication

	public ValueTask<SendStatus> TrySendEvent(IOutgoingEvent outgoingEvent) => StateMachineHost.DispatchEvent(SessionId, outgoingEvent, CancellationToken.None);

	public ValueTask ForwardEvent(InvokeId invokeId, IEvent evt) => StateMachineHost.ForwardEvent(SessionId, invokeId, evt, token: default);

	public ValueTask CancelEvent(SendId sendId) => StateMachineHost.CancelEvent(SessionId, sendId, CancellationToken.None);

	//public ValueTask StartInvoke(InvokeData invokeData) => StateMachineHost.StartInvoke(SessionId, StateMachineLocation?.Location, invokeData, CancellationToken.None);
	public ValueTask StartInvoke(InvokeId invokeId, InvokeData invokeData)
	{
		return ServiceScopeManager.StartService(invokeId, invokeData);
	}
	/*
	public async ValueTask StartInvoke(InvokeData invokeData)
	{
		await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);


		IServiceActivator activator = await FindServiceFactoryActivator(data.Type).ConfigureAwait(false);

		var serviceCommunication = new ServiceCommunication(this, GetTarget(sessionId), IoProcessorId, data.InvokeId);

		var invokedService = await activator.StartService(location, data, serviceCommunication).ConfigureAwait(false);

		await context.AddService(sessionId, data.InvokeId, invokedService, token).ConfigureAwait(false);

		CompleteAsync(context, invokedService, service, sessionId, data.InvokeId, _dataConverter).Forget();
	}
	*/

	public ValueTask CancelInvoke(InvokeId invokeId) => StateMachineHost.CancelInvoke(SessionId, invokeId, token: default);
	/*public ValueTask CancelInvoke(InvokeId invokeId)
	{
		var context = GetCurrentContext();

		context.ValidateSessionId(sessionId, out _);

		if (await context.TryRemoveService(sessionId, invokeId).ConfigureAwait(false) is { } service)
		{
			await service.Destroy().ConfigureAwait(false);

			await DisposeInvokedService(service).ConfigureAwait(false);
		}
	}*/

#endregion
}