﻿#region Copyright © 2019-2023 Sergii Artemenko

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

using System.ComponentModel;

namespace Xtate;


[ExcludeFromCodeCoverage]
public static class Infra
{
	/// <summary>
	///     Checks for a condition; if the condition is <see langword="false" />, throws
	///     <see cref="InfrastructureException" /> exception.
	/// </summary>
	/// <param name="condition">
	///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
	///     returned to caller.
	/// </param>
	/// <exception cref="InfrastructureException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool condition)
	{
		if (!condition)
		{
			ThrowInfrastructureException();
		}
	}

	/// <summary>
	///     Checks for a condition; if the condition is <see langword="false" />, throws
	///     <see cref="InfrastructureException" /> exception.
	/// </summary>
	/// <param name="condition">
	///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
	///     returned to caller.
	/// </param>
	/// <param name="message">The message for <see cref="InfrastructureException" />. </param>
	/// <exception cref="InfrastructureException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool condition, string message)
	{
		if (!condition)
		{
			ThrowInfrastructureException(message);
		}
	}

	/// <summary>
	///     Checks value for a null; if the value is <see langword="null" />, throws
	///     <see cref="ArgumentNullException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The value to check for null. If the value is not <see langword="null" />, execution returned to
	///     caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="ArgumentNullException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Requires<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] T? parameter, [CallerArgumentExpression(nameof(parameter))] string? parameterName = default)
	{
		if (parameter is null)
		{
			ThrowArgumentNullException(parameterName);
		}
	}

	/// <summary>
	///     Checks value for a valid enum value; if the value is not valid enum, throws
	///     <see cref="InvalidEnumArgumentException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The value to check for valid enum value. If the value is valid, execution returned to
	///     caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="InvalidEnumArgumentException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RequiresValidEnum<T>(T parameter, [CallerArgumentExpression(nameof(parameter))] string? parameterName = default) where T : struct, Enum
	{
#if NET6_0_OR_GREATER
		if (!Enum.IsDefined(parameter))
		{
			ThrowInvalidEnumException(parameterName, parameter);
		}
#else
		if (!Enum.IsDefined(typeof(T), parameter))
		{
			ThrowInvalidEnumException(parameterName, parameter);
		}
#endif
	}

	/// <summary>
	///     Checks value for a non-negative value; if the value is negative, throws
	///     <see cref="ArgumentOutOfRangeException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The value to check for non-negative value. If the value is zero or positive, execution returned to
	///     caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RequiresNonNegative(int parameter, [CallerArgumentExpression(nameof(parameter))] string? parameterName = default)
	{
		if (parameter < 0)
		{
			ThrowOutOfRangeException(parameterName, parameter, min: 0, int.MaxValue);
		}
	}

	/// <summary>
	///     Checks value for a null and empty collection; if the value is <see langword="null" />, throws
	///     <see cref="ArgumentNullException" /> exception. if the value is empty collection, throws
	///     <see cref="ArgumentException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The value to check for null or empty collection. If the value is not <see langword="null" /> or empty collection,
	///     execution returned to
	///     caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RequiresNonEmptyCollection<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] T[]? parameter,
													 [CallerArgumentExpression(nameof(parameter))]
													 string? parameterName = default)
	{
		if (parameter is null)
		{
			ThrowArgumentNullException(parameterName);
		}

		if (parameter.Length == 0)
		{
			ThrowEmptyCollectionException(parameterName);
		}
	}

	/// <summary>
	///     Checks value for a null and empty collection; if the value is empty or default, throws
	///     <see cref="ArgumentException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The value to check for default or empty collection. If the value is not default or empty collection, execution
	///     returned to
	///     caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="ArgumentException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RequiresNonEmptyCollection<T>(ImmutableArray<T> parameter, [CallerArgumentExpression(nameof(parameter))] string? parameterName = default)
	{
		if (parameter.IsDefaultOrEmpty)
		{
			ThrowDefaultOrEmptyArray(parameterName);
		}
	}

	/// <summary>
	///     Checks string for a null or empty value; if the value is null or empty, throws
	///     <see cref="ArgumentException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The string to check for null or empty. If the string is not null or empty, execution returned to
	///     caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="ArgumentException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RequiresNonEmptyString([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] string? parameter,
											  [CallerArgumentExpression(nameof(parameter))]
											  string? parameterName = default)
	{
		if (string.IsNullOrEmpty(parameter))
		{
			ThrowNullOrEmptyString(parameterName);
		}
	}

	/// <summary>
	///     Checks value for a null; if the value is <see langword="null" />, throws
	///     <see cref="InfrastructureException" /> exception.
	/// </summary>
	/// <param name="value">
	///     The value to check for null. If the value is not <see langword="null" />, execution returned to
	///     caller.
	/// </param>
	/// <exception cref="InfrastructureException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] T? value)
	{
		if (value is null)
		{
			ThrowInfrastructureException();
		}
	}

	/// <summary>
	///     Checks value for a null; if the value is <see langword="null" />, throws
	///     <see cref="InfrastructureException" /> exception.
	/// </summary>
	/// <param name="value">
	///     The value to check for null. If the value is not <see langword="null" />, execution returned to
	///     caller.
	/// </param>
	/// <param name="message">The message for <see cref="InfrastructureException" />. </param>
	/// <exception cref="InfrastructureException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] T? value, string message)
	{
		if (value is null)
		{
			ThrowInfrastructureException(message);
		}
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static void Fail()
	{
		ThrowInfrastructureException();

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static void Fail(string message)
	{
		ThrowInfrastructureException(message);

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static T Fail<T>()
	{
		ThrowInfrastructureException();

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static T Fail<T>(string message)
	{
		ThrowInfrastructureException(message);

		throw WrongExecutionPath();
	}

	private static Exception WrongExecutionPath() => new NotSupportedException(Resources.Exception_ThisExceptionShouldNeverHappen);

	[DoesNotReturn]
	private static void ThrowInfrastructureException() => throw new InfrastructureException(Resources.Exception_AssertionFailed);

	[DoesNotReturn]
	private static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);

	[DoesNotReturn]
	private static void ThrowEmptyCollectionException(string? paramName) => throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, paramName);

	[DoesNotReturn]
	private static void ThrowDefaultOrEmptyArray(string? paramName) => throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, paramName);

	[DoesNotReturn]
	private static void ThrowNullOrEmptyString(string? paramName) => throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, paramName);

	[DoesNotReturn]
	private static void ThrowInvalidEnumException<T>(string? paramName, T value) where T : struct, Enum => throw new InvalidEnumArgumentException(paramName, Convert.ToInt32(value), typeof(T));

	[DoesNotReturn]
	private static void ThrowOutOfRangeException(string? paramName,
												 int value,
												 int min,
												 int max) =>
		throw new ArgumentOutOfRangeException(paramName, value, Res.Format(Resources.Exception_ValidRangeIsMinMax, min, max));

	[DoesNotReturn]
	private static void ThrowInfrastructureException(string message) => throw new InfrastructureException(message);

	[DoesNotReturn]
	private static void AssertUnexpected(object? value, string message)
	{
		if (value is null)
		{
			ThrowInfrastructureException(Res.Format(Resources.Exception_AssertUnexpected, message, arg1: @"null"));

			throw WrongExecutionPath();
		}

		var type = value.GetType();
		if (type.IsPrimitive || type.IsEnum)
		{
			ThrowInfrastructureException(Res.Format(Resources.Exception_AssertUnexpectedWithType, message, type, value));

			throw WrongExecutionPath();
		}

		if (value is Delegate)
		{
			ThrowInfrastructureException(Res.Format(Resources.Exception_AssertUnexpectedWithType, message, arg1: @"Delegate", value));

			throw WrongExecutionPath();
		}

		ThrowInfrastructureException(Res.Format(Resources.Exception_AssertUnexpected, message, type));

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static void Unexpected(object? value)
	{
		AssertUnexpected(value, Resources.Exception_UnexpectedValue);

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static void Unexpected(object? value, string message)
	{
		AssertUnexpected(value, message);

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static T Unexpected<T>(object? value)
	{
		AssertUnexpected(value, Resources.Exception_UnexpectedValue);

		throw WrongExecutionPath();
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static T Unexpected<T>(object? value, string message)
	{
		AssertUnexpected(value, message);

		throw WrongExecutionPath();
	}
}