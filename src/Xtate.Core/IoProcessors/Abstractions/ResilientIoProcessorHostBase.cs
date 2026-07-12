// Copyright © 2019-2026 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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