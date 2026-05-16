namespace Xtate.Core;

public class SuspendEventDispatcher : ISuspendEventDispatcher
{
	private event Action? OnSuspend;

	event Action? ISuspendEventDispatcher.OnSuspend
	{
		add => OnSuspend += value;
		remove => this.OnSuspend -= value;
	}

	public void Suspend()
	{
		OnSuspend?.Invoke();
	}
}