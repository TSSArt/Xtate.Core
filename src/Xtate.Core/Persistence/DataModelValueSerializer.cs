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

public static class DataModelValueSerializer
{
	public static void Save(IStorage storage, string key, in DataModelValue value)
	{
		var bucket = new Bucket(storage).Nested(key);
		using var tracker = new DataModelReferenceTracker(bucket.Nested(Key.DataReferences));
		bucket.SetDataModelValue(tracker, value);
	}

	public static DataModelValue Load(IStorage storage, string key)
	{
		var bucket = new Bucket(storage).Nested(key);
		using var tracker = new DataModelReferenceTracker(bucket.Nested(Key.DataReferences));

		return bucket.GetDataModelValue(tracker, baseValue: default);
	}
}