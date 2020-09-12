#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xtate.Persistence
{
	internal sealed class ChannelPersistingController<T> : Channel<T>, IDisposable
	{
		private readonly Channel<T>                          _baseChannel;
		private readonly TaskCompletionSource<int>           _initializedTcs = new TaskCompletionSource<int>();
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

		public void Initialize(Bucket bucket, Func<Bucket, T> creator, SemaphoreSlim storageLock, Func<CancellationToken, ValueTask> postAction)
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
					throw new PersistenceException(Resources.Exception_Channel_can_t_consume_previously_persisted_object);
				}
			}

			_initializedTcs.TrySetResult(0);
		}

		private class ChannelReader : ChannelReader<T>
		{
			private readonly ChannelPersistingController<T> _parent;

			public ChannelReader(ChannelPersistingController<T> parent) => _parent = parent;

			public override Task Completion => _parent._baseChannel.Reader.Completion;

			public override bool TryRead(out T item) => throw new NotSupportedException(Resources.Exception_Use_ReadAsync___instead);

			public override async ValueTask<bool> WaitToReadAsync(CancellationToken token = default)
			{
				await _parent._initializedTcs.Task.WaitAsync(token).ConfigureAwait(false);

				await _parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
				try
				{
					return await _parent._baseChannel.Reader.WaitToReadAsync(token).ConfigureAwait(false);
				}
				finally
				{
					_parent._storageLock.Release();
				}
			}

			public override async ValueTask<T> ReadAsync(CancellationToken token = default)
			{
				await _parent._initializedTcs.Task.WaitAsync(token).ConfigureAwait(false);

				await _parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
				try
				{
					var item = await _parent._baseChannel.Reader.ReadAsync(token).ConfigureAwait(false);

					if (_parent._tailIndex > _parent._headIndex)
					{
						_parent._bucket.RemoveSubtree(_parent._headIndex ++);
						_parent._bucket.Add(Key.Head, _parent._headIndex);
					}
					else
					{
						_parent._bucket.RemoveSubtree(Bucket.RootKey);
						_parent._headIndex = _parent._tailIndex = 0;
					}

					await _parent._postAction!(token).ConfigureAwait(false);

					return item;
				}
				finally
				{
					_parent._storageLock.Release();
				}
			}
		}

		private class ChannelWriter : ChannelWriter<T>
		{
			private readonly ChannelPersistingController<T> _parent;

			public ChannelWriter(ChannelPersistingController<T> parent) => _parent = parent;

			public override bool TryComplete(Exception? error = default) => _parent._baseChannel.Writer.TryComplete(error);

			public override bool TryWrite(T item) => throw new NotSupportedException(Resources.Exception_Use_WriteAsync___instead);

			public override async ValueTask<bool> WaitToWriteAsync(CancellationToken token = default)
			{
				await _parent._initializedTcs.Task.WaitAsync(token).ConfigureAwait(false);

				await _parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
				try
				{
					return await _parent._baseChannel.Writer.WaitToWriteAsync(token).ConfigureAwait(false);
				}
				finally
				{
					_parent._storageLock.Release();
				}
			}

			public override async ValueTask WriteAsync([NotNull] T item, CancellationToken token = default)
			{
				if (item is null) throw new ArgumentNullException(nameof(item));

				await _parent._initializedTcs.Task.WaitAsync(token).ConfigureAwait(false);

				await _parent._storageLock!.WaitAsync(token).ConfigureAwait(false);
				try
				{
					await _parent._baseChannel.Writer.WriteAsync(item, token).ConfigureAwait(false);

					var bucket = _parent._bucket.Nested(_parent._tailIndex ++);
					_parent._bucket.Add(Key.Tail, _parent._tailIndex);
					item.As<IStoreSupport>().Store(bucket);

					await _parent._postAction!(token).ConfigureAwait(false);
				}
				finally
				{
					_parent._storageLock.Release();
				}
			}
		}

		private enum Key
		{
			Head,
			Tail
		}
	}
}