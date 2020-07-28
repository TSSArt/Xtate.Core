#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

namespace Xtate.Persistence
{
	internal sealed class DataModelListPersistingController : DataModelPersistingController
	{
		private readonly Bucket                    _bucket;
		private readonly DataModelList             _list;
		private readonly DataModelReferenceTracker _referenceTracker;
		private          int                       _record;
		private          bool                      _shrink;

		public DataModelListPersistingController(Bucket bucket, DataModelReferenceTracker referenceTracker, DataModelList dataModelList)
		{
			_bucket = bucket;
			_referenceTracker = referenceTracker ?? throw new ArgumentNullException(nameof(referenceTracker));
			_list = dataModelList ?? throw new ArgumentNullException(nameof(dataModelList));

			dataModelList.Change += OnRestoreChange;
			Restore();
			dataModelList.Change -= OnRestoreChange;

			if (_shrink)
			{
				Shrink();
			}

			dataModelList.Change += OnChange;
		}

		private void OnRestoreChange(DataModelList.ChangeAction action, in DataModelList.Entry entry)
		{
			switch (action)
			{
				case DataModelList.ChangeAction.RemoveAt:
				case DataModelList.ChangeAction.Reset:
				{
					if (!entry.Value.IsUndefined() || entry.Metadata != null)
					{
						_shrink = true;
					}

					RemoveReferences(entry);
					break;
				}
				case DataModelList.ChangeAction.Append:
				case DataModelList.ChangeAction.SetAt:
				case DataModelList.ChangeAction.InsertAt:
				case DataModelList.ChangeAction.SetCsKey:
				case DataModelList.ChangeAction.SetCiKey:
					AddReferences(entry);
					break;

				case DataModelList.ChangeAction.SetCount:
					var length = entry.Index;
					_shrink = length < _list.Count;

					foreach (var item in _list.Entries)
					{
						if (item.Index >= length)
						{
							RemoveReferences(item);
						}
					}

					break;

				case DataModelList.ChangeAction.SetMetadata: break;
				default:
					Infrastructure.UnexpectedValue();
					break;
			}
		}

		private void Restore()
		{
			if (_list.Count > 0)
			{
				_shrink = true;
			}

			while (true)
			{
				var recordBucket = _bucket.Nested(_record);

				if (!recordBucket.TryGet(Key.Operation, out Key operation))
				{
					break;
				}

				switch (operation)
				{
					case Key.SetCsKey:
					case Key.SetCiKey:
					{
						var caseInsensitive = operation == Key.SetCiKey;
						recordBucket.TryGet(Key.Access, out DataModelAccess access);
						var key = recordBucket.GetString(Key.Key);

						Infrastructure.Assert(key != null);

						_list.TryGet(key, caseInsensitive, out var baseEntry);

						var dataModelValue = recordBucket.GetDataModelValue(_referenceTracker, baseEntry.Value);
						var metadata = recordBucket.Nested(Key.Metadata).GetDataModelValue(_referenceTracker, baseEntry.Metadata).AsListOrDefault();

						_list.SetInternal(key, caseInsensitive, dataModelValue, access, metadata, throwOnDeny: false);
						break;
					}
					case Key.Append:
					{
						recordBucket.TryGet(Key.Access, out DataModelAccess access);
						var key = recordBucket.GetString(Key.Key);

						var dataModelValue = recordBucket.GetDataModelValue(_referenceTracker, baseValue: default);
						var metadata = recordBucket.Nested(Key.Metadata).GetDataModelValue(_referenceTracker, baseValue: default).AsListOrDefault();

						_list.AddInternal(key, dataModelValue, access, metadata, throwOnDeny: false);
						break;
					}
					case Key.Set:
					{
						var index = recordBucket.GetInt32(Key.Index);
						recordBucket.TryGet(Key.Access, out DataModelAccess access);
						var key = recordBucket.GetString(Key.Key);

						_list.TryGet(index, out var baseEntry);

						var dataModelValue = recordBucket.GetDataModelValue(_referenceTracker, baseEntry.Value);
						var metadata = recordBucket.Nested(Key.Metadata).GetDataModelValue(_referenceTracker, baseEntry.Metadata).AsListOrDefault();

						_list.SetInternal(index, key, dataModelValue, access, metadata, throwOnDeny: false);
						break;
					}

					case Key.Insert:
					{
						var index = recordBucket.GetInt32(Key.Index);
						recordBucket.TryGet(Key.Access, out DataModelAccess access);
						var key = recordBucket.GetString(Key.Key);

						var dataModelValue = recordBucket.GetDataModelValue(_referenceTracker, baseValue: default);
						var metadata = recordBucket.Nested(Key.Metadata).GetDataModelValue(_referenceTracker, baseValue: default).AsListOrDefault();

						_list.InsertInternal(index, key, dataModelValue, access, metadata, throwOnDeny: false);
						break;
					}

					case Key.Remove:
					{
						var index = recordBucket.GetInt32(Key.Index);
						_list.RemoveInternal(index, throwOnDeny: false);
						break;
					}

					case Key.SetLength:
					{
						var length = recordBucket.GetInt32(Key.Index);
						_list.SetLengthInternal(length, throwOnDeny: false);
						break;
					}
					case Key.SetMetadata:
					{
						var metadata = recordBucket.Nested(Key.Metadata).GetDataModelValue(_referenceTracker, _list.GetMetadata()).AsListOrDefault();
						_list.SetMetadataInternal(metadata, throwOnDeny: false);
						break;
					}
					default:
						Infrastructure.UnexpectedValue();
						break;
				}

				_record ++;
			}
		}

		private void Shrink()
		{
			_bucket.RemoveSubtree(Bucket.RootKey);

			if (_list.Access != DataModelAccess.Writable)
			{
				_bucket.Add(Key.Access, _list.Access);
			}

			if (_list.CaseInsensitive)
			{
				_bucket.Add(Key.CaseInsensitive, value: true);
			}

			_record = 0;

			var metadata = _list.GetMetadata();
			if (metadata != null)
			{
				var recordBucket = _bucket.Nested(_record ++);
				recordBucket.Add(Key.Operation, Key.SetMetadata);
				var entry = new DataModelList.Entry(metadata);
				AddEntry(ref recordBucket, entry);
			}

			if (_list.Access != DataModelAccess.Writable)
			{
				foreach (var entry in _list.Entries)
				{
					Set(entry);
				}
			}
			else if (!_list.HasItemAccess)
			{
				foreach (var entry in _list.Entries)
				{
					Append(entry);
				}
			}
			else
			{
				foreach (var entry in _list.Entries)
				{
					if (entry.Access != DataModelAccess.Writable)
					{
						Set(entry);
					}
				}

				foreach (var entry in _list.Entries)
				{
					if (entry.Access == DataModelAccess.Writable)
					{
						Append(entry);
					}
				}
			}

			void Set(in DataModelList.Entry entry)
			{
				var recordBucket = _bucket.Nested(_record ++);
				recordBucket.Add(Key.Operation, Key.Set);
				recordBucket.Add(Key.Index, entry.Index);
				AddEntry(ref recordBucket, entry);
			}

			void Append(in DataModelList.Entry entry)
			{
				var recordBucket = _bucket.Nested(_record ++);
				recordBucket.Add(Key.Operation, Key.Append);
				AddEntry(ref recordBucket, entry);
			}
		}

		private void OnChange(DataModelList.ChangeAction action, in DataModelList.Entry entry)
		{
			switch (action)
			{
				case DataModelList.ChangeAction.Reset:
					RemoveReferences(entry);
					break;

				case DataModelList.ChangeAction.Append:
				{
					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Operation, Key.Append);
					AddEntry(ref recordBucket, entry);
					AddReferences(entry);
					break;
				}
				case DataModelList.ChangeAction.SetCsKey:
				{
					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Operation, Key.SetCsKey);
					AddEntry(ref recordBucket, entry);
					AddReferences(entry);
					break;
				}
				case DataModelList.ChangeAction.SetCiKey:
				{
					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Operation, Key.SetCiKey);
					AddEntry(ref recordBucket, entry);
					AddReferences(entry);
					break;
				}
				case DataModelList.ChangeAction.SetAt:
				{
					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Index, entry.Index);
					recordBucket.Add(Key.Operation, Key.Set);
					AddEntry(ref recordBucket, entry);
					AddReferences(entry);
					break;
				}
				case DataModelList.ChangeAction.InsertAt:
				{
					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Index, entry.Index);
					recordBucket.Add(Key.Operation, Key.Insert);
					AddEntry(ref recordBucket, entry);
					AddReferences(entry);
					break;
				}
				case DataModelList.ChangeAction.RemoveAt:
				{
					RemoveReferences(entry);

					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Operation, Key.Remove);
					recordBucket.Add(Key.Index, entry.Index);

					break;
				}
				case DataModelList.ChangeAction.SetCount:
				{
					if (entry.Index < _list.Count)
					{
						foreach (var e in _list.Entries)
						{
							if (e.Index >= entry.Index)
							{
								RemoveReferences(e);
							}
						}
					}

					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Operation, Key.SetLength);
					recordBucket.Add(Key.Index, entry.Index);

					break;
				}
				case DataModelList.ChangeAction.SetMetadata:
				{
					RemoveReferences(new DataModelList.Entry(_list.GetMetadata()));

					var recordBucket = _bucket.Nested(_record ++);
					recordBucket.Add(Key.Operation, Key.SetMetadata);
					AddEntry(ref recordBucket, entry);
					AddReferences(entry);

					break;
				}

				default: throw new ArgumentOutOfRangeException(nameof(action), action, message: null);
			}
		}

		private void AddEntry(ref Bucket bucket, in DataModelList.Entry entry)
		{
			bucket.Add(Key.Key, entry.Key);

			if (entry.Access != DataModelAccess.Writable)
			{
				bucket.Add(Key.Access, entry.Access);
			}

			if (entry.Metadata != null)
			{
				bucket.Nested(Key.Metadata).SetDataModelValue(_referenceTracker, entry.Metadata);
			}

			bucket.SetDataModelValue(_referenceTracker, entry.Value);
		}

		private void AddReferences(in DataModelList.Entry entry)
		{
			_referenceTracker.AddReference(entry.Value);
			_referenceTracker.AddReference(entry.Metadata);
		}

		private void RemoveReferences(in DataModelList.Entry entry)
		{
			_referenceTracker.RemoveReference(entry.Value);
			_referenceTracker.RemoveReference(entry.Metadata);
		}

		public override void Dispose()
		{
			_list.Change -= OnChange;

			base.Dispose();
		}
	}
}