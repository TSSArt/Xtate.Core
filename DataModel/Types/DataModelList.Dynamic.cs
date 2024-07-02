// Copyright © 2019-2024 Sergii Artemenko
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

using System.Dynamic;
using System.Globalization;
using System.Reflection;

namespace Xtate;

public partial class DataModelList : IDynamicMetaObjectProvider
{
#region Interface IDynamicMetaObjectProvider

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this, Dynamic.CreateMetaObject);

#endregion

	internal class Dynamic(DataModelList list) : DynamicObject
	{
		private const string GetLength   = "GetLength";
		private const string GetMetadata = "GetMetadata";
		private const string SetLength   = "SetLength";
		private const string SetMetadata = "SetMetadata";

		private static readonly Dynamic Instance = new(default!);

		private static readonly ConstructorInfo ConstructorInfo = typeof(Dynamic).GetConstructor([typeof(DataModelList)])!;

		public static DynamicMetaObject CreateMetaObject(Expression expression)
		{
			var newExpression = Expression.New(ConstructorInfo, Expression.Convert(expression, typeof(DataModelList)));
			return Instance.GetMetaObject(newExpression);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object? result)
		{
			result = list[binder.Name, binder.IgnoreCase].ToObject();

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object? value)
		{
			list[binder.Name, binder.IgnoreCase] = DataModelValue.FromObject(value);

			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
		{
			switch (args ?? [])
			{
				case [] when IsName(GetLength):
				{
					result = list._count;

					return true;
				}

				case [] when IsName(GetMetadata):
				{
					result = list.GetMetadata();

					return true;
				}

				case [string key] when IsName(GetMetadata):
				{
					result = list.TryGet(key, binder.IgnoreCase, out var entry) ? entry.Metadata : null;

					return true;
				}

				case [IConvertible index] when IsName(GetMetadata):
				{
					result = list.TryGet(index.ToInt32(CultureInfo.InvariantCulture), out var entry) ? entry.Metadata : null;

					return true;
				}

				case [IConvertible index] when IsName(SetLength):
				{
					list.SetLength(index.ToInt32(CultureInfo.InvariantCulture));

					result = default;

					return true;
				}

				case [string key, DataModelList metadata] when IsName(SetMetadata):
				{
					if (list.TryGet(key, binder.IgnoreCase, out var entry))
					{
						list.Set(entry.Index, entry.Key, entry.Value, metadata);
					}
					else
					{
						list.Add(key, value: default, metadata);
					}

					result = default;

					return true;
				}

				case [IConvertible key, DataModelList metadata] when IsName(SetMetadata):
				{
					var index = key.ToInt32(CultureInfo.InvariantCulture);
					if (list.TryGet(index, out var entry))
					{
						list.Set(entry.Index, entry.Key, entry.Value, metadata);
					}
					else
					{
						list.Set(entry.Index, key: default, value: default, metadata);
					}

					result = default;

					return true;
				}

				default:
				{
					result = default;

					return false;
				}
			}

			bool IsName(string name) => string.Equals(binder.Name, name, binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
		{
			switch (indexes.Length == 1 ? indexes[0] : null)
			{
				case string key:
				{
					result = list[key].ToObject();

					return true;
				}

				case IConvertible index:
				{
					result = list[index.ToInt32(NumberFormatInfo.InvariantInfo)].ToObject();

					return true;
				}

				default:
				{
					result = default;

					return false;
				}
			}
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
		{
			switch (indexes.Length == 1 ? indexes[0] : null)
			{
				case string key:
				{
					list[key] = DataModelValue.FromObject(value);

					return true;
				}

				case IConvertible index:
				{
					list[index.ToInt32(NumberFormatInfo.InvariantInfo)] = DataModelValue.FromObject(value);

					return true;
				}

				default:
					return false;
			}
		}

		public override bool TryConvert(ConvertBinder binder, out object? result)
		{
			if (binder.Type == typeof(DataModelList))
			{
				result = list;

				return true;
			}

			if (binder.Type == typeof(DataModelValue))
			{
				result = new DataModelValue(list);

				return true;
			}

			result = default;

			return false;
		}
	}
}