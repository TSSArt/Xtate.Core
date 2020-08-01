#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Collections.Generic;

namespace Xtate.Persistence
{
	internal sealed class DataModelReferenceTracker : IDisposable
	{
		private readonly Bucket _bucket;

		private readonly Dictionary<object, Entry> _objects = new Dictionary<object, Entry>();
		private readonly Dictionary<int, object>   _refIds  = new Dictionary<int, object>();
		private          bool                      _disposed;

		private int _nextRefId;

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

			foreach (var entry in _objects.Values)
			{
				entry.Controller.Dispose();
			}

			_disposed = true;
		}

	#endregion

		public object GetValue(int refId, DataModelValueType type, object? baseObject)
		{
			if (_refIds.TryGetValue(refId, out var obj))
			{
				Infrastructure.Assert(baseObject == null || baseObject == obj, Resources.Assertion_ObjectsStructureMismatch);

				return obj;
			}

			if (baseObject == null)
			{
				return GetValue(refId, type);
			}

			FillObject(refId, type, baseObject);

			return baseObject;
		}

		private void FillObject(int refId, DataModelValueType type, object obj)
		{
			var controller = type switch
			{
					DataModelValueType.Object => ObjectControllerCreator(_bucket.Nested(refId), obj),
					DataModelValueType.Array => ArrayControllerCreator(_bucket.Nested(refId), obj),
					_ => Infrastructure.UnexpectedValue<DataModelPersistingController>()
			};

			_objects[obj] = new Entry { RefCount = null, RefId = refId, Controller = controller };
			_refIds[refId] = obj;
		}

		private object GetValue(int refId, DataModelValueType type)
		{
			var bucket = _bucket.Nested(refId);
			bucket.TryGet(Key.Access, out DataModelAccess access);

			switch (type)
			{
				case DataModelValueType.Object:
					bucket.TryGet(Key.CaseInsensitive, out bool caseInsensitive);
					var obj = new DataModelObject(caseInsensitive);
					var objController = ObjectControllerCreator(bucket, obj);
					obj.Access = access;
					_objects[obj] = new Entry { RefCount = 0, RefId = refId, Controller = objController };
					_refIds[refId] = obj;

					return obj;

				case DataModelValueType.Array:
					var arr = new DataModelArray();
					var arrController = ArrayControllerCreator(bucket, arr);
					arr.Access = access;
					_objects[arr] = new Entry { RefCount = 0, RefId = refId, Controller = arrController };
					_refIds[refId] = arr;

					return arr;

				default: return Infrastructure.UnexpectedValue<object>();
			}
		}

		private int GetRefId(object obj, Func<Bucket, object, DataModelPersistingController> creator, bool incrementReference)
		{
			if (!_objects.TryGetValue(obj, out var entry))
			{
				var refId = _nextRefId ++;
				_bucket.Add(Bucket.RootKey, _nextRefId);
				entry.RefCount = incrementReference ? 1 : 0;
				entry.RefId = refId;
				_refIds[refId] = obj;
				entry.Controller = creator(_bucket.Nested(refId), obj);
				_objects[obj] = entry;
			}
			else if (incrementReference)
			{
				entry.RefCount ++;
				_objects[obj] = entry;
			}

			return entry.RefId;
		}

		public int GetRefId(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Object => GetRefId(value.AsObject(), ObjectControllerCreator, incrementReference: false),
						DataModelValueType.Array => GetRefId(value.AsArray(), ArrayControllerCreator, incrementReference: false),
						_ => Infrastructure.UnexpectedValue<int>()
				};

		public void AddReference(in DataModelValue value)
		{
			switch (value.Type)
			{
				case DataModelValueType.Object:
					GetRefId(value.AsObject(), ObjectControllerCreator, incrementReference: true);
					break;
				case DataModelValueType.Array:
					GetRefId(value.AsArray(), ArrayControllerCreator, incrementReference: true);
					break;
			}
		}

		private DataModelPersistingController ObjectControllerCreator(Bucket bucket, object obj) => new DataModelListPersistingController(bucket, this, (DataModelObject) obj);
		private DataModelPersistingController ArrayControllerCreator(Bucket bucket, object obj)  => new DataModelListPersistingController(bucket, this, (DataModelArray) obj);

		public void RemoveReference(in DataModelValue value)
		{
			switch (value.Type)
			{
				case DataModelValueType.Object:
					Remove(value.AsObject());
					break;
				case DataModelValueType.Array:
					Remove(value.AsArray());
					break;
			}

			void Remove(object obj)
			{
				if (_objects.TryGetValue(obj, out var entry))
				{
					if (-- entry.RefCount == 0)
					{
						entry.Controller.Dispose();
						_bucket.RemoveSubtree(entry.RefId);
						_objects.Remove(obj);
						_refIds.Remove(entry.RefId);
					}
					else
					{
						_objects[obj] = entry;
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