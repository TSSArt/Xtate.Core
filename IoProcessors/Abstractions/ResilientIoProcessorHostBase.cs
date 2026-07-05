using Xtate.IoProcessors.NamedPipe.Internal;
using Xtate.Logging;

namespace Xtate.IoProcessors;

public abstract class ResilientIoProcessorHostBase : IoProcessorHostBase
{
	public required ILogger<ResilientIoProcessorHostBase> LoggerBase { private get; [SetByIoC] init; }

	protected abstract Task ProtectedBackgroundProcess();

	protected sealed override async Task BackgroundProcess()
	{
		var delayedTries = new DelayedTries
						   {
							   MinDelay = TimeSpan.FromMilliseconds(125),
							   MaxDelay = TimeSpan.FromSeconds(60)
						   };

		while (!Token.IsCancellationRequested)
		{
			try
			{
				await ProtectedBackgroundProcess().ConfigureAwait(false);

				return;
			}
			catch (OperationCanceledException) when (Token.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				await LoggerBase.Write(Level.Error, eventId: 1, Resources.Message_AnErrorOccurredInTheBackgroundProcess, ex).ConfigureAwait(false);
			}

			try
			{
				await delayedTries.Delay(Token).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (Token.IsCancellationRequested)
			{
				break;
			}
		}
	}
}