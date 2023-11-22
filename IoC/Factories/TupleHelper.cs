#region Copyright © 2019-2022 Sergii Artemenko

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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Xtate.Core.IoC;

public static class TupleHelper
{
	private static readonly Dictionary<Type, Type> _valueTupleMap = new()
																	{
																		{ typeof(ValueTuple<>), typeof(ValueTupleArgument<>) },
																		{ typeof(ValueTuple<,>), typeof(ValueTupleArgument<,>) },
																		{ typeof(ValueTuple<,,>), typeof(ValueTupleArgument<,,>) },
																		{ typeof(ValueTuple<,,,>), typeof(ValueTupleArgument<,,,>) },
																		{ typeof(ValueTuple<,,,,>), typeof(ValueTupleArgument<,,,,>) },
																		{ typeof(ValueTuple<,,,,,>), typeof(ValueTupleArgument<,,,,,>) },
																		{ typeof(ValueTuple<,,,,,,>), typeof(ValueTupleArgument<,,,,,,>) },
																		{ typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleArgument<,,,,,,,>) }
																	};

	public static bool TryMatch<T>(Type type, ref T? arg, out object? value) => ArgumentBase<T>.Instance.TryMatchInternal(type, ref arg, out value);

	public static bool TryBuild<T>(Type type, ref Expression arg, Func<Expression, Expression>? convert = default) => ArgumentBase<T>.Instance.TryBuildInternal(type, ref arg, convert);

	public static bool IsMatch<T>(Type type) => ArgumentBase<T>.Instance.IsMatchInternal(type);

	private static Expression GetItem1(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item1");
	private static Expression GetItem2(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item2");
	private static Expression GetItem3(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item3");
	private static Expression GetItem4(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item4");
	private static Expression GetItem5(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item5");
	private static Expression GetItem6(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item6");
	private static Expression GetItem7(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item7");
	private static Expression GetRest(Expression expr)  => Expression.PropertyOrField(expr, propertyOrFieldName: @"Rest");

	private abstract class ArgumentBase<T>
	{
		public static readonly ArgumentBase<T> Instance = (ArgumentBase<T>) Activator.CreateInstance(GetInstanceType())!;

		private static Type GetInstanceType()
		{
			if (typeof(T).IsGenericType && _valueTupleMap.TryGetValue(typeof(T).GetGenericTypeDefinition(), out var type))
			{
				return type.MakeGenericType(typeof(T).GetGenericArguments());
			}

			return typeof(OtherArgument<>).MakeGenericType(typeof(T));
		}

		public abstract bool TryMatchInternal(Type type, ref T? arg, out object? val);

		public abstract bool IsMatchInternal(Type type);

		public abstract bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc);
	}

	private sealed class OtherArgument<T> : ArgumentBase<T>
	{
		public override bool TryMatchInternal(Type type, ref T? arg, out object? val)
		{
			if (type == typeof(T))
			{
				val = arg;

				return true;
			}

			val = default;

			return false;
		}

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc)
		{
			if (type == typeof(T))
			{
				if (convertFunc is not null)
				{
					arg = convertFunc(arg);
				}

				return true;
			}

			return false;
		}

		public override bool IsMatchInternal(Type type) => type == typeof(T);
	}

	private sealed class ValueTupleArgument<T1> : ArgumentBase<ValueTuple<T1?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?> arg, out object? val) => TryMatch(type, ref arg.Item1, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) => TryBuild<T1>(type, ref arg, GetItem1);

		public override bool IsMatchInternal(Type type) => type == typeof(T1);
	}

	private sealed class ValueTupleArgument<T1, T2> : ArgumentBase<ValueTuple<T1?, T2?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?> arg, out object? val) => TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2);

		public override bool IsMatchInternal(Type type) => type == typeof(T1) || type == typeof(T2);
	}

	private sealed class ValueTupleArgument<T1, T2, T3> : ArgumentBase<ValueTuple<T1?, T2?, T3?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2) || TryBuild<T3>(type, ref arg, GetItem3);

		public override bool IsMatchInternal(Type type) => type == typeof(T1) || type == typeof(T2) || type == typeof(T3);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2) || TryBuild<T3>(type, ref arg, GetItem3) ||
			TryBuild<T4>(type, ref arg, GetItem4);

		public override bool IsMatchInternal(Type type) => type == typeof(T1) || type == typeof(T2) || type == typeof(T3) || type == typeof(T4);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2) || TryBuild<T3>(type, ref arg, GetItem3) ||
			TryBuild<T4>(type, ref arg, GetItem4) || TryBuild<T5>(type, ref arg, GetItem5);

		public override bool IsMatchInternal(Type type) => type == typeof(T1) || type == typeof(T2) || type == typeof(T3) || type == typeof(T4) || type == typeof(T5);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, T6> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val) || TryMatch(type, ref arg.Item6, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2) || TryBuild<T3>(type, ref arg, GetItem3) ||
			TryBuild<T4>(type, ref arg, GetItem4) || TryBuild<T5>(type, ref arg, GetItem5) || TryBuild<T6>(type, ref arg, GetItem6);

		public override bool IsMatchInternal(Type type) => type == typeof(T1) || type == typeof(T2) || type == typeof(T3) || type == typeof(T4) || type == typeof(T5) || type == typeof(T6);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, T6, T7> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val) || TryMatch(type, ref arg.Item6, out val) ||
			TryMatch(type, ref arg.Item7, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2) || TryBuild<T3>(type, ref arg, GetItem3) ||
			TryBuild<T4>(type, ref arg, GetItem4) || TryBuild<T5>(type, ref arg, GetItem5) || TryBuild<T6>(type, ref arg, GetItem6) ||
			TryBuild<T7>(type, ref arg, GetItem7);

		public override bool IsMatchInternal(Type type) =>
			type == typeof(T1) || type == typeof(T2) || type == typeof(T3) || type == typeof(T4) || type == typeof(T5) || type == typeof(T6) || type == typeof(T7);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, T6, T7, TRest> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>> where TRest : struct
	{
		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val) || TryMatch(type, ref arg.Item6, out val) ||
			TryMatch(type, ref arg.Item7, out val) || TryMatch(type, ref arg.Rest, out val);

		public override bool TryBuildInternal(Type type, ref Expression arg, Func<Expression, Expression>? convertFunc) =>
			TryBuild<T1>(type, ref arg, GetItem1) || TryBuild<T2>(type, ref arg, GetItem2) || TryBuild<T3>(type, ref arg, GetItem3) ||
			TryBuild<T4>(type, ref arg, GetItem4) || TryBuild<T5>(type, ref arg, GetItem5) || TryBuild<T6>(type, ref arg, GetItem6) ||
			TryBuild<T7>(type, ref arg, GetItem7) || TryBuild<TRest>(type, ref arg, GetRest);

		public override bool IsMatchInternal(Type type) =>
			type == typeof(T1) || type == typeof(T2) || type == typeof(T3) || type == typeof(T4) || type == typeof(T5) || type == typeof(T6) || type == typeof(T7) || type == typeof(TRest);
	}
}