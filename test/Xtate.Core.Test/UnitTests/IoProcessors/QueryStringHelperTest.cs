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

using Xtate.IoProcessors.Http.Internal;

namespace Xtate.Test.UnitTests.IoProcessors;

[TestClass]
public class QueryStringHelperTest
{
	[TestMethod]
	public void AddQueryStringAppendsFirstParameter()
	{
		var uri = QueryStringHelper.AddQueryString(uri: "https://xtate.net/callback", name: "event", value: "done.invoke");

		Assert.AreEqual(expected: "https://xtate.net/callback?event=done.invoke", uri);
	}

	[TestMethod]
	public void AddQueryStringAppendsAdditionalParameterBeforeFragment()
	{
		var uri = QueryStringHelper.AddQueryString(uri: "https://xtate.net/callback?existing=1#anchor", name: "event name", value: "hello world");

		Assert.AreEqual(expected: "https://xtate.net/callback?existing=1&event%20name=hello%20world#anchor", uri);
	}

	[TestMethod]
	public void ParseQueryReturnsEmptyCollectionForMissingQuery()
	{
		Assert.AreEqual(expected: 0, QueryStringHelper.ParseQuery(null).Count);
		Assert.AreEqual(expected: 0, QueryStringHelper.ParseQuery(string.Empty).Count);
		Assert.AreEqual(expected: 0, QueryStringHelper.ParseQuery("?").Count);
	}

	[TestMethod]
	public void ParseQueryHandlesValuesFlagsWhitespaceAndEscaping()
	{
		var values = QueryStringHelper.ParseQuery("? first=one+two&encoded=a%2Fb&flag&empty=&repeated=1&repeated=2");

		Assert.AreEqual(expected: "one two", values["first"]);
		Assert.AreEqual(expected: "a/b", values["encoded"]);
		Assert.AreEqual(string.Empty, values["flag"]);
		Assert.AreEqual(string.Empty, values["empty"]);
		CollectionAssert.AreEqual(new[] { "1", "2" }, values.GetValues("repeated"));
	}

	[TestMethod]
	public void ParseQueryIgnoresEmptySegmentsBetweenDelimiters()
	{
		var values = QueryStringHelper.ParseQuery("&&left=1&&right=2&");

		Assert.AreEqual(expected: 2, values.Count);
		Assert.AreEqual(expected: "1", values["left"]);
		Assert.AreEqual(expected: "2", values["right"]);
	}

	[TestMethod]
	public void ParseQueryNoEqualSign()
	{
		var values = QueryStringHelper.ParseQuery("&left&right");

		Assert.AreEqual(expected: 2, values.Count);
		Assert.AreEqual(expected: "", values["left"]);
		Assert.AreEqual(expected: "", values["right"]);
	}
}