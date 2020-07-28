#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
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

		[AssertionMethod]
		[DoesNotReturn]
		public static void UnexpectedValue()
		{
			Assert(condition: false, Resources.Exception_UnexpectedValue);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static void UnexpectedValue(string message)
		{
			Assert(condition: false, message);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T UnexpectedValue<T>()
		{
			Assert(condition: false, Resources.Exception_UnexpectedValue);

			throw new NotSupportedException();
		}

		[AssertionMethod]
		[DoesNotReturn]
		public static T UnexpectedValue<T>(string message)
		{
			Assert(condition: false, message);

			throw new NotSupportedException();
		}

		public static void IgnoredException(Exception _) { }
	}
}