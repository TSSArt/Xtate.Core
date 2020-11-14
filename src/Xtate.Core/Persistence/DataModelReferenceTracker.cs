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
using System.Collections.Generic;

namespace Xtate.Persistence
{
	internal sealed class DataModelReferenceTracker : IDisposable
	{
		private readonly Bucket                           _bucket;
		private readonly Dictionary<DataModelList, Entry> _lists  = new();
		private readonly Dictionary<int, DataModelList>   _refIds = new();

		private bool _disposed;
		private int  _nextRefId;

		public DataModelReferenceTracker(in Bucket bucket)
		{
			_bucket = bucket;
			bucket.TryGet(Bucket.RootKey, out _nextRefId);
		}

	#region Interface IDisposable

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			foreach (var entry in _lists.Values)
			{
				entry.Controller.Dispose();
			}

			_disposed = true;
		}

	#endregion

		public object GetValue(int refId, DataModelValueType type, DataModelList? baseList)
		{
			if (_refIds.TryGetValue(refId, out var list))
			{
				Infrastructure.Assert(baseList is null || baseList == list, Resources.Assertion_ObjectsStructureMismatch);

				return list;
			}

			if (baseList is null)
			{
				return GetValue(refId, type);
			}

			FillList(refId, type, baseList);

			return baseList;
		}

		private void FillList(int refId, DataModelValueType type, DataModelList list)
		{
			var controller = type switch
			{
					DataModelValueType.List => ListControllerCreator(_bucket.Nested(refId), list),
					_ => Infrastructure.UnexpectedValue<DataModelPersistingController>(type)
			};

			_lists[list] = new Entry { RefCount = null, RefId = refId, Controller = controller };
			_refIds[refId] = list;
		}

		private DataModelList GetValue(int refId, DataModelValueType type)
		{
			var bucket = _bucket.Nested(refId);
			bucket.TryGet(Key.Access, out DataModelAccess access);

			switch (type)
			{
				case DataModelValueType.List:
					bucket.TryGet(Key.CaseInsensitive, out bool caseInsensitive);
					var list = new DataModelList(caseInsensitive);
					var listController = ListControllerCreator(bucket, list);
					list.Access = access;
					_lists[list] = new Entry { RefCount = 0, RefId = refId, Controller = listController };
					_refIds[refId] = list;

					return list;

				default: return Infrastructure.UnexpectedValue<DataModelList>(type);
			}
		}

		private int GetRefId(DataModelList list, Func<Bucket, DataModelList, DataModelPersistingController> creator, bool incrementReference)
		{
			if (!_lists.TryGetValue(list, out var entry))
			{
				var refId = _nextRefId ++;
				_bucket.Add(Bucket.RootKey, _nextRefId);
				entry.RefCount = incrementReference ? 1 : 0;
				entry.RefId = refId;
				_refIds[refId] = list;
				entry.Controller = creator(_bucket.Nested(refId), list);
				_lists[list] = entry;
			}
			else if (incrementReference)
			{
				entry.RefCount ++;
				_lists[list] = entry;
			}

			return entry.RefId;
		}

		public int GetRefId(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.List => GetRefId(value.AsList(), ListControllerCreator, incrementReference: false),
						_ => Infrastructure.UnexpectedValue<int>(value.Type)
				};

		public void AddReference(in DataModelValue value)
		{
			switch (value.Type)
			{
				case DataModelValueType.List:
					GetRefId(value.AsList(), ListControllerCreator, incrementReference: true);
					break;
			}
		}

		private DataModelPersistingController ListControllerCreator(Bucket bucket, DataModelList list) => new DataModelListPersistingController(bucket, this, list);

		public void RemoveReference(in DataModelValue value)
		{
			switch (value.Type)
			{
				case DataModelValueType.List:
					Remove(value.AsList());
					break;
			}

			void Remove(DataModelList list)
			{
				if (_lists.TryGetValue(list, out var entry))
				{
					if (-- entry.RefCount == 0)
					{
						entry.Controller.Dispose();
						_bucket.RemoveSubtree(entry.RefId);
						_lists.Remove(list);
						_refIds.Remove(entry.RefId);
					}
					else
					{
						_lists[list] = entry;
					}
				}
			}
		}

		private struct Entry
		{
			public DataModelPersistingController Controller;
			public int?                          RefCount;
			public int                           RefId;
		}
	}
}