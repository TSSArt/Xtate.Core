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

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class FullUriCoverageTest
{
	[TestMethod]
	public void EqualityIncludesFragmentForAbsoluteUris()
	{
		var first = new FullUri("https://example.test/path#one");
		var same = new FullUri("https://example.test/path#one");
		var differentFragment = new FullUri("https://example.test/path#two");
		Uri plainUri = new("https://example.test/path#one");

		Assert.IsTrue(first.Equals(same));
		Assert.IsTrue(first == same);
		Assert.IsFalse(first != same);
		Assert.AreEqual(first.GetHashCode(), same.GetHashCode());
		Assert.IsFalse(first.Equals(differentFragment));
		Assert.IsTrue(first != differentFragment);
		Assert.IsFalse(first.Equals(plainUri));
		Assert.IsFalse(first.Equals(null));
	}

	[TestMethod]
	public void ConstructorsAndImplicitConversionPreserveResolvedUri()
	{
		var baseUri = new Uri("https://example.test/root/");
		var fromString = new FullUri(baseUri, relativeUri: "child#fragment");
		var fromUri = new FullUri(baseUri, new Uri(uriString: "other#fragment", UriKind.Relative));
		FullUri implicitUri = "relative/path#fragment";

		Assert.AreEqual(expected: "https://example.test/root/child#fragment", fromString.AbsoluteUri);
		Assert.AreEqual(expected: "https://example.test/root/other#fragment", fromUri.AbsoluteUri);
		Assert.IsFalse(implicitUri.IsAbsoluteUri);
		Assert.AreEqual(expected: "relative/path#fragment", implicitUri.OriginalString);
	}

	[TestMethod]
	public void TryCreateReturnsFullUriForAbsoluteAndRelativeInput()
	{
		Assert.IsTrue(FullUri.TryCreate(uriString: "https://example.test/path#fragment", out var absolute));
		Assert.IsNotNull(absolute);
		Assert.IsTrue(absolute.IsAbsoluteUri);
		Assert.AreEqual(expected: "#fragment", absolute.Fragment);

		Assert.IsTrue(FullUri.TryCreate(uriString: "relative/path#fragment", out var relative));
		Assert.IsNotNull(relative);
		Assert.IsFalse(relative.IsAbsoluteUri);
		Assert.AreEqual(expected: "relative/path#fragment", relative.OriginalString);

		Assert.IsFalse(FullUri.TryCreate(uriString: null, out var missing));
		Assert.IsNull(missing);
	}
}