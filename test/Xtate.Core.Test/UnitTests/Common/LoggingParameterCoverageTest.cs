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
using Xtate.Logging.Provider;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class LoggingParameterCoverageTest
{
	[TestMethod]
	public void NamesFormattingAndFullNameIncludeNamespaceWhenPresent()
	{
		var parameter = new LoggingParameter(name: "Count", value: 15, format: "X2") { Namespace = "Machine" };

		Assert.AreEqual(expected: "Machine::Count", parameter.FullName());
		Assert.AreEqual(expected: "Machine::Count:0F", parameter.ToString(format: null, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: "Machine::Count:0F", parameter.ToString());
	}

	[TestMethod]
	public void FormattingHandlesMissingNameNamespaceAndValue()
	{
		var unnamed = new LoggingParameter(string.Empty, value: 12);
		var named = new LoggingParameter(name: "Value", value: "text");
		var nullValue = new LoggingParameter(name: "Value", value: null);

		Assert.AreEqual(string.Empty, unnamed.FullName());
		Assert.AreEqual(expected: "12", unnamed.ToString(format: null, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: "Value", named.FullName());
		Assert.AreEqual(expected: "Value:text", named.ToString());
		Assert.AreEqual(expected: "Value:", nullValue.ToString());
	}

	[TestMethod]
	public void TryFormatWritesSpanFormattableAndObjectValues()
	{
		var numeric = new LoggingParameter(name: "Count", value: 15, format: "X2") { Namespace = "Machine" };
		Span<char> numericBuffer = stackalloc char[32];

		Assert.IsTrue(numeric.TryFormat(numericBuffer, out var numericLength, format: default, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: "Machine::Count:0F", numericBuffer[..numericLength].ToString());

		var text = new LoggingParameter(name: "Text", new TextValue("payload"));
		Span<char> textBuffer = stackalloc char[32];
		Assert.IsTrue(text.TryFormat(textBuffer, out var textLength, format: default, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: "Text:payload", textBuffer[..textLength].ToString());
	}

	[TestMethod]
	public void TryFormatReturnsFalseWhenAnySegmentDoesNotFit()
	{
		AssertCannotFormat(new LoggingParameter(name: "Name", value: 1) { Namespace = "Namespace" }, capacity: 1);
		AssertCannotFormat(new LoggingParameter(name: "Name", value: 1) { Namespace = "Namespace" }, capacity: 10);
		AssertCannotFormat(new LoggingParameter(name: "Name", value: 1), capacity: 2);
		AssertCannotFormat(new LoggingParameter(name: "Name", value: 1), capacity: 4);
		AssertCannotFormat(new LoggingParameter(string.Empty, value: 12345), capacity: 2);
		AssertCannotFormat(new LoggingParameter(string.Empty, new TextValue("long-value")), capacity: 2);
	}

	private static void AssertCannotFormat(LoggingParameter parameter, int capacity)
	{
		var buffer = new char[capacity];

		Assert.IsFalse(parameter.TryFormat(buffer, out _, format: default, CultureInfo.InvariantCulture));
	}

	private sealed record TextValue(string Text)
	{
		public override string ToString() => Text;
	}
}