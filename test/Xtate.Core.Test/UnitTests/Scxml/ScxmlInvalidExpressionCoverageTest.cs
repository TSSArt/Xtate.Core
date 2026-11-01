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
using System.Xml;
using Xtate.Ancestor.Extensions;
using Xtate.IoC;
using Xtate.Scxml;
using Xtate.Scxml.DependencyInjection;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Core.Test.UnitTests.Scxml;

[TestClass]
public class ScxmlInvalidExpressionCoverageTest
{
	[TestMethod]
	public async Task EqualSizedNamespaceScopesKeepTheirDistinctBindings()
	{
		const string xml = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" xmlns:p="urn:outer" version="1.0">
			  <datamodel><data id="outer" expr="p:node"/></datamodel>
			  <state id="state" xmlns:p="urn:inner"><datamodel><data id="inner" expr="p:node"/></datamodel></state>
			</scxml>
			""";
		await using var container = Container.Create<ScxmlModule>();
		var deserializer = await container.GetRequiredService<IScxmlDeserializer>();
		using var text = new StringReader(xml);
		using var reader = XmlReader.Create(text, new XmlReaderSettings { Async = true });
		var model = await deserializer.Deserialize(reader);
		var outer = model.DataModel!.Data[0].Expression!.UseAncestor.As<IXmlNamespacesInfo>().Namespaces;
		var inner = ((IState)model.States[0]).DataModel!.Data[0].Expression!.UseAncestor.As<IXmlNamespacesInfo>().Namespaces;
		Assert.AreEqual(outer.Length, inner.Length);
		Assert.AreEqual("urn:outer", outer.Single(value => value.Prefix == "p").Namespace);
		Assert.AreEqual("urn:inner", inner.Single(value => value.Prefix == "p").Namespace);
	}
	[TestMethod]
	public async Task ExpressionNamespaceAncestorsTrackShadowingWithoutChangingEarlierScopes()
	{
		const string xml = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" xmlns:p="urn:outer" version="1.0" datamodel="xpath">
			  <datamodel><data id="first" expr="p:node"/><data id="second" expr="p:node"/></datamodel>
			  <state id="state" xmlns:p="urn:inner" xmlns:q="urn:extra">
			    <onentry><log expr="p:node"/><log expr="q:node"/></onentry>
			  </state>
			</scxml>
			""";
		await using var container = Container.Create<ScxmlModule>();
		var deserializer = await container.GetRequiredService<IScxmlDeserializer>();
		using var text = new StringReader(xml);
		using var reader = XmlReader.Create(text, new XmlReaderSettings { Async = true });
		var model = await deserializer.Deserialize(reader);
		var outer = model.DataModel!.Data[0].Expression!.UseAncestor.As<IXmlNamespacesInfo>().Namespaces;
		var second = model.DataModel.Data[1].Expression!.UseAncestor.As<IXmlNamespacesInfo>().Namespaces;
		var inner = ((ILog)((IState)model.States[0]).OnEntry[0].Action[0]).Expression!.UseAncestor.As<IXmlNamespacesInfo>().Namespaces;
		Assert.AreEqual("urn:outer", outer.Single(value => value.Prefix == "p").Namespace);
		Assert.AreEqual("urn:outer", second.Single(value => value.Prefix == "p").Namespace);
		Assert.AreEqual("urn:inner", inner.Single(value => value.Prefix == "p").Namespace);
		Assert.AreEqual("urn:extra", inner.Single(value => value.Prefix == "q").Namespace);
	}

	[TestMethod]
	[DataRow("<state><transition event=''/></state>")]
	[DataRow("<state><transition event=' &#x9; '/></state>")]
	[DataRow("<state><transition cond=''/></state>")]
	[DataRow("<state><transition target=''/></state>")]
	[DataRow("<state><onentry><assign location='' expr='1'/></onentry></state>")]
	[DataRow("<state><onentry><send event='test' namelist=''/></onentry></state>")]
	[DataRow("<state><onentry><send event='test' namelist=' &#x9; '/></onentry></state>")]
	public async Task EmptyExpressionAttributesAreReportedAsValidationErrors(string body)
	{
		await using var container = Container.Create<ScxmlModule>();
		var deserializer = await container.GetRequiredService<IScxmlDeserializer>();
		using var text = new StringReader("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'>" + body + "</scxml>");
		using var reader = XmlReader.Create(text, new XmlReaderSettings { Async = true });
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(async () => await deserializer.Deserialize(reader));
	}
}
