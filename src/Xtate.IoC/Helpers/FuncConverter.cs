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

internal static class FuncConverter
{
	private static readonly Type[] FuncSet =
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

	private static readonly Type[] NumToValueTupleMap =
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

	public static TDelegate Cast<TDelegate>(Delegate func) where TDelegate : Delegate => (TDelegate) Cast(func, typeof(TDelegate));

	private static Delegate Cast(Delegate func, Type toType)
	{
		Infra.Requires(func);

		if (func.GetType() == toType)
		{
			return func;
		}

		if (!toType.IsGenericType || Array.IndexOf(FuncSet, toType.GetGenericTypeDefinition()) < 0)
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
					  0 => Expression.Default(typeof(Empty)),
					  1 => args[0],
					  _ => CreateSingleArgument(args, start: 0)
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
			Array.Copy(args, start, valueTupleArgs, destinationIndex: 0, length: 7);
			valueTupleArgs[7] = CreateSingleArgument(args, start + 7);
		}
		else
		{
			valueTupleArgs = new Expression[length];
			Array.Copy(args, start, valueTupleArgs, destinationIndex: 0, length);
		}

		var types = Array.ConvertAll(valueTupleArgs, static e => e.Type);
		var valueTupleType = NumToValueTupleMap[types.Length].MakeGenericType(types);

		return Expression.New(valueTupleType.GetConstructor(types)!, valueTupleArgs);
	}
}