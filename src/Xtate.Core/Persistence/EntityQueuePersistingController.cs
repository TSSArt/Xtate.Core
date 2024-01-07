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

namespace Xtate.Persistence;

internal sealed class EntityQueuePersistingController<T> : IDisposable where T : class
{
	private readonly Bucket         _bucket;
	private readonly EntityQueue<T> _entityQueue;
	private          int            _headIndex;
	private          int            _tailIndex;

	public EntityQueuePersistingController(in Bucket bucket, EntityQueue<T> entityQueue, Func<Bucket, T> creator)
	{
		if (creator is null) throw new ArgumentNullException(nameof(creator));

		_bucket = bucket;
		_entityQueue = entityQueue ?? throw new ArgumentNullException(nameof(entityQueue));

		bucket.TryGet(Key.Head, out _headIndex);
		bucket.TryGet(Key.Tail, out _tailIndex);

		for (var i = _headIndex; i < _tailIndex; i ++)
		{
			entityQueue.Enqueue(creator(bucket.Nested(i)));
		}

		entityQueue.Changed += OnChanged;
	}

#region Interface IDisposable

	public void Dispose()
	{
		_entityQueue.Changed -= OnChanged;
	}

#endregion

	private void OnChanged(EntityQueue<T>.ChangedAction action, T? entity)
	{
		switch (action)
		{
			case EntityQueue<T>.ChangedAction.Enqueue:
				var bucket = _bucket.Nested(_tailIndex ++);
				_bucket.Add(Key.Tail, _tailIndex);
				entity!.As<IStoreSupport>().Store(bucket);
				break;

			case EntityQueue<T>.ChangedAction.Dequeue:
				if (_entityQueue.Count > 1)
				{
					_bucket.RemoveSubtree(_headIndex ++);
					_bucket.Add(Key.Head, _headIndex);
				}
				else
				{
					_bucket.RemoveSubtree(Bucket.RootKey);
					_headIndex = _tailIndex = 0;
				}

				break;

			default:
				throw Infra.Unexpected<Exception>(action);
		}
	}

	private enum Key
	{
		Head,
		Tail
	}
}