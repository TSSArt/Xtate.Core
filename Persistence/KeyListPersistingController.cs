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

using System.Buffers.Binary;

namespace Xtate.Persistence;

internal sealed class KeyListPersistingController<T> : IDisposable where T : class
{
	private const int Id     = 0;
	private const int IdList = 1;

	private readonly Bucket               _bucket;
	private readonly KeyList<T>           _keyList;
	private readonly Dictionary<int, int> _records = [];

	public KeyListPersistingController(Bucket bucket, KeyList<T> keyList, ImmutableDictionary<int, IEntity> entityMap)
	{
		if (entityMap is null) throw new ArgumentNullException(nameof(entityMap));
		_bucket = bucket;
		_keyList = keyList ?? throw new ArgumentNullException(nameof(keyList));

		while (true)
		{
			var recordBucket = bucket.Nested(_records.Count);

			if (!recordBucket.TryGet(Id, out int documentId) || !recordBucket.TryGet(IdList, out var bytes))
			{
				break;
			}

			var list = new List<T>(bytes.Length / 4);
			for (var i = 0; i < list.Count; i ++)
			{
				var itemDocumentId = BinaryPrimitives.ReadInt32LittleEndian(bytes[(i * 4)..].Span);
				list.Add(entityMap[itemDocumentId].As<T>());
			}

			_records.Add(documentId, _records.Count);
			keyList.Set(entityMap[documentId], list);
		}

		keyList.Changed += OnChanged;
	}

#region Interface IDisposable

	public void Dispose()
	{
		_keyList.Changed -= OnChanged;
	}

#endregion

	private void OnChanged(KeyList<T>.ChangedAction action, IEntity entity, List<T> list)
	{
		if (action != KeyList<T>.ChangedAction.Set)
		{
			throw new ArgumentOutOfRangeException(nameof(action), action, message: null);
		}

		Span<byte> bytes = stackalloc byte[list.Count * 4];
		for (var i = 0; i < list.Count; i ++)
		{
			BinaryPrimitives.WriteInt32LittleEndian(bytes[(i * 4)..], list[i].As<IDocumentId>().DocumentId);
		}

		var documentId = entity.As<IDocumentId>().DocumentId;
		if (!_records.TryGetValue(documentId, out var record))
		{
			record = _records.Count;
			_records.Add(documentId, record);
			_bucket.Nested(record).Add(Id, record);
		}

		_bucket.Nested(record).Add(IdList, bytes);
	}
}