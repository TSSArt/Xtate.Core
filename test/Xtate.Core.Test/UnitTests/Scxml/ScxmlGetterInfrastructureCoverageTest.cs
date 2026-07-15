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

using System.Xml;
using Xtate.Interpreter;
using Xtate.NameTable;
using Xtate.Scxml;
using Xtate.Scxml.Services;
using Xtate.StateMachine.Validator;
using System.Reflection;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class ScxmlGetterInfrastructureCoverageTest
{
	[TestMethod]
	public void LocationGetterCreatesAsyncDtdSettingsAndParserContextWithLocation()
	{
		var nameTable = new System.Xml.NameTable();
		var resolver = new XmlUrlResolver();
		var getter = new TestLocationGetter
					 {
						 XmlResolver = resolver,
						 StateMachineLocation = Mock.Of<IStateMachineLocation>(static value => value.Location == new Uri("https://example.test/machine.scxml")),
						 ScxmlDeserializer = Mock.Of<IScxmlDeserializer>(),
						 NameTableProvider = Mock.Of<INameTableProvider>(provider => provider.GetNameTable() == nameTable),
						 StateMachineValidator = Mock.Of<IStateMachineValidator>()
					 };

		var settings = getter.Settings();
		var context = getter.Context();

		Assert.IsTrue(settings.Async);
		Assert.AreEqual(DtdProcessing.Parse, settings.DtdProcessing);
		Assert.AreSame(nameTable, context.NameTable);
		Assert.AreEqual("https://example.test/machine.scxml", context.BaseURI);
		Assert.AreEqual(XmlSpace.None, context.XmlSpace);
		Assert.AreEqual("https://example.test/machine.scxml", typeof(ScxmlLocationStateMachineGetter).GetProperty("Location", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(getter));
	}

	[TestMethod]
	public void LocationGetterRejectsMissingLocation()
	{
		var getter = new TestLocationGetter
					 {
						 XmlResolver = new XmlUrlResolver(),
						 StateMachineLocation = Mock.Of<IStateMachineLocation>(),
						 ScxmlDeserializer = Mock.Of<IScxmlDeserializer>(),
						 NameTableProvider = Mock.Of<INameTableProvider>(),
						 StateMachineValidator = Mock.Of<IStateMachineValidator>()
					 };

		try
		{
			_ = typeof(ScxmlLocationStateMachineGetter).GetProperty("Location", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(getter);
			Assert.Fail("A missing location did not fail.");
		}
		catch (TargetInvocationException exception)
		{
			Assert.IsNotNull(exception.InnerException);
		}
	}

	[TestMethod]
	public void ReaderGetterCreatesCloseInputSettingsAndEmptyOrLocatedParserContexts()
	{
		var nameTable = new System.Xml.NameTable();
		var resolver = new XmlUrlResolver();
		var getter = CreateReaderGetter(nameTable, resolver, location: null);

		var settings = getter.Settings();
		var emptyContext = getter.Context();

		Assert.IsTrue(settings.Async);
		Assert.IsTrue(settings.CloseInput);
		Assert.AreEqual(DtdProcessing.Parse, settings.DtdProcessing);
		Assert.AreEqual(string.Empty, emptyContext.BaseURI);

		var locatedGetter = CreateReaderGetter(
			nameTable,
			resolver,
			Mock.Of<IStateMachineLocation>(static value => value.Location == new Uri("file:///machine.scxml")));
		Assert.AreEqual("file:///machine.scxml", locatedGetter.Context().BaseURI);
	}

	private static TestReaderGetter CreateReaderGetter(System.Xml.NameTable nameTable, XmlResolver resolver, IStateMachineLocation? location) =>
		new()
		{
			ScxmlDeserializer = Mock.Of<IScxmlDeserializer>(),
			ScxmlStateMachine = Mock.Of<IScxmlStateMachine>(),
			XmlResolver = resolver,
			StateMachineLocation = location,
			NameTableProvider = Mock.Of<INameTableProvider>(provider => provider.GetNameTable() == nameTable),
			StateMachineValidator = Mock.Of<IStateMachineValidator>()
		};

	private sealed class TestLocationGetter : ScxmlLocationStateMachineGetter
	{
		public XmlReaderSettings Settings() => GetXmlReaderSettings();

		public XmlParserContext Context() => GetXmlParserContext();
	}

	private sealed class TestReaderGetter : ScxmlReaderStateMachineGetter
	{
		public XmlReaderSettings Settings() => GetXmlReaderSettings();

		public XmlParserContext Context() => GetXmlParserContext();
	}
}
