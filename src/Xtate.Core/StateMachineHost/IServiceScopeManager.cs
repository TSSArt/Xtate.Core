namespace Xtate.Core;

public interface IServiceScopeManager
{
	ValueTask StartService(InvokeId invokeId, InvokeData invokeData);
}