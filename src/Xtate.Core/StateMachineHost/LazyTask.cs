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

public class LazyTask : LazyTask<ValueTuple>
{
	public LazyTask(Func<ValueTask> factory) : base(Converter(factory)) { }

	public LazyTask(Func<ValueTask> factory, TaskMonitor? taskCollector, CancellationToken token) : base(Converter(factory), taskCollector, token) { }

	private static Func<ValueTask<ValueTuple>> Converter(Func<ValueTask> factory) =>
		async () =>
		{
			await factory().ConfigureAwait(false);

			return default;
		};
}

public class LazyTask<T>
{
	private CancellationTokenRegistration _cancellationTokenRegistration;

	private TaskCompletionSource<T> _taskCompletionSource = default!;

	[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
	private CancellationToken _token;

	private readonly TaskMonitor? _taskMonitor;

	private readonly Func<ValueTask<T>> _factory;

	public LazyTask(Func<ValueTask<T>> factory)
	{
		_factory = factory;
		_token = default;
		_taskMonitor = default;
	}

	public LazyTask(Func<ValueTask<T>> factory, TaskMonitor? taskMonitor, CancellationToken token)
	{
		_factory = factory;
		_token = token;
		_taskMonitor = token.CanBeCanceled ? taskMonitor : default;
	}

	public Task<T> Task
	{
		get
		{
			if (_taskCompletionSource is { } tcs)
			{
				return tcs.Task;
			}

			tcs = new TaskCompletionSource<T>();

			if (Interlocked.CompareExchange(ref _taskCompletionSource, tcs, comparand: default) is { } existedTcs)
			{
				return existedTcs.Task;
			}

			_cancellationTokenRegistration = _token.Register(static s => ((LazyTask<T>) s!).TokenCancelled(), this);

			if (_taskMonitor is not null)
			{
				_taskMonitor.Run(static lazyTask => lazyTask.Execute(), this);
			}
			else
			{
				Execute().Preserve();
			}

			return tcs.Task;
		}
	}

	private void TokenCancelled()
	{
		_taskCompletionSource.TrySetCanceled(_token);
		DisposeCancellationRegistration();
	}

	private async ValueTask Execute()
	{
		try
		{
			var result = await _factory().ConfigureAwait(false);

			_taskCompletionSource.TrySetResult(result);
		}
		catch (OperationCanceledException) when (_token.IsCancellationRequested)
		{
			_taskCompletionSource.TrySetCanceled(_token);
		}
		catch (OperationCanceledException ex)
		{
			_taskCompletionSource.TrySetCanceled(ex.CancellationToken);
		}
		catch (Exception ex)
		{
			if (!_taskCompletionSource.TrySetException(ex))
			{
				throw;
			}
		}
		finally
		{
			DisposeCancellationRegistration();
		}
	}

	private void DisposeCancellationRegistration()
	{
		_cancellationTokenRegistration.Dispose();
		_cancellationTokenRegistration = default;
	}
}