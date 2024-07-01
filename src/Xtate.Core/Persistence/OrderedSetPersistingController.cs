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

internal sealed class OrderedSetPersistingController<T> : IDisposable where T : class, IDocumentId
{
	private const int DocumentId = 0;
	private const int Operation  = 1;
	private const int Added      = 2;
	private const int Deleted    = 3;


	private readonly Bucket        _bucket;
	private readonly OrderedSet<T> _orderedSet;
	private          int           _record;

	public OrderedSetPersistingController(in Bucket bucket, OrderedSet<T> orderedSet, ImmutableDictionary<int, IEntity> entityMap)
	{
		if (entityMap is null) throw new ArgumentNullException(nameof(entityMap));
		_bucket = bucket;
		_orderedSet = orderedSet ?? throw new ArgumentNullException(nameof(orderedSet));

		var shrink = !orderedSet.IsEmpty;
		while (true)
		{
			var recordBucket = bucket.Nested(_record);

			if (!recordBucket.TryGet(Operation, out int operation) ||
				!recordBucket.TryGet(DocumentId, out int documentId))
			{
				break;
			}

			switch (operation)
			{
				case Added:
					orderedSet.Add(entityMap[documentId].As<T>());
					break;

				case Deleted:
					orderedSet.Delete(entityMap[documentId].As<T>());
					shrink = true;
					break;
			}

			_record ++;
		}

		if (shrink)
		{
			bucket.RemoveSubtree(Bucket.RootKey);

			_record = 0;
			foreach (var entity in orderedSet)
			{
				var recordBucket = bucket.Nested(_record ++);
				recordBucket.Add(DocumentId, entity.As<IDocumentId>().DocumentId);
				recordBucket.Add(Operation, Added);
			}
		}

		orderedSet.Changed += OnChanged;
	}

#region Interface IDisposable

	public void Dispose()
	{
		_orderedSet.Changed -= OnChanged;
	}

#endregion

	private void OnChanged(OrderedSet<T>.ChangedAction action, T? item)
	{
		switch (action)
		{
			case OrderedSet<T>.ChangedAction.Add:
			{
				var bucket = _bucket.Nested(_record ++);
				bucket.Add(DocumentId, item!.As<IDocumentId>().DocumentId);
				bucket.Add(Operation, Added);
				break;
			}

			case OrderedSet<T>.ChangedAction.Clear:
				_record = 0;
				_bucket.RemoveSubtree(Bucket.RootKey);
				break;

			case OrderedSet<T>.ChangedAction.Delete:
				if (_orderedSet.IsEmpty)
				{
					_record = 0;
					_bucket.RemoveSubtree(Bucket.RootKey);
				}
				else
				{
					var bucket = _bucket.Nested(_record ++);
					bucket.Add(DocumentId, item!.As<IDocumentId>().DocumentId);
					bucket.Add(Operation, Deleted);
				}

				break;

			default:
				throw Infra.Unexpected<Exception>(action);
		}
	}
}