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

using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC.Tools;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.ExternalServices;

public abstract class ExternalServiceBase : IExternalService, IEventDispatcher
{
	private readonly CancellationToken? _destroyToken;

	private readonly LazyTask<DataModelValue>? _lazyTask;

	private readonly ITaskMonitor? _taskMonitor;

	[SetByIoC]
	public required IExternalServiceSource ExternalServiceSourceBase
	{
		init
		{
			Source = value.Source;
			Content = value.Content;
			RawContent = value.RawContent;
		}
	}

	[SetByIoC]
	public required IExternalServiceParameters ExternalServiceParametersBase
	{
		init => Parameters = value.Parameters;
	}

	[SetByIoC]
	public required DisposeToken DisposeTokenBase
	{
		init
		{
			_destroyToken = value.Token;

			if (_taskMonitor is not null && _lazyTask is null)
			{
				_lazyTask = new LazyTask<DataModelValue>(Execute, _taskMonitor, _destroyToken.Value);
			}
		}
	}

	[SetByIoC]
	public required ITaskMonitor TaskMonitorBase
	{
		init
		{
			_taskMonitor = value;

			if (_destroyToken is not null && _lazyTask is null)
			{
				_lazyTask = new LazyTask<DataModelValue>(Execute, _taskMonitor, _destroyToken.Value);
			}
		}
	}

	protected Uri? Source { get; private init; }

	protected string? RawContent { get; private init; }

	protected DataModelValue Content { get; private init; }

	protected DataModelValue Parameters { get; private init; }

	protected CancellationToken DestroyToken => _destroyToken!.Value;

#region Interface IEventDispatcher

	ValueTask IEventDispatcher.Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => Dispatch(incomingEvent, token);

#endregion

#region Interface IExternalService

	ValueTask<DataModelValue> IExternalService.GetResult()
	{
		Infra.NotNull(_lazyTask);

		return new ValueTask<DataModelValue>(_lazyTask.Task);
	}

#endregion

	protected abstract ValueTask<DataModelValue> Execute();

	protected virtual ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => ValueTask.CompletedTask;
}