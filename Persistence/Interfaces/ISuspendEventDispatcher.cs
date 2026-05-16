namespace Xtate.Core;

public interface ISuspendEventDispatcher
{
	event Action OnSuspend;
}