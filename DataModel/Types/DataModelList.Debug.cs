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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Xtate.Annotations;

namespace Xtate
{
	public partial class DataModelList
	{
	#region Interface IFormattable

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			foreach (var keyValue in KeyValues)
			{
				if (keyValue.Key is not null)
				{
					return ToStringAsObject(formatProvider);
				}
			}

			return ToStringAsArray(formatProvider);
		}

	#endregion

		private string ToStringAsObject(IFormatProvider? formatProvider)
		{
			if (_count == 0)
			{
				return "()";
			}

			var sb = new StringBuilder();
			var addDelimiter = false;

			sb.Append('(');
			foreach (var keyValue in KeyValues)
			{
				if (addDelimiter)
				{
					sb.Append(',');
				}
				else
				{
					addDelimiter = true;
				}

				sb.Append(keyValue.Key).Append('=').Append(keyValue.Value.ToString(format: null, formatProvider));
			}

			sb.Append(')');

			return sb.ToString();
		}

		private string ToStringAsArray(IFormatProvider? formatProvider)
		{
			if (_count == 0)
			{
				return "[]";
			}

			var sb = new StringBuilder();
			var addDelimiter = false;

			sb.Append('[');
			foreach (var value in Values)
			{
				if (addDelimiter)
				{
					sb.Append(',');
				}
				else
				{
					addDelimiter = true;
				}

				sb.Append(value.ToString(format: null, formatProvider));
			}

			sb.Append(']');

			return sb.ToString();
		}

		public override string ToString() => ToString(format: null, formatProvider: null);

		[ExcludeFromCodeCoverage]
		[DebuggerDisplay(value: "{" + nameof(Value) + "}", Name = "{" + nameof(IndexKey) + ",nq}")]
		private readonly struct DebugIndexKeyValue
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly Entry _entry;

			public DebugIndexKeyValue(in Entry entry) => _entry = entry;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			private DataModelValue Value => _entry.Value;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private string IndexKey => _entry.Key ?? "[" + _entry.Index + "]";

			[UsedImplicitly]
			public ItemInfo __ItemInfo__ => new ItemInfo(_entry);
		}

		[ExcludeFromCodeCoverage]
		[DebuggerDisplay(value: "Index = {" + nameof(Index) + "}, Access = {" + nameof(Access) + "}, Metadata = {" + nameof(MetadataNote) + ",nq}")]
		private readonly struct ItemInfo
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly Entry _entry;

			public ItemInfo(in Entry entry) => _entry = entry;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private int Index => _entry.Index;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private DataModelAccess Access => _entry.Access;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private string MetadataNote => _entry.Metadata is not null ? "{...}" : "null";

			[UsedImplicitly]
			public DataModelList? Metadata => _entry.Metadata;
		}

		[ExcludeFromCodeCoverage]
		[DebuggerDisplay(value: "Access = {" + nameof(Access) + "}, Metadata = {" + nameof(MetadataNote) + ",nq}")]
		private readonly struct ListInfo
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly DataModelList _dataModelList;

			public ListInfo(DataModelList dataModelList) => _dataModelList = dataModelList;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private string MetadataNote => _dataModelList.GetMetadata() is not null ? "{...}" : "null";

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private DataModelAccess Access => _dataModelList.Access;

			[UsedImplicitly]
			public DataModelList? Metadata => _dataModelList.GetMetadata();
		}

		[ExcludeFromCodeCoverage]
		private class DebugView
		{
			private readonly DataModelList _dataModelList;

			public DebugView(DataModelList dataModelList) => _dataModelList = dataModelList;

			[UsedImplicitly]
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugIndexKeyValue[] Items => _dataModelList.Entries.Select(entry => new DebugIndexKeyValue(entry)).ToArray();

			[UsedImplicitly]
			public ListInfo __ListInfo__ => new ListInfo(_dataModelList);
		}
	}
}