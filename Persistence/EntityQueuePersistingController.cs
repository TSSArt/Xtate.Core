using System;

namespace Xtate.Persistence
{
	internal sealed class EntityQueuePersistingController<T> : IDisposable where T : class
	{
		private readonly Bucket         _bucket;
		private readonly EntityQueue<T> _entityQueue;
		private          int            _headIndex;
		private          int            _tailIndex;

		public EntityQueuePersistingController(Bucket bucket, EntityQueue<T> entityQueue, Func<Bucket, T> creator)
		{
			if (creator == null) throw new ArgumentNullException(nameof(creator));

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

		private void OnChanged(EntityQueue<T>.ChangedAction action, T entity)
		{
			switch (action)
			{
				case EntityQueue<T>.ChangedAction.Enqueue:
					var bucket = _bucket.Nested(_tailIndex ++);
					_bucket.Add(Key.Tail, _tailIndex);
					entity.As<IStoreSupport>().Store(bucket);
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

				default: throw new ArgumentOutOfRangeException(nameof(action), action, message: null);
			}
		}

		private enum Key
		{
			Head,
			Tail
		}
	}
}