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

namespace Xtate.IoC;

internal static class TupleHelper
{
	private static readonly Dictionary<Type, Type> ValueTupleMap = new()
																   {
																	   { typeof(ValueTuple<>), typeof(ValueTupleArgument<,>) },
																	   { typeof(ValueTuple<,>), typeof(ValueTupleArgument<,,>) },
																	   { typeof(ValueTuple<,,>), typeof(ValueTupleArgument<,,,>) },
																	   { typeof(ValueTuple<,,,>), typeof(ValueTupleArgument<,,,,>) },
																	   { typeof(ValueTuple<,,,,>), typeof(ValueTupleArgument<,,,,,>) },
																	   { typeof(ValueTuple<,,,,,>), typeof(ValueTupleArgument<,,,,,,>) },
																	   { typeof(ValueTuple<,,,,,,>), typeof(ValueTupleArgument<,,,,,,,>) },
																	   { typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleArgument<,,,,,,,,>) }
																   };

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryMatch<TArg, TValue>(ref TArg? arg, out TValue? value) =>
		ArgumentBase<TValue, TArg>.Instance.TryUnwrapInternal(ref arg, out value) || ArgumentBase<TArg, TValue>.Instance.TryMatchInternal(ref arg, out value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryMatch<T>(Type type, ref T? arg, out object? value) => ArgumentBase<T, object>.Instance.TryMatchInternal(type, ref arg, out value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Expression? TryBuild<T>(Type type, Expression arg) => ArgumentBase<T, object>.Instance.TryBuildInternal(type, arg);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsMatch<T>(Type type) => ArgumentBase<T, object>.Instance.IsMatchInternal(type);

	private static Expression GetItem1(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item1");
	private static Expression GetItem2(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item2");
	private static Expression GetItem3(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item3");
	private static Expression GetItem4(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item4");
	private static Expression GetItem5(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item5");
	private static Expression GetItem6(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item6");
	private static Expression GetItem7(Expression expr) => Expression.PropertyOrField(expr, propertyOrFieldName: @"Item7");
	private static Expression GetRest(Expression expr)  => Expression.PropertyOrField(expr, propertyOrFieldName: @"Rest");

	private abstract class ArgumentBase<TArg, TValue>
	{
		public static readonly ArgumentBase<TArg, TValue> Instance = GetInstanceType().CreateInstance<ArgumentBase<TArg, TValue>>();

		private static Type GetInstanceType()
		{
			if (typeof(TArg).IsGenericType && ValueTupleMap.TryGetValue(typeof(TArg).GetGenericTypeDefinition(), out var type))
			{
				return type.MakeGenericTypeExt(typeof(TArg).GetGenericArguments(), typeof(TValue));
			}

			return typeof(OtherArgument<,>).MakeGenericTypeExt(typeof(TArg), typeof(TValue));
		}

		public abstract bool TryMatchInternal(ref TArg? arg, out TValue? val);

		public abstract bool TryMatchInternal(Type type, ref TArg? arg, out object? val);

		public abstract bool IsMatchInternal(Type type);

		public abstract Expression? TryBuildInternal(Type type, Expression arg);

		public abstract bool TryUnwrapInternal(ref TValue? arg, out TArg? val);
	}

	private sealed class OtherArgument<TArg, TValue> : ArgumentBase<TArg, TValue>
	{
		public override bool TryMatchInternal(ref TArg? arg, out TValue? val)
		{
			if (typeof(TValue) == typeof(TArg))
			{
				val = ConvertHelper<TArg, TValue>.Convert(arg!);

				return true;
			}

			val = default;

			return false;
		}

		public override bool TryUnwrapInternal(ref TValue? arg, out TArg? val)
		{
			val = default;

			return typeof(TArg) == typeof(Empty);
		}

		public override bool TryMatchInternal(Type type, ref TArg? arg, out object? val)
		{
			if (type == typeof(TArg))
			{
				val = arg;

				return true;
			}

			val = default;

			return false;
		}

		public override Expression? TryBuildInternal(Type type, Expression arg) => type == typeof(TArg) ? arg : default;

		public override bool IsMatchInternal(Type type) => type == typeof(TArg);
	}

	private sealed class ValueTupleArgument<T1, TValue> : ArgumentBase<ValueTuple<T1?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?> arg, out TValue? val) => TryMatch(ref arg.Item1, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?> arg, out object? val) => TryMatch(type, ref arg.Item1, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) => TryBuild<T1>(type, GetItem1(arg));

		public override bool IsMatchInternal(Type type) => IsMatch<T1>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out ValueTuple<T1?> val) => TryMatch(ref arg, out val.Item1);
	}

	private sealed class ValueTupleArgument<T1, T2, TValue> : ArgumentBase<ValueTuple<T1?, T2?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?> arg, out TValue? val) => TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?> arg, out object? val) => TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) => TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg));

		public override bool IsMatchInternal(Type type) => IsMatch<T1>(type) || IsMatch<T2>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out (T1?, T2?) val) => TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, TValue> : ArgumentBase<ValueTuple<T1?, T2?, T3?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?, T3?> arg, out TValue? val) =>
			TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val) || TryMatch(ref arg.Item3, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) => TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg)) ?? TryBuild<T3>(type, GetItem3(arg));

		public override bool IsMatchInternal(Type type) => IsMatch<T1>(type) || IsMatch<T2>(type) || IsMatch<T3>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out (T1?, T2?, T3?) val) => TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2) & TryMatch(ref arg, out val.Item3);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, TValue> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?, T3?, T4?> arg, out TValue? val) =>
			TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val) || TryMatch(ref arg.Item3, out val) || TryMatch(ref arg.Item4, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) =>
			TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg)) ??
			TryBuild<T3>(type, GetItem3(arg)) ?? TryBuild<T4>(type, GetItem4(arg));

		public override bool IsMatchInternal(Type type) => IsMatch<T1>(type) || IsMatch<T2>(type) || IsMatch<T3>(type) || IsMatch<T4>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out (T1?, T2?, T3?, T4?) val) =>
			TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2) & TryMatch(ref arg, out val.Item3) & TryMatch(ref arg, out val.Item4);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, TValue> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?, T3?, T4?, T5?> arg, out TValue? val) =>
			TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val) || TryMatch(ref arg.Item3, out val) || TryMatch(ref arg.Item4, out val) ||
			TryMatch(ref arg.Item5, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) =>
			TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg)) ??
			TryBuild<T3>(type, GetItem3(arg)) ?? TryBuild<T4>(type, GetItem4(arg)) ?? TryBuild<T5>(type, GetItem5(arg));

		public override bool IsMatchInternal(Type type) => IsMatch<T1>(type) || IsMatch<T2>(type) || IsMatch<T3>(type) || IsMatch<T4>(type) || IsMatch<T5>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out (T1?, T2?, T3?, T4?, T5?) val) =>
			TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2) & TryMatch(ref arg, out val.Item3) & TryMatch(ref arg, out val.Item4) & TryMatch(ref arg, out val.Item5);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, T6, TValue> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> arg, out TValue? val) =>
			TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val) || TryMatch(ref arg.Item3, out val) || TryMatch(ref arg.Item4, out val) ||
			TryMatch(ref arg.Item5, out val) || TryMatch(ref arg.Item6, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val) || TryMatch(type, ref arg.Item6, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) =>
			TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg)) ??
			TryBuild<T3>(type, GetItem3(arg)) ?? TryBuild<T4>(type, GetItem4(arg)) ?? TryBuild<T5>(type, GetItem5(arg)) ?? TryBuild<T6>(type, GetItem6(arg));

		public override bool IsMatchInternal(Type type) => IsMatch<T1>(type) || IsMatch<T2>(type) || IsMatch<T3>(type) || IsMatch<T4>(type) || IsMatch<T5>(type) || IsMatch<T6>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out (T1?, T2?, T3?, T4?, T5?, T6?) val) =>
			TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2) & TryMatch(ref arg, out val.Item3) & TryMatch(ref arg, out val.Item4) & TryMatch(ref arg, out val.Item5) &
			TryMatch(ref arg, out val.Item6);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, T6, T7, TValue> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>, TValue>
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> arg, out TValue? val) =>
			TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val) || TryMatch(ref arg.Item3, out val) || TryMatch(ref arg.Item4, out val) ||
			TryMatch(ref arg.Item5, out val) || TryMatch(ref arg.Item6, out val) || TryMatch(ref arg.Item7, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val) || TryMatch(type, ref arg.Item6, out val) ||
			TryMatch(type, ref arg.Item7, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) =>
			TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg)) ??
			TryBuild<T3>(type, GetItem3(arg)) ?? TryBuild<T4>(type, GetItem4(arg)) ?? TryBuild<T5>(type, GetItem5(arg)) ?? TryBuild<T6>(type, GetItem6(arg)) ??
			TryBuild<T7>(type, GetItem7(arg));

		public override bool IsMatchInternal(Type type) =>
			IsMatch<T1>(type) || IsMatch<T2>(type) || IsMatch<T3>(type) || IsMatch<T4>(type) || IsMatch<T5>(type) || IsMatch<T6>(type) || IsMatch<T7>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out (T1?, T2?, T3?, T4?, T5?, T6?, T7?) val) =>
			TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2) & TryMatch(ref arg, out val.Item3) & TryMatch(ref arg, out val.Item4) & TryMatch(ref arg, out val.Item5) &
			TryMatch(ref arg, out val.Item6) & TryMatch(ref arg, out val.Item7);
	}

	private sealed class ValueTupleArgument<T1, T2, T3, T4, T5, T6, T7, TRest, TValue> : ArgumentBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>, TValue> where TRest : struct
	{
		public override bool TryMatchInternal(ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> arg, out TValue? val) =>
			TryMatch(ref arg.Item1, out val) || TryMatch(ref arg.Item2, out val) || TryMatch(ref arg.Item3, out val) || TryMatch(ref arg.Item4, out val) ||
			TryMatch(ref arg.Item5, out val) || TryMatch(ref arg.Item6, out val) || TryMatch(ref arg.Item7, out val) || TryMatch(ref arg.Rest, out val);

		public override bool TryMatchInternal(Type type, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> arg, out object? val) =>
			TryMatch(type, ref arg.Item1, out val) || TryMatch(type, ref arg.Item2, out val) || TryMatch(type, ref arg.Item3, out val) ||
			TryMatch(type, ref arg.Item4, out val) || TryMatch(type, ref arg.Item5, out val) || TryMatch(type, ref arg.Item6, out val) ||
			TryMatch(type, ref arg.Item7, out val) || TryMatch(type, ref arg.Rest, out val);

		public override Expression? TryBuildInternal(Type type, Expression arg) =>
			TryBuild<T1>(type, GetItem1(arg)) ?? TryBuild<T2>(type, GetItem2(arg)) ??
			TryBuild<T3>(type, GetItem3(arg)) ?? TryBuild<T4>(type, GetItem4(arg)) ?? TryBuild<T5>(type, GetItem5(arg)) ?? TryBuild<T6>(type, GetItem6(arg)) ??
			TryBuild<T7>(type, GetItem7(arg)) ?? TryBuild<TRest>(type, GetRest(arg));

		public override bool IsMatchInternal(Type type) =>
			IsMatch<T1>(type) || IsMatch<T2>(type) || IsMatch<T3>(type) || IsMatch<T4>(type) || IsMatch<T5>(type) || IsMatch<T6>(type) || IsMatch<T7>(type) || IsMatch<TRest>(type);

		public override bool TryUnwrapInternal(ref TValue? arg, out ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> val) =>
			TryMatch(ref arg, out val.Item1) & TryMatch(ref arg, out val.Item2) & TryMatch(ref arg, out val.Item3) & TryMatch(ref arg, out val.Item4) & TryMatch(ref arg, out val.Item5) &
			TryMatch(ref arg, out val.Item6) & TryMatch(ref arg, out val.Item7) & TryMatch(ref arg, out val.Rest);
	}
}