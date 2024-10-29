using Xtate.DataModel;
using Xtate.Service;

namespace Xtate.Core;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class ExternalServiceRunner : IExternalServiceRunner, IDisposable
{
	private AsyncInit _actionOnComplete;

	public required DataConverter DataConverter { private get; init; }
	private         IExternalService      _externalService;

	public required IEventDispatcher         Creator { private get; init; }
	private         IStateMachineHostContext _stateMachineHostContext;

	private          InvokeId?                _invokeId;
	//private          ValueTask _actionOnComplete;

	public ExternalServiceRunner( IStateMachineSessionId stateMachineSessionId, IStateMachineInvokeId stateMachineInvokeId, IExternalService externalService, IStateMachineHostContext stateMachineHostContext )
	{
		_invokeId = stateMachineInvokeId.InvokeId;
		_externalService = externalService;
		_stateMachineHostContext = stateMachineHostContext;
		_stateMachineHostContext.AddService(stateMachineSessionId.SessionId, _invokeId, _externalService, default);

		//_actionOnComplete = ActionOnComplete().Preserve();

		_actionOnComplete = AsyncInit.Run(ActionOnComplete);
	}

	private async ValueTask ActionOnComplete()
	{
		try
		{
			var result = await _externalService.GetResult().ConfigureAwait(false);

			var nameParts = EventName.GetDoneInvokeNameParts(_invokeId);
			var evt = new EventObject { Type = EventType.External, NameParts = nameParts, Data = result, InvokeId = _invokeId };
			await Creator.Send(evt, token: default).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var evt = new EventObject
					  {
						  Type = EventType.External,
						  NameParts = EventName.ErrorExecution,
						  Data = DataConverter.FromException(ex),
						  InvokeId = _invokeId
					  };
			await Creator.Send(evt, token: default).ConfigureAwait(false);
		}
	}
#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion


	public ValueTask WaitForCompletion() => new (_actionOnComplete.Task);


	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Interlocked.Exchange(ref _invokeId, value: default) is { } invokeId)
			{
				_stateMachineHostContext.TryRemoveService(null, invokeId);
			}
		}
	}
}