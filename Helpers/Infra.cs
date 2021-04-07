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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Xtate
{
	[PublicAPI]
	[ExcludeFromCodeCoverage]
	public static class Infra
	{
		/// <summary>
		///     Checks for a condition; if the condition is <see langword="false" />, throws
		///     <see cref="Xtate.InfrastructureException" /> exception.
		/// </summary>
		/// <param name="condition">
		///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
		///     returned to caller.
		/// </param>
		[AssertionMethod]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
								  bool condition)
		{
			if (!condition)
			{
				ThrowInfrastructureException();
			}
		}

		/// <summary>
		///     Checks for a condition; if the condition is <see langword="false" />, throws
		///     <see cref="Xtate.InfrastructureException" /> exception.
		/// </summary>
		/// <param name="condition">
		///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
		///     returned to caller.
		/// </param>
		/// <param name="message">The message for <see cref="Xtate.InfrastructureException" />. </param>
		[AssertionMethod]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
								  bool condition,
								  string message)
		{
			if (!condition)
			{
				ThrowInfrastructureException(message);
			}
		}

		/// <summary>
		///     Checks value for a null; if the value is <see langword="null" />, throws
		///     <see cref="Xtate.InfrastructureException" /> exception.
		/// </summary>
		/// <param name="value">
		///     The value to check for null. If the value is not <see langword="null" />, execution returned to
		///     caller.
		/// </param>
		[AssertionMethod]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull]
								   object? value)
		{
			if (value is null)
			{
				ThrowInfrastructureException();
			}
		}

		/// <summary>
		///     Checks value for a null; if the value is <see langword="null" />, throws
		///     <see cref="Xtate.InfrastructureException" /> exception.
		/// </summary>
		/// <param name="value">
		///     The value to check for null. If the value is not <see langword="null" />, execution returned to
		///     caller.
		/// </param>
		/// <param name="message">The message for <see cref="Xtate.InfrastructureException" />. </param>
		[AssertionMethod]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull]
								   object? value,
								   string message)
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
			Assert(false);

			throw WrongExecutionPath();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static void Fail(string message)
		{
			Assert(condition: false, message);

			throw WrongExecutionPath();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T Fail<T>()
		{
			Assert(condition: false);

			throw WrongExecutionPath();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T Fail<T>(string message)
		{
			Assert(condition: false, message);

			throw WrongExecutionPath();
		}

		private static Exception WrongExecutionPath() => new NotSupportedException(Resources.Exception_ThisExceptionShouldNeverHappen);

		[DoesNotReturn]
		private static void ThrowInfrastructureException() => throw new InfrastructureException(Resources.Exception_AssertionFailed);

		[DoesNotReturn]
		private static void ThrowInfrastructureException(string message) => throw new InfrastructureException(message);

		[DoesNotReturn]
		private static void AssertUnexpected(object? value, string message)
		{
			if (value is null)
			{
				Assert(condition: false, @$"{message} (null)");

				throw WrongExecutionPath();
			}

			var type = value.GetType();
			if (type.IsPrimitive || type.IsEnum)
			{
				Assert(condition: false, @$"{message} ({type.FullName}:{value})");

				throw WrongExecutionPath();
			}

			Assert(condition: false, @$"{message} ({type.FullName})");

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
}