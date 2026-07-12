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
		var uri = QueryStringHelper.AddQueryString("https://xtate.net/callback", "event", "done.invoke");

		Assert.AreEqual("https://xtate.net/callback?event=done.invoke", uri);
	}

	[TestMethod]
	public void AddQueryStringAppendsAdditionalParameterBeforeFragment()
	{
		var uri = QueryStringHelper.AddQueryString("https://xtate.net/callback?existing=1#anchor", "event name", "hello world");

		Assert.AreEqual("https://xtate.net/callback?existing=1&event%20name=hello%20world#anchor", uri);
	}

	[TestMethod]
	public void ParseQueryReturnsEmptyCollectionForMissingQuery()
	{
		Assert.AreEqual(0, QueryStringHelper.ParseQuery(null).Count);
		Assert.AreEqual(0, QueryStringHelper.ParseQuery(string.Empty).Count);
		Assert.AreEqual(0, QueryStringHelper.ParseQuery("?").Count);
	}

	[TestMethod]
	public void ParseQueryHandlesValuesFlagsWhitespaceAndEscaping()
	{
		var values = QueryStringHelper.ParseQuery("? first=one+two&encoded=a%2Fb&flag&empty=&repeated=1&repeated=2");

		Assert.AreEqual("one two", values["first"]);
		Assert.AreEqual("a/b", values["encoded"]);
		Assert.AreEqual(string.Empty, values["flag"]);
		Assert.AreEqual(string.Empty, values["empty"]);
		CollectionAssert.AreEqual(new[] { "1", "2" }, values.GetValues("repeated"));
	}

	[TestMethod]
	public void ParseQueryIgnoresEmptySegmentsBetweenDelimiters()
	{
		var values = QueryStringHelper.ParseQuery("&&left=1&&right=2&");

		Assert.AreEqual(2, values.Count);
		Assert.AreEqual("1", values["left"]);
		Assert.AreEqual("2", values["right"]);
	}
}
