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
using System.Linq.Expressions;

namespace Xtate.Core.IoC;

public readonly struct ArgumentType
{
	private static readonly Type[] _funcSet =
	{
		typeof(Func<>),
		typeof(Func<,>),
		typeof(Func<,,>),
		typeof(Func<,,,>),
		typeof(Func<,,,,>),
		typeof(Func<,,,,,>),
		typeof(Func<,,,,,,>),
		typeof(Func<,,,,,,,>),
		typeof(Func<,,,,,,,,>),
		typeof(Func<,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,,,,,,>),
		typeof(Func<,,,,,,,,,,,,,,,,>)
	};

	private static readonly Type[] _numToValueTupleMap =
	{
		typeof(ValueTuple),
		typeof(ValueTuple<>),
		typeof(ValueTuple<,>),
		typeof(ValueTuple<,,>),
		typeof(ValueTuple<,,,>),
		typeof(ValueTuple<,,,,>),
		typeof(ValueTuple<,,,,,>),
		typeof(ValueTuple<,,,,,,>),
		typeof(ValueTuple<,,,,,,,>)
	};

	private readonly Type? _type;

	private ArgumentType(Type type) => _type = type != typeof(ValueTuple) ? type : default;

	public static ArgumentType TypeOf<T>() => new(typeof(T));

	public bool IsEmpty => _type is null;

	public Type Type => _type ?? typeof(ValueTuple);

	public override string ToString() => _type?.Name ?? @"(empty)";

	public static TDelegate CastFunc<TDelegate>(Delegate func) where TDelegate : Delegate => (TDelegate) CastFunc(func, typeof(TDelegate));

	private static Delegate CastFunc(Delegate func, Type toType)
	{
		Infra.Requires(func);

		if (!toType.IsGenericType || Array.IndexOf(_funcSet, toType.GetGenericTypeDefinition()) < 0)
		{
			throw new InvalidCastException(Res.Format(Resources.Exception_CantCastForwardDelegate, func.GetType(), toType));
		}

		var toArgs = toType.GetGenericArguments();
		var args = toArgs.Length > 1 ? new ParameterExpression[toArgs.Length - 1] : Array.Empty<ParameterExpression>();

		for (var i = 0; i < args.Length; i ++)
		{
			args[i] = Expression.Parameter(toArgs[i]);
		}

		var arg = args.Length switch
				  {
					  0    => Expression.Default(typeof(ValueTuple)),
					  1    => args[0],
					  >= 2 => CreateSingleArgument(args, 0),
					  _    => Infra.Unexpected<Expression>(args.Length)
				  };

		return Expression.Lambda(Expression.Invoke(Expression.Constant(func), arg), args).Compile();
	}

	private static Expression CreateSingleArgument(ParameterExpression[] args, int start)
	{
		Expression[] valueTupleArgs;
		var length = args.Length - start;

		if (length > 7)
		{
			valueTupleArgs = new Expression[8];
			Array.Copy(args, start, valueTupleArgs, 0, 7);
			valueTupleArgs[7] = CreateSingleArgument(args, start + 7);
		}
		else
		{
			valueTupleArgs = new Expression[length];
			Array.Copy(args, start, valueTupleArgs, 0, length);
		}

		var types = Array.ConvertAll(valueTupleArgs, static e => e.Type);
		var valueTupleType = _numToValueTupleMap[types.Length].MakeGenericType(types);

		return Expression.New(valueTupleType.GetConstructor(types)!, valueTupleArgs);
	}
}