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

using Xtate.IoC.Tools.Services;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class StackSpanAndSafeFactoryCoverageTest
{
	[TestMethod]
	public void StackSpanUsesStackSignalForSmallLengthAndPoolForLargeLength()
	{
		var small = new StackSpan<int>(StackSpan<int>.MaxLengthInStack);

		Assert.IsTrue(!small);
		Assert.IsFalse(small ? true : false);
		Assert.AreEqual(StackSpan<int>.MaxLengthInStack, (int) small);
		small.Dispose();

		var requestedLength = StackSpan<int>.MaxLengthInStack + 1;
		var large = new StackSpan<int>(requestedLength);

		Assert.IsFalse(!large);
		Assert.IsTrue(large ? true : false);
		Assert.AreEqual(requestedLength, (int) large);
		Span<int> span = large;
		Assert.AreEqual(requestedLength, span.Length);
		span[0] = 17;
		span[^1] = 29;
		Assert.AreEqual(expected: 17, span[0]);
		Assert.AreEqual(expected: 29, span[^1]);

		large.Dispose();
		large.Dispose();
	}

	[TestMethod]
	public async Task SafeFactoryPreservesResolvedValueAndNullValue()
	{
		var value = new object();
		var valueFactory = await SafeFactory<object>.Constructor(() => new ValueTask<object?>(value));
		var nullFactory = await SafeFactory<object>.Constructor(static () => new ValueTask<object?>((object?) null));

		Assert.AreSame(value, valueFactory.GetValueFunc()());
		Assert.IsNull(nullFactory.GetValueFunc()());
	}
}
