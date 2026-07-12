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

namespace Xtate.StateMachineHost.Services;

public class StateMachineHostNew : IStateMachineHostNew
{
	private readonly Stack<IIoProcessorHost> _ioProcessorHosts = new();

	public required IAsyncEnumerable<IIoProcessorHost> IoProcessorHosts { private get; [SetByIoC] init; }

#region Interface IStateMachineHostNew

	ValueTask IStateMachineHostNew.Start() => Start();

	ValueTask IStateMachineHostNew.Stop() => Stop();

#endregion

	protected virtual async ValueTask Start()
	{
		await foreach (var ioProcessorHost in IoProcessorHosts.ConfigureAwait(false))
		{
			_ioProcessorHosts.Push(ioProcessorHost);

			await ioProcessorHost.Start().ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask Stop()
	{
		while (_ioProcessorHosts.Count > 0)
		{
			await _ioProcessorHosts.Pop().Stop().ConfigureAwait(false);
		}
	}
}