﻿// Copyright © 2019-2024 Sergii Artemenko
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

using System.Diagnostics;
using System.Text;

namespace Xtate;

[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay(value: "Count = {" + nameof(Count) + "}")]
public partial class DataModelList : ISpanFormattable
{
#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider) => IsArray() ? ToStringAsArray(formatProvider) : ToStringAsObject(formatProvider);

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? formatProvider) =>
		IsArray() ? TryFormatAsArray(destination, out charsWritten, formatProvider) : TryFormatAsObject(destination, out charsWritten, formatProvider);

#endregion

	private bool IsArray() => Count > 0 && !HasKeys;

	private string ToStringAsObject(IFormatProvider? formatProvider)
	{
		if (_count == 0)
		{
			return @"()";
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

	private bool TryFormatAsObject(Span<char> destination, out int charsWritten, IFormatProvider? formatProvider)
	{
		var addDelimiter = false;
		charsWritten = 0;

		if (!'('.TryCopyIncremental(ref destination, ref charsWritten)) return false;

		foreach (var keyValue in KeyValues)
		{
			if (addDelimiter)
			{
				if (!','.TryCopyIncremental(ref destination, ref charsWritten)) return false;
			}
			else
			{
				addDelimiter = true;
			}

			if (!keyValue.Key.TryCopyIncremental(ref destination, ref charsWritten)) return false;

			if (!'='.TryCopyIncremental(ref destination, ref charsWritten)) return false;

			if (!keyValue.Value.TryFormat(destination, out var valCharsWritten, format: default, formatProvider)) return false;

			destination = destination[valCharsWritten..];
			charsWritten += valCharsWritten;
		}

		if (!')'.TryCopyIncremental(ref destination, ref charsWritten)) return false;

		return true;
	}

	private string ToStringAsArray(IFormatProvider? formatProvider)
	{
		if (_count == 0)
		{
			return @"[]";
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

	private bool TryFormatAsArray(Span<char> destination, out int charsWritten, IFormatProvider? formatProvider)
	{
		var addDelimiter = false;
		charsWritten = 0;

		if (!'['.TryCopyIncremental(ref destination, ref charsWritten)) return false;

		foreach (var value in Values)
		{
			if (addDelimiter)
			{
				if (!','.TryCopyIncremental(ref destination, ref charsWritten)) return false;
			}
			else
			{
				addDelimiter = true;
			}

			if (!value.TryFormat(destination, out var valCharsWritten, format: default, formatProvider)) return false;

			destination = destination[valCharsWritten..];
			charsWritten += valCharsWritten;
		}

		if (!']'.TryCopyIncremental(ref destination, ref charsWritten)) return false;

		return true;
	}

	public override string ToString() => ToString(format: null, formatProvider: null);

	[ExcludeFromCodeCoverage]
	[DebuggerDisplay(value: "{" + nameof(Value) + "}", Name = "{" + nameof(IndexKey) + ",nq}")]
	private readonly struct DebugIndexKeyValue(in Entry entry)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Entry _entry = entry;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private DataModelValue Value => _entry.Value;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string IndexKey => _entry.Key ?? @"[" + _entry.Index + @"]";

		[UsedImplicitly]
		[SuppressMessage(category: "ReSharper", checkId: "InconsistentNaming")]
		[SuppressMessage(category: "Style", checkId: "IDE1006:Naming Styles")]
		public ItemInfo __ItemInfo__ => new(_entry);
	}

	[ExcludeFromCodeCoverage]
	[DebuggerDisplay(value: "Index = {" + nameof(Index) + "}, Access = {" + nameof(Access) + "}, Metadata = {" + nameof(MetadataNote) + ",nq}")]
	private readonly struct ItemInfo(in Entry entry)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Entry _entry = entry;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int Index => _entry.Index;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private DataModelAccess Access => _entry.Access;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string MetadataNote => _entry.Metadata is not null ? @"{...}" : @"null";

		[UsedImplicitly]
		public DataModelList? Metadata => _entry.Metadata;
	}

	[ExcludeFromCodeCoverage]
	[DebuggerDisplay(value: "Access = {" + nameof(Access) + "}, Metadata = {" + nameof(MetadataNote) + ",nq}")]
	private readonly struct ListInfo(DataModelList dataModelList)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly DataModelList _dataModelList = dataModelList;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string MetadataNote => _dataModelList.GetMetadata() is not null ? @"{...}" : @"null";

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private DataModelAccess Access => _dataModelList.Access;

		[UsedImplicitly]
		public DataModelList? Metadata => _dataModelList.GetMetadata();
	}

	[ExcludeFromCodeCoverage]
	private class DebugView(DataModelList dataModelList)
	{
		[UsedImplicitly]
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebugIndexKeyValue[] Items => dataModelList.Entries.Select(entry => new DebugIndexKeyValue(entry)).ToArray();

		[UsedImplicitly]
		[SuppressMessage(category: "ReSharper", checkId: "InconsistentNaming")]
		[SuppressMessage(category: "Style", checkId: "IDE1006:Naming Styles")]
		public ListInfo __ListInfo__ => new(dataModelList);
	}
}