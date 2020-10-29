#region Copyright © 2019-2020 Sergii Artemenko

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
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	[ExcludeFromCodeCoverage]
	public static class Infrastructure
	{
		[AssertionMethod]
		public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
								  bool condition)
		{
			if (condition)
			{
				return;
			}

			throw new InfrastructureException(Resources.Exception_AssertionFailed);
		}

		[AssertionMethod]
		public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
								  bool condition, string message)
		{
			if (condition)
			{
				return;
			}

			throw new InfrastructureException(message);
		}

		[AssertionMethod]
		public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull]
								   object? value)
		{
			if (value is not null)
			{
				return;
			}

			throw new InfrastructureException(Resources.Exception_AssertionFailed);
		}

		[AssertionMethod]
		public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull]
								   object? value, string message)
		{
			if (value is not null)
			{
				return;
			}

			throw new InfrastructureException(message);
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static void Fail()
		{
			Assert(false);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static void Fail(string message)
		{
			Assert(condition: false, message);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T Fail<T>()
		{
			Assert(condition: false);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T Fail<T>(string message)
		{
			Assert(condition: false, message);

			throw new NotSupportedException();
		}

		private static void AssertUnexpectedValue(object? value, string message)
		{
			if (value is null)
			{
				Assert(condition: false, $"{message} (null)");

				return;
			}

			if (value.GetType().IsValueType)
			{
				Assert(condition: false, $"{message} ({value.GetType().FullName}:{value})");

				return;
			}

			Assert(condition: false, $"{message} ({value.GetType().FullName})");
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static void UnexpectedValue(object? value)
		{
			AssertUnexpectedValue(value, Resources.Exception_UnexpectedValue);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static void UnexpectedValue(object? value, string message)
		{
			AssertUnexpectedValue(value, message);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T UnexpectedValue<T>(object? value)
		{
			AssertUnexpectedValue(value, Resources.Exception_UnexpectedValue);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T UnexpectedValue<T>(object? value, string message)
		{
			AssertUnexpectedValue(value, message);

			throw new NotSupportedException();
		}

		public static void IgnoredException(Exception _) { }
	}
}