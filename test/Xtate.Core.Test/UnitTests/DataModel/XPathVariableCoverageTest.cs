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

using System.Xml.XPath;
using Xtate.DataModel.XPath.Services;
using Xtate.NameTable;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathVariableCoverageTest
{
	[TestMethod]
	public void UninitializedDescriptorReturnsReusableEmptyIteratorAndMetadata()
	{
		var descriptor = new XPathVarDescriptor { Name = "missing" };

		Assert.IsFalse(descriptor.IsLocal);
		Assert.IsFalse(descriptor.IsParam);
		Assert.AreEqual(XPathResultType.NodeSet, descriptor.VariableType);

		var iterator = (XPathNodeIterator) descriptor.Evaluate(null!);

		Assert.AreEqual(expected: 0, iterator.CurrentPosition);
		Assert.IsNull(iterator.Current);
		Assert.AreSame(iterator, iterator.Clone());
		Assert.IsFalse(iterator.MoveNext());
		Assert.AreSame(iterator, descriptor.Evaluate(null!));
	}

	[TestMethod]
	public async Task InitializedDescriptorDelegatesVariableCreationToEngine()
	{
		var descriptor = new XPathVarDescriptor { Name = "created" };
		var engine = new XPathEngine(dataModelController: null);

		await descriptor.Initialize(engine);
		var iterator = (XPathNodeIterator) descriptor.Evaluate(null!);

		Assert.IsTrue(iterator.MoveNext());
		var current = iterator.Current;
		Assert.IsNotNull(current);
		Assert.AreEqual("created", current.Name);
	}

	[TestMethod]
	public async Task CompiledExpressionExposesMetadataAndInitializesContextBeforeReturningExpression()
	{
		var nameTableProvider = new Mock<INameTableProvider>();
		nameTableProvider.Setup(static p => p.GetNameTable()).Returns(new System.Xml.NameTable());
		var context = new XPathExpressionContext(nameTableProvider.Object, xmlNamespacesInfo: null)
					  {
						  FunctionProviders = [],
						  XPathVarDescriptorFactory = static name => new XPathVarDescriptor { Name = name },
						  XPathEngineFactory = static () => new ValueTask<XPathEngine>(new XPathEngine(dataModelController: null))
					  };
		var compiled = new XPathCompiledExpression("1 + 1", xmlNamespacesInfo: null, _ => context);

		Assert.AreEqual("1 + 1", compiled.Expression);
		Assert.AreEqual(XPathResultType.Number, compiled.ReturnType);

		var first = await compiled.GetXPathExpression();
		var second = await compiled.GetXPathExpression();

		Assert.AreSame(first, second);
		Assert.AreEqual("1 + 1", first.Expression);
	}
}
