#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System.Threading.Channels;

namespace Xtate.Persistence;

internal sealed class ChannelPersistingController<T> : Channel<T>, IDisposable
{
	private readonly Channel<T>                          _baseChannel;
	private readonly TaskCompletionSource<int>           _initializedTcs = new();
	private          Bucket                              _bucket;
	private          int                                 _headIndex;
	private          Func<CancellationToken, ValueTask>? _postAction;
	private          SemaphoreSlim?                      _storageLock;
	private          int                                 _tailIndex;

	public ChannelPersistingController(Channel<T> baseChannel)
	{
		_baseChannel = baseChannel;

		Reader = new ChannelReader(this);
		Writer = new ChannelWriter(this);
	}

#region Interface IDisposable

	public void Dispose() => _initializedTcs.TrySetResult(0);

#endregion

	public void Initialize(Bucket bucket,
						   Func<Bucket, T> creator,
						   SemaphoreSlim storageLock,
						   Func<CancellationToken, ValueTask> postAction)
	{
		if (creator is null) throw new ArgumentNullException(nameof(creator));

		_bucket = bucket;
		_storageLock = storageLock ?? throw new ArgumentNullException(nameof(storageLock));
		_postAction = postAction ?? throw new ArgumentNullException(nameof(postAction));

		bucket.TryGet(Key.Head, out _headIndex);
		bucket.TryGet(Key.Tail, out _tailIndex);

		for (var i = _headIndex; i < _tailIndex; i ++)
		{
			if (!_baseChannel.Writer.TryWrite(creator(bucket.Nested(i))))
			{
				throw new PersistenceException(Resources.Exception_ChannelCantConsumePreviouslyPersistedObject);
			}
		}

		_initializedTcs.TrySetResult(0);
	}

	private class ChannelReader(ChannelPersistingController<T> parent) : ChannelReader<T>
	{
		public override Task Completion => parent._baseChannel.Reader.Completion;

		public override bool TryRead(out T item) => throw new NotSupportedException(Resources.Exception_UseReadAsyncInstead);

		public override async ValueTask<bool> WaitToReadAsync(CancellationToken token = default)
		{
			await parent._initializedTcs.WaitAsync(token).ConfigureAwait(false);

			await parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
			try
			{
				return await parent._baseChannel.Reader.WaitToReadAsync(token).ConfigureAwait(false);
			}
			finally
			{
				parent._storageLock.Release();
			}
		}

		public override async ValueTask<T> ReadAsync(CancellationToken token = default)
		{
			await parent._initializedTcs.WaitAsync(token).ConfigureAwait(false);

			await parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
			try
			{
				var item = await parent._baseChannel.Reader.ReadAsync(token).ConfigureAwait(false);

				if (parent._tailIndex > parent._headIndex)
				{
					parent._bucket.RemoveSubtree(parent._headIndex ++);
					parent._bucket.Add(Key.Head, parent._headIndex);
				}
				else
				{
					parent._bucket.RemoveSubtree(Bucket.RootKey);
					parent._headIndex = parent._tailIndex = 0;
				}

				await parent._postAction!(token).ConfigureAwait(false);

				return item;
			}
			finally
			{
				parent._storageLock.Release();
			}
		}
	}

	private class ChannelWriter(ChannelPersistingController<T> parent) : ChannelWriter<T>
	{
		public override bool TryComplete(Exception? error = default) => parent._baseChannel.Writer.TryComplete(error);

		public override bool TryWrite(T item) => throw new NotSupportedException(Resources.Exception_UseWriteAsyncInstead);

		public override async ValueTask<bool> WaitToWriteAsync(CancellationToken token = default)
		{
			await parent._initializedTcs.WaitAsync(token).ConfigureAwait(false);

			await parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
			try
			{
				return await parent._baseChannel.Writer.WaitToWriteAsync(token).ConfigureAwait(false);
			}
			finally
			{
				parent._storageLock.Release();
			}
		}

		public override async ValueTask WriteAsync([NotNull] T item, CancellationToken token = default)
		{
			if (item is null) throw new ArgumentNullException(nameof(item));

			await parent._initializedTcs.WaitAsync(token).ConfigureAwait(false);

			await parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
			try
			{
				await parent._baseChannel.Writer.WriteAsync(item, token).ConfigureAwait(false);

				var bucket = parent._bucket.Nested(parent._tailIndex ++);
				parent._bucket.Add(Key.Tail, parent._tailIndex);
				item.As<IStoreSupport>().Store(bucket);

				await parent._postAction!(token).ConfigureAwait(false);
			}
			finally
			{
				parent._storageLock.Release();
			}
		}
	}

	private enum Key
	{
		Head,
		Tail
	}
}