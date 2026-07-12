// Copyright © 2019-2026 Sergii Artemenko
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
using System.Reflection;

namespace Xtate.DataTypes.Internal;

internal class DataModelListMetaObject(Expression expression, DataModelList dataModelList) : MetaObjectBase(expression, dataModelList)
{
	protected override BindingRestrictions SameTypeRestriction() => BindingRestrictions.GetTypeRestriction(Expression, typeof(DataModelList));

	protected override Expression CastToList(Expression expression)
	{
		var result = Expression;

		if (result.Type != typeof(DataModelList))
		{
			result = Expression.Convert(result, typeof(DataModelList));
		}

		return result;
	}
}

internal class DataModelValueMetaObject(Expression expression, DataModelValue dataModelValue) : MetaObjectBase(expression, dataModelValue)
{
	protected override BindingRestrictions SameTypeRestriction() => BindingRestrictions.GetTypeRestriction(Expression, typeof(DataModelValue));

	protected override Expression CastToList(Expression expression)
	{
		var result = Expression;

		if (result.Type != typeof(DataModelValue))
		{
			result = Expression.Convert(result, typeof(DataModelValue));
		}

		return Expression.Convert(result, typeof(DataModelList));
	}

	public override DynamicMetaObject BindConvert(ConvertBinder binder)
	{
		var result = Expression;

		if (result.Type != typeof(DataModelValue))
		{
			result = Expression.Convert(result, typeof(DataModelValue));
		}

		return new DynamicMetaObject(CastResult(result, binder.ReturnType), SameTypeRestriction());
	}
}

internal abstract class MetaObjectBase(Expression expression, object value) : DynamicMetaObject(expression, BindingRestrictions.Empty, value)
{
	private const string GetLength = "GetLength";

	private const string GetMetadata = "GetMetadata";

	private const string SetLength = "SetLength";

	private const string SetMetadata = "SetMetadata";

	private static readonly PropertyInfo ItemKeyProperty;

	private static readonly PropertyInfo ItemKeyCaseProperty;

	private static readonly PropertyInfo ItemIndexProperty;

	private static readonly MethodInfo ToObjectMethod;

	private static readonly MethodInfo SetLengthMethod;

	private static readonly MethodInfo GetMetadataMethod;

	private static readonly PropertyInfo CountProperty;

	private static readonly MethodInfo GetMetadataKeyMethod;

	private static readonly MethodInfo GetMetadataIndexMethod;

	private static readonly MethodInfo SetMetadataKeyMethod;

	private static readonly MethodInfo SetMetadataIndexMethod;

	static MetaObjectBase()
	{
		var listType = typeof(DataModelList);

		ItemKeyProperty = listType.GetProperty(name: @"Item", [typeof(string)])!;
		ItemKeyCaseProperty = listType.GetProperty(name: @"Item", [typeof(string), typeof(bool)])!;
		ItemIndexProperty = listType.GetProperty(name: @"Item", [typeof(int)])!;
		CountProperty = listType.GetProperty(nameof(DataModelList.Count))!;
		SetLengthMethod = listType.GetMethod(nameof(DataModelList.SetLength), [typeof(int)])!;
		GetMetadataMethod = listType.GetMethod(nameof(DataModelList.GetMetadata), [])!;
		GetMetadataKeyMethod = listType.GetMethod(nameof(DataModelList.GetMetadata), [typeof(string)])!;
		GetMetadataIndexMethod = listType.GetMethod(nameof(DataModelList.GetMetadata), [typeof(int)])!;
		SetMetadataKeyMethod = listType.GetMethod(nameof(DataModelList.SetMetadata), [typeof(string), typeof(DataModelList)])!;
		SetMetadataIndexMethod = listType.GetMethod(nameof(DataModelList.SetMetadata), [typeof(int), typeof(DataModelList)])!;

		ToObjectMethod = typeof(DataModelValue).GetMethod(nameof(DataModelValue.ToObject), [])!;
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
	{
		var property = Expression.Property(CastToList(Expression), ItemKeyCaseProperty, Expression.Constant(binder.Name), Expression.Constant(binder.IgnoreCase));

		return new DynamicMetaObject(CastResult(property, binder.ReturnType), SameTypeRestriction());
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
	{
		var property = Expression.Property(CastToList(Expression), ItemKeyCaseProperty, Expression.Constant(binder.Name), Expression.Constant(binder.IgnoreCase));
		var assign = Expression.Assign(property, Expression.Convert(value.Expression, typeof(DataModelValue)));

		return new DynamicMetaObject(CastResult(assign, binder.ReturnType), SameTypeRestriction());
	}

	protected abstract Expression CastToList(Expression expression);

	protected Expression CastResult(Expression expression, Type returnType)
	{
		if (expression.Type == typeof(DataModelValue) && returnType == typeof(object))
		{
			return Expression.Call(expression, ToObjectMethod);
		}

		if (expression.Type == typeof(void))
		{
			return Expression.Block(expression, Expression.Default(returnType));
		}

		return Expression.Convert(expression, returnType);
	}

	public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		Expression? property = null;

		if (indexes.Length == 1)
		{
			var index = indexes[0].Expression;

			var list = CastToList(Expression);

			if (index.Type == typeof(string))
			{
				property = Expression.Property(list, ItemKeyProperty, index);
			}
			else if (index.Type == typeof(int))
			{
				property = Expression.Property(list, ItemIndexProperty, index);
			}
			else
			{
				property = Expression.Property(list, ItemIndexProperty, Expression.Convert(index, typeof(int)));
			}
		}

		if (property is not null)
		{
			return new DynamicMetaObject(CastResult(property, binder.ReturnType), SameTypeRestriction());
		}

		return ThrowWrongIndexCount();
	}

	public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		Expression? property = null;

		if (indexes.Length == 1)
		{
			var arg = indexes[0].Expression;
			var list = CastToList(Expression);

			if (arg.Type == typeof(string))
			{
				property = Expression.Property(list, ItemKeyProperty, arg);
			}
			else if (arg.Type == typeof(int))
			{
				property = Expression.Property(list, ItemIndexProperty, arg);
			}
			else
			{
				property = Expression.Property(list, ItemIndexProperty, Expression.Convert(arg, typeof(int)));
			}
		}

		if (property is not null)
		{
			var assign = Expression.Assign(property, Expression.Convert(value.Expression, typeof(DataModelValue)));

			return new DynamicMetaObject(CastResult(assign, binder.ReturnType), SameTypeRestriction());
		}

		return ThrowWrongIndexCount();
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		var list = CastToList(Expression);

		Expression? result = null;

		if (args.Length == 0 && IsName(GetLength))
		{
			result = Expression.Property(list, CountProperty);
		}
		else if (args.Length == 1 && IsName(SetLength))
		{
			var len = args[0].Expression;

			result = Expression.Call(list, SetLengthMethod, len.Type == typeof(int) ? len : Expression.Convert(len, typeof(int)));
		}
		else if (args.Length == 0 && IsName(GetMetadata))
		{
			result = Expression.Call(list, GetMetadataMethod);
		}
		else if (args.Length == 1 && IsName(GetMetadata))
		{
			var arg = args[0].Expression;

			if (arg.Type == typeof(string))
			{
				result = Expression.Call(list, GetMetadataKeyMethod, arg);
			}
			else if (arg.Type == typeof(int))
			{
				result = Expression.Call(list, GetMetadataIndexMethod, arg);
			}
			else
			{
				result = Expression.Call(list, GetMetadataIndexMethod, Expression.Convert(arg, typeof(int)));
			}
		}
		else if (args.Length == 2 && IsName(SetMetadata))
		{
			var arg = args[0].Expression;
			var metadata = args[1].Expression;

			if (arg.Type == typeof(string))
			{
				result = Expression.Call(list, SetMetadataKeyMethod, arg, metadata);
			}
			else if (arg.Type == typeof(int))
			{
				result = Expression.Call(list, SetMetadataIndexMethod, arg, metadata);
			}
			else
			{
				result = Expression.Call(list, SetMetadataIndexMethod, Expression.Convert(arg, typeof(int)), metadata);
			}
		}

		if (result is not null)
		{
			return new DynamicMetaObject(CastResult(result, binder.ReturnType), SameTypeRestriction());
		}

		return ThrowMethodDoesNotExist();

		bool IsName(string name) => string.Equals(binder.Name, name, binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	protected abstract BindingRestrictions SameTypeRestriction();

	private DynamicMetaObject ThrowWrongIndexCount()
	{
		var exception = Expression.New(typeof(ArgumentException).GetConstructor([typeof(string)])!, Expression.Constant("DataModelList supports exactly one index argument string or numeric."));

		return new DynamicMetaObject(Expression.Throw(exception, typeof(object)), SameTypeRestriction());
	}

	private DynamicMetaObject ThrowMethodDoesNotExist()
	{
		var exception = Expression.New(typeof(MissingMethodException).GetConstructor([typeof(string)])!, Expression.Constant("The method does not exist."));

		return new DynamicMetaObject(Expression.Throw(exception, typeof(object)), SameTypeRestriction());
	}
}