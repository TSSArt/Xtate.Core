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
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Xtate.Core;

namespace Xtate
{
	public partial class DataModelList : IDynamicMetaObjectProvider
	{
	#region Interface IDynamicMetaObjectProvider

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this, Dynamic.CreateMetaObject);

	#endregion

		internal class Dynamic : DynamicObject
		{
			private const string GetLength   = @"GetLength";
			private const string GetMetadata = @"GetMetadata";
			private const string SetLength   = @"SetLength";
			private const string SetMetadata = @"SetMetadata";

			private static readonly IDynamicMetaObjectProvider Instance = new Dynamic(default!);

			private static readonly ConstructorInfo ConstructorInfo = typeof(Dynamic).GetConstructor(new[] { typeof(DataModelList) })!;

			private readonly DataModelList _list;

			public Dynamic(DataModelList list) => _list = list;

			public static DynamicMetaObject CreateMetaObject(Expression expression)
			{
				var newExpression = Expression.New(ConstructorInfo, Expression.Convert(expression, typeof(DataModelList)));
				return Instance.GetMetaObject(newExpression);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object? result)
			{
				result = _list[binder.Name, binder.IgnoreCase].ToObject();

				return true;
			}

			public override bool TrySetMember(SetMemberBinder binder, object? value)
			{
				_list[binder.Name, binder.IgnoreCase] = DataModelValue.FromObject(value);

				return true;
			}

			public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
			{
				if (args is null || args.Length == 0)
				{
					if (IsName(GetLength))
					{
						result = _list._count;

						return true;
					}

					if (IsName(GetMetadata))
					{
						result = _list.GetMetadata();

						return true;
					}
				}
				else if (args.Length == 1 && args[0] is string strArg)
				{
					if (IsName(GetMetadata))
					{
						result = _list.TryGet(strArg, binder.IgnoreCase, out var entry) ? entry.Metadata : null;

						return true;
					}
				}
				else if (args.Length == 1 && args[0] is IConvertible cnvArg)
				{
					if (IsName(GetMetadata))
					{
						result = _list.TryGet(cnvArg.ToInt32(CultureInfo.InvariantCulture), out var entry) ? entry.Metadata : null;

						return true;
					}

					if (IsName(SetLength))
					{
						_list.SetLength(cnvArg.ToInt32(CultureInfo.InvariantCulture));

						result = null;

						return true;
					}
				}
				else if (args.Length == 2 && args[0] is string str1Arg)
				{
					if (IsName(SetMetadata))
					{
						if (_list.TryGet(str1Arg, binder.IgnoreCase, out var entry))
						{
							_list.Set(entry.Index, entry.Key, entry.Value, (DataModelList?) args[1]);
						}
						else
						{
							_list.Add(str1Arg, value: default, (DataModelList?) args[1]);
						}

						result = null;

						return true;
					}
				}
				else if (args.Length == 2 && args[0] is IConvertible cnv1Arg)
				{
					if (IsName(SetMetadata))
					{
						var index = cnv1Arg.ToInt32(CultureInfo.InvariantCulture);
						if (_list.TryGet(index, out var entry))
						{
							_list.Set(entry.Index, entry.Key, entry.Value, (DataModelList?) args[1]);
						}
						else
						{
							_list.Set(entry.Index, key: default, value: default, (DataModelList?) args[1]);
						}

						result = null;

						return true;
					}
				}

				result = null;

				return false;

				bool IsName(string name) => string.Equals(binder.Name, name, binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
			}

			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
			{
				var arg = indexes.Length == 1 ? indexes[0] : null;

				switch (arg)
				{
					case string key:
						result = _list[key].ToObject();

						return true;

					case IConvertible convertible:
						result = _list[convertible.ToInt32(NumberFormatInfo.InvariantInfo)].ToObject();

						return true;

					default:
						result = null;

						return false;
				}
			}

			public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
			{
				var arg = indexes.Length == 1 ? indexes[0] : null;

				switch (arg)
				{
					case string key:
						_list[key] = DataModelValue.FromObject(value);

						return true;

					case IConvertible convertible:
						_list[convertible.ToInt32(NumberFormatInfo.InvariantInfo)] = DataModelValue.FromObject(value);

						return true;

					default:
						return false;
				}
			}

			public override bool TryConvert(ConvertBinder binder, out object? result)
			{
				if (binder.Type == typeof(DataModelList))
				{
					result = _list;

					return true;
				}

				if (binder.Type == typeof(DataModelValue))
				{
					result = new DataModelValue(_list);

					return true;
				}

				result = null;

				return false;
			}
		}
	}
}