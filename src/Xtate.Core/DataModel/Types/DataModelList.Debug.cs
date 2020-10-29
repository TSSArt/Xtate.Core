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
	[DebuggerTypeProxy(typeof(DebugView))]
	[DebuggerDisplay(value: "Count = {" + nameof(Count) + "}")]
	public partial class DataModelList : IFormattable
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
			if (Count == 0)
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
			if (Count == 0)
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
			public string? Key => _entry.Key;

			[UsedImplicitly]
			public int Index => _entry.Index;

			[UsedImplicitly]
			public DataModelList? Metadata => _entry.Metadata;

			[UsedImplicitly]
			public DataModelAccess Access => _entry.Access;
		}

		[ExcludeFromCodeCoverage]
		private class DebugView
		{
			private readonly DataModelList _dataModelList;

			public DebugView(DataModelList dataModelList) => _dataModelList = dataModelList;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugIndexKeyValue[] Items => _dataModelList.Entries.Select(entry => new DebugIndexKeyValue(entry)).ToArray();

			public DataModelList? Metadata => _dataModelList.GetMetadata();
		}
	}
}