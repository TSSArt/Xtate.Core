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

using System.Threading.Channels;
using Xtate.Persistence.Internal;

namespace Xtate.Persistence.Services;

public class PersistentChannel<T> : Channel<T> where T : class
{
	private readonly Bucket _bucket;

	private readonly TaskCompletionSource _completion;

	private readonly SemaphoreSlim _events;

	private readonly SemaphoreSlim _lock;

	private readonly Func<Bucket, T> _restore;

	private readonly ITransactionalStorage _storage;

	private readonly Action<Bucket, T> _store;

	private volatile Exception? _completed;

	private int _headIndex;

	private volatile T? _nextItem;

	private int _tailIndex;

	private volatile int _waitCount;

	public PersistentChannel(ITransactionalStorage storage, Action<Bucket, T> store, Func<Bucket, T> restore)
	{
		_storage = storage;
		_store = store;
		_restore = restore;

		_bucket = new Bucket(storage);
		_headIndex = _bucket.TryGet(Keys.Head, out int head) ? head : 0;
		_tailIndex = _bucket.TryGet(Keys.Tail, out int tail) ? tail : 0;

		_lock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
		_events = new SemaphoreSlim(unchecked(_tailIndex - _headIndex));
		_completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		Reader = new ChannelReader(this);
		Writer = new ChannelWriter(this);
	}

	private int Count => unchecked(_tailIndex - _headIndex) + (_nextItem is not null ? 1 : 0);

	private bool TryRead([MaybeNullWhen(false)] out T item)
	{
		if (!_events.Wait(0))
		{
			item = null;

			return false;
		}

		item = Interlocked.Exchange(ref _nextItem, value: null);

		var result = item is not null;

		if (!result)
		{
			_events.Release();
		}

		CheckCompletedAndDrained(!result);

		return result;
	}

	private async ValueTask<bool> WaitToReadAsync(CancellationToken token)
	{
		while (_nextItem is null)
		{
			if (CheckCompletedAndDrained())
			{
				return false;
			}

			Interlocked.Increment(ref _waitCount);

			try
			{
				await _events.WaitAsync(token).ConfigureAwait(false);
			}
			finally
			{
				Interlocked.Decrement(ref _waitCount);
			}

			try
			{
				if (_nextItem is null)
				{
					if (CheckCompletedAndDrained())
					{
						return false;
					}

					await TryDequeue(token).ConfigureAwait(false);
				}
			}
			finally
			{
				_events.Release();
			}
		}

		return true;
	}

	private ValueTask<bool> WaitToWriteAsync(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		return new ValueTask<bool>(_completed is null);
	}

	private bool TryWrite(T item)
	{
		Infra.NotNull(item);

		if (_completed is not null)
		{
			throw new ChannelClosedException();
		}

		if (_waitCount == 0 || Interlocked.CompareExchange(ref _nextItem, item, comparand: null) is not null)
		{
			return false;
		}

		_events.Release();

		return true;
	}

	private async ValueTask WriteAsync(T item, CancellationToken token)
	{
		Infra.NotNull(item);

		if (_completed is not null)
		{
			throw new ChannelClosedException();
		}

		if (_waitCount == 0 || Interlocked.CompareExchange(ref _nextItem, item, comparand: null) is not null)
		{
			await Enqueue(item, token).ConfigureAwait(false);
		}

		_events.Release();
	}

	private async ValueTask Enqueue(T item, CancellationToken token)
	{
		await _lock.WaitAsync(token).ConfigureAwait(false);

		var cellIndex = unchecked(++ _tailIndex);

		try
		{
			_store(_bucket.Nested(cellIndex), item);
			_bucket.Add(Keys.Tail, _tailIndex);

			await _storage.CheckPoint(0).ConfigureAwait(false);
		}
		catch
		{
			// Rollback tail index if write failed
			_bucket.Add(Keys.Tail, unchecked(-- _tailIndex));
			_bucket.RemoveSubtree(cellIndex);

			throw;
		}
		finally
		{
			_lock.Release();
		}
	}

	private async ValueTask TryDequeue(CancellationToken token)
	{
		await _lock.WaitAsync(token).ConfigureAwait(false);

		var cellIndex = unchecked(++ _headIndex);
		T? item = null;

		try
		{
			item = _restore(_bucket.Nested(cellIndex));

			_bucket.Add(Keys.Head, _headIndex);
			_bucket.RemoveSubtree(cellIndex);

			await _storage.CheckPoint(0).ConfigureAwait(false);

			if (Interlocked.CompareExchange(ref _nextItem, item, comparand: null) is not null)
			{
				_bucket.Add(Keys.Head, unchecked(-- _headIndex));
				_store(_bucket.Nested(cellIndex), item);

				await _storage.CheckPoint(0).ConfigureAwait(false);
			}
		}
		catch
		{
			// Rollback head index if read failed
			_bucket.Add(Keys.Head, unchecked(-- _headIndex));

			if (item is not null)
			{
				_store(_bucket.Nested(cellIndex), item);
			}

			throw;
		}
		finally
		{
			_lock.Release();
		}
	}

	private bool TryComplete(Exception? error)
	{
		if (Interlocked.CompareExchange(ref _completed, error ?? PersistentChannelHelper.SentinelCompleted, comparand: null) is not null)
		{
			return false;
		}

		_events.Release();

		CheckCompletedAndDrained(false);

		return true;
	}

	private bool CheckCompletedAndDrained(bool throwIfError = true)
	{
		if (_completed is null)
		{
			return false;
		}

		if (Count > 0)
		{
			return false;
		}

		if (_completed == PersistentChannelHelper.SentinelCompleted)
		{
			_completion.TrySetResult();

			return true;
		}

		if (_completed is OperationCanceledException ex)
		{
			_completion.TrySetCanceled(ex.CancellationToken);
		}
		else
		{
			_completion.TrySetException(_completed);
		}

		return !throwIfError ? true : throw _completed;
	}

	private enum Keys
	{
		Head,

		Tail
	}

	private class ChannelReader(PersistentChannel<T> persistentChannel) : ChannelReader<T>
	{
		public override bool CanCount => true;

		public override Task Completion => persistentChannel._completion.Task;

		public override int Count => persistentChannel.Count;

		public override bool TryRead([MaybeNullWhen(false)] out T item) => persistentChannel.TryRead(out item);

		public override ValueTask<bool> WaitToReadAsync(CancellationToken token = default) => persistentChannel.WaitToReadAsync(token);
	}

	private class ChannelWriter(PersistentChannel<T> persistentChannel) : ChannelWriter<T>
	{
		public override bool TryWrite(T item) => persistentChannel.TryWrite(item);

		public override bool TryComplete(Exception? error = null) => persistentChannel.TryComplete(error);

		public override ValueTask WriteAsync(T item, CancellationToken token = default) => persistentChannel.WriteAsync(item, token);

		public override ValueTask<bool> WaitToWriteAsync(CancellationToken token = default) => persistentChannel.WaitToWriteAsync(token);
	}
}