// Copyright © 2019-2024 Sergii Artemenko
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
	private readonly CancellationTokenSource _destroyTokenSource = new();

	private readonly AsyncInit<DataModelValue> _executionAsyncInit;

	protected ExternalServiceBase() => _executionAsyncInit = AsyncInit.Run(this, es => es.ExecuteWithCancellation());

	public required IExternalServiceDefinition ExternalServiceDefinition { private get; [UsedImplicitly] init; }

	protected Uri? Source => ExternalServiceDefinition.Source;

	protected string? RawContent => ExternalServiceDefinition.RawContent;

	protected DataModelValue Content => ExternalServiceDefinition.Content;

	protected DataModelValue Parameters => ExternalServiceDefinition.Parameters;

	protected CancellationToken DestroyToken => _destroyTokenSource.Token;

#region Interface IEventDispatcher

	ValueTask IEventDispatcher.Send(IEvent evt) => default;

#endregion

#region Interface IExternalService

	async ValueTask<DataModelValue> IExternalService.GetResult()
	{
		await _executionAsyncInit.Task.ConfigureAwait(false);

		return _executionAsyncInit.Value;
	}

	ValueTask IExternalService.Destroy()
	{
		_destroyTokenSource.Cancel();

		return default;
	}

#endregion

	private ValueTask<DataModelValue> ExecuteWithCancellation() => Execute().WaitAsync(_destroyTokenSource.Token);

	protected abstract ValueTask<DataModelValue> Execute();
}