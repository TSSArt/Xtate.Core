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