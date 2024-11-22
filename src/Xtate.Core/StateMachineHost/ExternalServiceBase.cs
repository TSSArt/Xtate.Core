﻿// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.ExternalService;

public abstract class ExternalServiceBase : IExternalService
{
	private readonly LazyTask<DataModelValue> _lazyTask = default!;

	[UsedImplicitly]
	public required IExternalServiceSource ExternalServiceSourceLocal
	{
		init
		{
			Source = value.Source;
			Content = value.Content;
			RawContent = value.RawContent;
		}
	}

	[UsedImplicitly]
	public required IExternalServiceParameters ExternalServiceParametersLocal
	{
		init => Parameters = value.Parameters;
	}

	[UsedImplicitly]
	public required DisposeToken DisposeTokenLocal
	{
		init
		{
			DestroyToken = value.Token;
			_lazyTask = new LazyTask<DataModelValue>(Execute, value.TaskMonitor, value.Token);
		}
	}

	protected Uri? Source { get; private init; }

	protected string? RawContent { get; private init; }

	protected DataModelValue Content { get; private init; }

	protected DataModelValue Parameters { get; private init; }

	protected CancellationToken DestroyToken { get; private init; }

#region Interface IExternalService

	ValueTask<DataModelValue> IExternalService.GetResult() => new(_lazyTask.Task);

#endregion

	protected abstract ValueTask<DataModelValue> Execute();
}