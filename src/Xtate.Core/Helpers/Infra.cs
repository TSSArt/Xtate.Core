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

using System.ComponentModel;

namespace Xtate;

[ExcludeFromCodeCoverage]
internal static class Infra
{
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void EnsureNotDisposed([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool condition, object? instance)
	{
		if (!condition)
		{
			ThrowObjectDisposed(instance);
		}
	}

	/// <summary>
	///     Checks for a condition; if the condition is <see langword="false" />, throws
	///     <see cref="InvalidOperationException" /> exception.
	/// </summary>
	/// <param name="condition">
	///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
	///     returned to caller.
	/// </param>
	/// <exception cref="InvalidOperationException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool condition)
	{
		if (!condition)
		{
			ThrowAssertion();
		}
	}

	/// <summary>
	///     Checks for a condition; if the condition is <see langword="false" />, throws
	///     <see cref="InvalidOperationException" /> exception.
	/// </summary>
	/// <param name="condition">
	///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
	///     returned to caller.
	/// </param>
	/// <param name="message">The message for <see cref="InvalidOperationException" />. </param>
	/// <exception cref="InvalidOperationException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool condition, string message)
	{
		if (!condition)
		{
			ThrowAssertion(message);
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
#if NET5_0_OR_GREATER
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
	///     Checks value for a positive value; if the value is negative or zero, throws
	///     <see cref="ArgumentOutOfRangeException" /> exception.
	/// </summary>
	/// <param name="parameter">
	///     The value to check for positive value. If the value is positive, execution returned to caller.
	/// </param>
	/// <param name="parameterName">Parameter name. (Autogenerated)</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RequiresPositive(TimeSpan parameter, [CallerArgumentExpression(nameof(parameter))] string? parameterName = default)
	{
		if (parameter <= TimeSpan.Zero)
		{
			ThrowOutOfRangeException(parameterName, parameter, new TimeSpan(1), TimeSpan.MaxValue);
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
		if (parameter is not { Length: > 0 })
		{
			ThrowNullOrEmptyString(parameterName);
		}
	}

	/// <summary>
	///     Checks value for a null; if the value is <see langword="null" />, throws
	///     <see cref="InvalidOperationException" /> exception.
	/// </summary>
	/// <param name="value">
	///     The value to check for null. If the value is not <see langword="null" />, execution returned to
	///     caller.
	/// </param>
	/// <exception cref="InvalidOperationException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] T? value)
	{
		if (value is null)
		{
			ThrowAssertion();
		}
	}

	/// <summary>
	///     Checks value for a null; if the value is <see langword="null" />, throws
	///     <see cref="InvalidOperationException" /> exception.
	/// </summary>
	/// <param name="value">
	///     The value to check for null. If the value is not <see langword="null" />, execution returned to
	///     caller.
	/// </param>
	/// <param name="message">The message for <see cref="InvalidOperationException" />. </param>
	/// <exception cref="InvalidOperationException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] T? value, string message)
	{
		if (value is null)
		{
			ThrowAssertion(message);
		}
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static void Fail() => ThrowAssertion();

	[AssertionMethod]
	[DoesNotReturn]
	public static void Fail(string message) => ThrowAssertion(message);

	[AssertionMethod]
	[DoesNotReturn]
	public static T Fail<T>()
	{
		ThrowAssertion();

		return default;
	}

	[AssertionMethod]
	[DoesNotReturn]
	public static T Fail<T>(string message)
	{
		ThrowAssertion(message);

		return default;
	}

	[DoesNotReturn]
	private static void ThrowObjectDisposed(object? instance) =>
		throw (instance switch
			   {
				   null => new ObjectDisposedException(default),
				   _    => new ObjectDisposedException(instance.GetType().FullName)
			   });

	[DoesNotReturn]
	private static void ThrowAssertion() => throw new InvalidOperationException(Resources.Exception_AssertionFailed);

	[DoesNotReturn]
	private static void ThrowAssertion(string message) => throw new InvalidOperationException(message);

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
	private static void ThrowOutOfRangeException<T>(string? paramName,
													T value,
													T min,
													T max) =>
		throw new ArgumentOutOfRangeException(paramName, value, Res.Format(Resources.Exception_ValidRangeIsMinMax, min, max));

	public static InvalidOperationException Unmatched<T>(T value) => new(Res.Format(Resources.Exception_AssertUnmatched, typeof(T).FullName, value));

	public static InvalidOperationException Unmatched<T>(T value, string message) => new(Res.Format(Resources.Exception_AssertUnmatchedMessage, message, typeof(T).FullName, value));
}