namespace Xtate.Core;

public interface IExternalServiceScopeManager
{
	ValueTask StartService(InvokeId invokeId, InvokeData invokeData);
}