#region Copyright © 2019-2021 Sergii Artemenko

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
using System.ComponentModel;

namespace Xtate
{
	public partial class DataModelList
	{
		public DataModelValue this[[Localizable(false)] string key]
		{
			get
			{
				TryGet(key, CaseInsensitive, out var entry);

				return entry.Value;
			}

			set => Set(key, CaseInsensitive, value, metadata: default);
		}

		public DataModelValue this[[Localizable(false)] string key, bool caseInsensitive]
		{
			get
			{
				TryGet(key, caseInsensitive, out var entry);

				return entry.Value;
			}

			set => Set(key, caseInsensitive, value, metadata: default);
		}

		public void Add([Localizable(false)] string key, DataModelValue value)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			Add(key, value, metadata: default);
		}

		public bool ContainsKey([Localizable(false)] string key) => TryGet(key, CaseInsensitive, out _);

		public bool ContainsKey([Localizable(false)] string key, bool caseInsensitive) => TryGet(key, caseInsensitive, out _);

		public bool RemoveFirst([Localizable(false)] string key) => RemoveFirst(key, CaseInsensitive);

		public bool RemoveFirst([Localizable(false)] string key, bool caseInsensitive)
		{
			if (TryGet(key, caseInsensitive, out var entry))
			{
				if (CanRemove(entry.Index))
				{
					Remove(entry.Index);
				}
				else
				{
					Set(entry.Index, key: default, value: default, metadata: default);
				}

				return true;
			}

			return false;
		}

		public bool RemoveAll([Localizable(false)] string key) => RemoveAll(key, CaseInsensitive);

		public bool RemoveAll([Localizable(false)] string key, bool caseInsensitive)
		{
			var enumerator = ListEntries(key, caseInsensitive).GetEnumerator();

			try
			{
				return RemoveNext(ref enumerator);
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		private bool RemoveNext(ref EntryByKeyEnumerator enumerator)
		{
			if (!enumerator.MoveNext())
			{
				return false;
			}

			var index = enumerator.Current.Index;

			RemoveNext(ref enumerator);

			if (CanRemove(index))
			{
				Remove(index);
			}
			else
			{
				Set(index, key: default, value: default, metadata: default);
			}

			return true;
		}
	}
}