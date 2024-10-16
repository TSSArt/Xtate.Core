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

namespace Xtate.Service;

public abstract class ServiceBase : IService, IAsyncDisposable, IDisposable
{
	private readonly TaskCompletionSource<DataModelValue> _completedTcs = new();
	private readonly CancellationTokenSource              _tokenSource  = new();

	private bool        _disposed;
	private InvokeData? _invokeData;

	protected Uri?                  BaseUri              { get; private set; }
	protected IServiceCommunication ServiceCommunication { get; private set; } = default!;
	protected Uri?                  Source               => _invokeData?.Source;
	protected string?               RawContent           => _invokeData?.RawContent;
	protected DataModelValue        Content              => _invokeData?.Content ?? default;
	protected DataModelValue        Parameters           => _invokeData?.Parameters ?? default;
	protected CancellationToken     StopToken            => _tokenSource.Token;

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
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

#region Interface IEventDispatcher

	ValueTask IEventDispatcher.Send(IEvent evt, CancellationToken token) => default;

#endregion

#region Interface IService

	public ValueTask<DataModelValue> GetResult() => new(_completedTcs.Task);

	ValueTask IService.Destroy()
	{
		_tokenSource.Cancel();
		_completedTcs.TrySetCanceled();

		return default;
	}

#endregion

	internal void Start(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication)
	{
		BaseUri = baseUri;
		_invokeData = invokeData;
		ServiceCommunication = serviceCommunication;

		RunAsync().Forget();

		async ValueTask RunAsync()
		{
			try
			{
				_completedTcs.TrySetResult(await Execute().ConfigureAwait(false));
			}
			catch (OperationCanceledException ex)
			{
				_completedTcs.TrySetCanceled(ex.CancellationToken);
			}
			catch (Exception ex)
			{
				_completedTcs.TrySetException(ex);
			}
		}
	}

	protected abstract ValueTask<DataModelValue> Execute();

	protected virtual ValueTask DisposeAsyncCore()
	{
		if (!_disposed)
		{
			_tokenSource.Dispose();

			_disposed = true;
		}

		return default;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			_tokenSource.Dispose();
		}

		_disposed = true;
	}
}