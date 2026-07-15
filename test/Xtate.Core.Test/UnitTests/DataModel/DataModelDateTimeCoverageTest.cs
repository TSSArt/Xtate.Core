// Copyright © 2019-2026 Sergii Artemenko
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

using System.Globalization;
using System.Reflection;
using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelDateTimeCoverageTest
{
	[TestMethod]
	public void ExplicitConvertibleNumericMembersThrowInvalidCastException()
	{
		var value = (object) DataModelDateTime.FromDateTime(default);
		var methods = new[]
					  {
						  nameof(IConvertible.ToBoolean),
						  nameof(IConvertible.ToByte),
						  nameof(IConvertible.ToChar),
						  nameof(IConvertible.ToDecimal),
						  nameof(IConvertible.ToDouble),
						  nameof(IConvertible.ToInt16),
						  nameof(IConvertible.ToInt32),
						  nameof(IConvertible.ToInt64),
						  nameof(IConvertible.ToSByte),
						  nameof(IConvertible.ToSingle),
						  nameof(IConvertible.ToUInt16),
						  nameof(IConvertible.ToUInt32),
						  nameof(IConvertible.ToUInt64)
					  };

		foreach (var methodName in methods)
		{
			var method = GetConvertibleTarget(methodName);

			try
			{
				_ = method.Invoke(value, [null]);
				Assert.Fail($"{methodName} did not throw.");
			}
			catch (TargetInvocationException exception)
			{
				Assert.AreEqual(typeof(InvalidCastException), exception.InnerException?.GetType());
			}
		}
	}

	[TestMethod]
	public void ExplicitConvertibleToTypeCoversBothResultBranches()
	{
		var value = (object) DataModelDateTime.FromDateTimeOffset(new DateTimeOffset(2026, 7, 15, 12, 30, 0, TimeSpan.FromHours(2)));
		var method = GetConvertibleTarget(nameof(IConvertible.ToType));

		Assert.IsInstanceOfType<DateTimeOffset>(method.Invoke(value, [typeof(DateTimeOffset), CultureInfo.InvariantCulture]));
		Assert.IsInstanceOfType<DateTime>(method.Invoke(value, [typeof(DateTime), CultureInfo.InvariantCulture]));
	}

	[TestMethod]
	public void ObjectComparisonFormattingAndEqualityCoverInterfaceBranches()
	{
		var dateTime = DataModelDateTime.FromDateTime(new DateTime(2026, 7, 15, 12, 30, 0, DateTimeKind.Utc));
		var dateTimeOffset = DataModelDateTime.FromDateTimeOffset(new DateTimeOffset(2026, 7, 15, 12, 30, 0, TimeSpan.FromHours(2)));
		Span<char> buffer = stackalloc char[64];

		Assert.IsTrue(TryFormat((ISpanFormattable) dateTime, buffer, out var dateTimeLength));
		Assert.IsTrue(dateTimeLength > 0);
		Assert.IsTrue(TryFormat((ISpanFormattable) dateTimeOffset, buffer, out var offsetLength));
		Assert.IsTrue(offsetLength > 0);
		Assert.IsFalse(EqualsObject(dateTime, other: "not a date"));
		Assert.AreEqual(expected: 1, CompareObject(dateTime, other: null));

		try
		{
			_ = CompareObject(dateTime, other: "not a date");
			Assert.Fail("Comparing with an unrelated type did not throw.");
		}
		catch (ArgumentException)
		{
		}
	}

	private static MethodInfo GetConvertibleTarget(string methodName)
	{
		var interfaceMap = typeof(DataModelDateTime).GetInterfaceMap(typeof(IConvertible));
		var index = Array.FindIndex(interfaceMap.InterfaceMethods, method => method.Name == methodName);

		return interfaceMap.TargetMethods[index];
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool TryFormat(ISpanFormattable value, Span<char> destination, out int charsWritten) =>
		value.TryFormat(destination, out charsWritten, format: "O", CultureInfo.InvariantCulture);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool EqualsObject(DataModelDateTime value, object other) => value.Equals(other);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static int CompareObject(IComparable value, object? other) => value.CompareTo(other);
}
