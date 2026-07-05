using Xtate.IoC.Tools;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.IoProcessors;

public abstract class IoProcessorHostBase : IIoProcessorHost
{
	private CancellationTokenSource? _cts;

	protected CancellationToken Token { get; private set; } = new(true);

	public required DisposeToken DisposeTokenBase { private get; [SetByIoC] init; }

	public required ITaskMonitor TaskMonitorBase { private get; [SetByIoC] init; }

#region Interface IIoProcessorHost

	ValueTask IIoProcessorHost.Start() => Start();

	ValueTask IIoProcessorHost.Stop() => Stop();

#endregion

	protected virtual async ValueTask Start()
	{
		while (_cts?.IsCancellationRequested == true)
		{
			await Stop().ConfigureAwait(false);
		}

		if (_cts is null)
		{
			_cts = CancellationTokenSource.CreateLinkedTokenSource(DisposeTokenBase);
			Token = _cts.Token;

			StartNewBackgroundProcess().Forget(TaskMonitorBase);
		}
	}

	protected virtual async ValueTask Stop()
	{
		if (_cts is { } cts)
		{
			Token = new CancellationToken(true);
			_cts = null;

			await cts.CancelAsync().ConfigureAwait(false);

			cts.Dispose();
		}
	}

	protected virtual Task StartNewBackgroundProcess()
	{
		var task = Task.Factory.StartNew(static state => ((IoProcessorHostBase) state!).BackgroundProcess(), this, Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

		return task.Unwrap();
	}

	protected abstract Task BackgroundProcess();
}