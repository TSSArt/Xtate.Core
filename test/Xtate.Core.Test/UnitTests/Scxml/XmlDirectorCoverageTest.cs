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
using Xtate.Scxml.Services;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class XmlDirectorCoverageTest
{
	[TestMethod]
	public async Task PopulateAppliesPoliciesAndReportsStructuralErrors()
	{
		const string xml =
			"<root xmlns:p='urn:test' required='yes' optional='value'>" +
			"<optional/><single/><multiple/><multiple/><unknown/>" +
			"</root>";
		var director = CreateDirector(xml, useAsync: true);
		var model = await director.PopulateModel();

		Assert.AreEqual("yes", model.Required);
		Assert.AreEqual("value", model.Optional);
		CollectionAssert.AreEqual(new[] { "optional", "single", "multiple", "multiple" }, model.Elements);
		CollectionAssert.Contains(director.Namespaces, "p");
		Assert.IsNotEmpty(director.Errors);
	}

	[TestMethod]
	public async Task PopulateReportsMissingRequiredMembersAndCapturesRawContent()
	{
		var invalid = CreateDirector("<wrong><unknown/></wrong>", useAsync: true);
		_ = await invalid.PopulateModel();

		Assert.IsGreaterThanOrEqualTo(2, invalid.Errors.Count);

		var raw = CreateDirector("<raw><child>text</child></raw>", useAsync: true);
		var model = await raw.PopulateRaw();

		StringAssert.Contains(model.RawContent, "<child>text</child>");
	}

	[TestMethod]
	public async Task ReaderHelpersUseSyncAndAsyncReaderOperations()
	{
		var syncSkip = CreateDirector("<root><skip><child/></skip><after/></root>", useAsync: false);
		syncSkip.MoveToFirstElement();
		syncSkip.MoveToFirstElement();
		await syncSkip.SkipCurrent();
		Assert.AreEqual("after", syncSkip.Name);

		var syncOuter = CreateDirector("<outer><child/></outer>", useAsync: false);
		syncOuter.MoveToFirstElement();
		StringAssert.Contains(await syncOuter.ReadOuter(), "<outer>");

		var asyncOuter = CreateDirector("<outer><child/></outer>", useAsync: true);
		await asyncOuter.MoveToFirstElementAsync();
		StringAssert.Contains(await asyncOuter.ReadOuter(), "<outer>");
	}

	private static CoverageDirector CreateDirector(string xml, bool useAsync)
	{
		var settings = new XmlReaderSettings { Async = useAsync };

		return new CoverageDirector(XmlReader.Create(new StringReader(xml), settings));
	}

	private sealed class CoverageDirector : XmlDirector<CoverageDirector>
	{
		private readonly XmlReader _reader;

		private static readonly Policy<Model> ModelPolicy = BuildPolicy<Model>(builder =>
		{
			builder.IgnoreUnknownElements(false)
				   .ValidateElementName("root")
				   .RequiredAttribute("required", static (director, model) => model.Required = director.AttributeValue)
				   .OptionalAttribute("optional", static (director, model) => model.Optional = director.AttributeValue)
				   .OptionalElement("optional", VisitElement)
				   .SingleElement("single", VisitElement)
				   .MultipleElements("multiple", VisitElement)
				   .IgnoreUnknownElements()
				   .DenyUnknownElements();
		});

		private static readonly Policy<Model> RawPolicy = BuildPolicy<Model>(builder =>
			builder.ValidateElementName("raw").RawContent(static (director, model) => model.RawContent = director.RawContent));

		public CoverageDirector(XmlReader reader) : base(reader)
		{
			_reader = reader;
			ModelPolicy.FillNameTable(reader.NameTable);
			RawPolicy.FillNameTable(reader.NameTable);
		}

		public List<string> Errors { get; } = [];

		public List<string> Namespaces { get; } = [];

		public string Name => CurrentName;

		public ValueTask<Model> PopulateModel() => Populate(new Model(), ModelPolicy);

		public ValueTask<Model> PopulateRaw() => Populate(new Model(), RawPolicy);

		public ValueTask SkipCurrent() => Skip();

		public ValueTask<string> ReadOuter() => ReadOuterXml();

		public void MoveToFirstElement()
		{
			while (_reader.Read() && _reader.NodeType != XmlNodeType.Element) { }
		}

		public async ValueTask MoveToFirstElementAsync()
		{
			while (await _reader.ReadAsync() && _reader.NodeType != XmlNodeType.Element) { }
		}

		protected override void NamespaceAttribute(string prefix)
		{
			base.NamespaceAttribute(prefix);
			Namespaces.Add(prefix);
		}

		protected override void OnError(string message, Exception? exception) => Errors.Add(message);

		private static async ValueTask VisitElement(CoverageDirector director, Model model)
		{
			model.Elements.Add(director.CurrentName);
			await director.Skip();
		}
	}

	private sealed class Model
	{
		public string? Required { get; set; }

		public string? Optional { get; set; }

		public string? RawContent { get; set; }

		public List<string> Elements { get; } = [];
	}
}
