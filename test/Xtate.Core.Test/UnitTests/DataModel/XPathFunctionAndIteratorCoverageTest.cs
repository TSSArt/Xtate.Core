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

using System.IO;
using System.Xml.XPath;
using Xtate.DataModel;
using Xtate.DataModel.XPath.Functions;
using Xtate.DataModel.XPath.Internal;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathFunctionAndIteratorCoverageTest
{
	[TestMethod]
	public async Task InFunctionHandlesStringsEmptyInvalidAndNodeSetArguments()
	{
		var controller = new Mock<IInStateController>();
		controller.Setup(static c => c.InState(It.IsAny<IIdentifier>())).Returns(static (IIdentifier id) => id.Value is "A" or "B");
		var function = new InFunction { InStateControllerFactory = () => new ValueTask<IInStateController>(controller.Object) };
		await function.Initialize();

		Assert.AreEqual(expected: true, function.Invoke(null!, ["A"], null!));
		Assert.AreEqual(expected: false, function.Invoke(null!, [string.Empty], null!));
		Assert.AreEqual(expected: false, function.Invoke(null!, [17], null!));
		Assert.AreEqual(expected: false, function.Invoke(null!, [], null!));

		var navigator = CreateNavigator("<root><state>A</state><state>B</state></root>");
		Assert.AreEqual(expected: true, function.Invoke(null!, [navigator.Select("/root/state")], null!));
		Assert.AreEqual(expected: false, function.Invoke(null!, [navigator.Select("/root/missing")], null!));

		var mixed = CreateNavigator("<root><state>A</state><state>C</state></root>");
		Assert.AreEqual(expected: false, function.Invoke(null!, [mixed.Select("/root/state")], null!));
	}

	[TestMethod]
	public void StripRootsIteratorYieldsChildrenAcrossRootsAndClonesIndependentState()
	{
		var navigator = CreateNavigator("<roots><group><item>one</item><item>two</item></group><group/><group><item>three</item></group></roots>");
		var iterator = new XPathStripRootsIterator(navigator.Select("/roots/group"));
		var clone = iterator.Clone();

		Assert.IsNull(iterator.Current);
		Assert.AreEqual(expected: 0, iterator.CurrentPosition);
		Assert.IsTrue(iterator.MoveNext());
		Assert.AreEqual(expected: "one", iterator.Current!.Value);
		Assert.AreEqual(expected: 1, iterator.CurrentPosition);
		Assert.IsTrue(iterator.MoveNext());
		Assert.AreEqual(expected: "two", iterator.Current.Value);
		Assert.IsTrue(iterator.MoveNext());
		Assert.AreEqual(expected: "three", iterator.Current.Value);
		Assert.AreEqual(expected: 3, iterator.CurrentPosition);
		Assert.IsFalse(iterator.MoveNext());

		Assert.IsTrue(clone.MoveNext());
		Assert.AreEqual(expected: "one", clone.Current!.Value);
		Assert.AreEqual(expected: 1, clone.CurrentPosition);
	}

	private static XPathNavigator CreateNavigator(string xml) => new XPathDocument(new StringReader(xml)).CreateNavigator();
}