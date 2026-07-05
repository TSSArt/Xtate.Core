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

using Xtate.StateMachineHost;

namespace Xtate.IoProcessors.NamedPipe.Services;

[InstantiatedByIoC]
public class NamedPipeIoProcessorHost : ResilientIoProcessorHostBase
{
	public required NamedPipeController NamedPipeController { private get; [SetByIoC] init; }

	public required IExternalEventDispatcher<NamedPipeIoProcessorHost> ExternalEventDispatcher { private get; [SetByIoC] init; }

	protected override async Task ProtectedBackgroundProcess()
	{
		if(!NamedPipeController.IsNamedPipeIoProcessorEnabled)
		{
			return;
		}

		while (!Token.IsCancellationRequested)
		{
			await NamedPipeController.ReceiveAndProcessEvent(ProcessEvent,  Token).ConfigureAwait(false);
		}
	}

	private ValueTask ProcessEvent(NamedPipeEventMessage eventMessage) => ExternalEventDispatcher.Dispatch(eventMessage.TargetServiceId, eventMessage, Token);
}