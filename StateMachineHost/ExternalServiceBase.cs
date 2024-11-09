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

public abstract class ExternalServiceBase : IExternalService, IDisposable, IAsyncDisposable
{
	private readonly DisposingToken _disposingToken = new();

	private readonly AsyncInit<DataModelValue> _execution;

	protected ExternalServiceBase() => _execution = AsyncInit.Run(this, es => es.ExecuteWithCancellation());

	public required IExternalServiceSource ExternalServiceSource { private get; [UsedImplicitly] init; }

	public required IExternalServiceParameters ExternalServiceParameters { private get; [UsedImplicitly] init; }

	protected Uri? Source => ExternalServiceSource.Source;

	protected string? RawContent => ExternalServiceSource.RawContent;

	protected DataModelValue Content => ExternalServiceSource.Content;

	protected DataModelValue Parameters => ExternalServiceParameters.Parameters;

	protected CancellationToken DestroyToken => _disposingToken.Token;

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IExternalService

	async ValueTask<DataModelValue> IExternalService.GetResult()
	{
		await _execution.Task.ConfigureAwait(false);

		return _execution.Value;
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Cancel();
			_disposingToken.Dispose();
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _disposingToken.CancelAsync().ConfigureAwait(false);
		_disposingToken.Dispose();
	}

	private ValueTask<DataModelValue> ExecuteWithCancellation() => Execute().WaitAsync(_disposingToken.Token);

	protected abstract ValueTask<DataModelValue> Execute();
}