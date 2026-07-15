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
using System.Net.Mime;
using System.Text;
using Xtate.DataModel.Services;
using Xtate.DataModel.XPath.Internal;
using Xtate.DataTypes;
using Xtate.ResourceLoaders;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelConverterCoverageTest
{
	[TestMethod]
	public void ArrayAndObjectDetectionHonorsMetadataBeforeShapeHeuristics()
	{
		var empty = new DataModelList();
		var array = new DataModelList { "value" };
		var obj = new DataModelList { ["key"] = "value" };
		var forcedArray = DataModelConverter.CreateAsArray();
		forcedArray.Add("key", "value");
		var forcedObject = DataModelConverter.CreateAsObject();
		forcedObject.Add("value");

		Assert.IsFalse(DataModelConverter.IsArray(empty));
		Assert.IsFalse(DataModelConverter.IsObject(empty));
		Assert.IsTrue(DataModelConverter.IsArray(array));
		Assert.IsFalse(DataModelConverter.IsObject(array));
		Assert.IsFalse(DataModelConverter.IsArray(obj));
		Assert.IsTrue(DataModelConverter.IsObject(obj));
		Assert.IsTrue(DataModelConverter.IsArray(forcedArray));
		Assert.IsFalse(DataModelConverter.IsObject(forcedArray));
		Assert.IsFalse(DataModelConverter.IsArray(forcedObject));
		Assert.IsTrue(DataModelConverter.IsObject(forcedObject));
		Assert.AreEqual(expected: "[]", DataModelConverter.ToJson(DataModelConverter.CreateAsArray()));
		Assert.AreEqual(expected: "{}", DataModelConverter.ToJson(DataModelConverter.CreateAsObject()));
	}

	[TestMethod]
	public async Task JsonStreamByteSpanAndIndentedOverloadsRoundTripValues()
	{
		var value = new DataModelList { ["text"] = "value", ["number"] = 42, ["flag"] = true };
		var indented = DataModelConverter.ToJson(value, DataModelConverter.JsonOptions.WriteIndented);
		StringAssert.Contains(indented, Environment.NewLine);

		var bytes = DataModelConverter.ToJsonUtf8Bytes(value);
		var fromSpan = DataModelConverter.FromJson(bytes.AsSpan()).AsList();
		Assert.AreEqual(expected: "value", fromSpan["text"].AsString());

		using var syncStream = new MemoryStream();
		DataModelConverter.ToJson(syncStream, value);
		syncStream.Position = 0;
		var fromSyncStream = await DataModelConverter.FromJsonAsync(syncStream);
		Assert.AreEqual(expected: 42d, fromSyncStream.AsList()["number"].AsNumber().ToDouble());

		using var asyncStream = new MemoryStream();
		await DataModelConverter.ToJsonAsync(asyncStream, value, DataModelConverter.JsonOptions.WriteIndented);
		asyncStream.Position = 0;
		var fromAsyncStream = await DataModelConverter.FromJsonAsync(asyncStream);
		Assert.IsTrue(fromAsyncStream.AsList()["flag"].AsBoolean());
	}

	[TestMethod]
	public async Task JsonResourceLoadingUsesUtf8StreamAndNonUtf8ContentPaths()
	{
		var json = "{\"text\":\"zażółć\"}";
		await using var utf8 = new Resource(
			new MemoryStream(Encoding.UTF8.GetBytes(json)),
			new ContentType("application/json") { CharSet = "utf-8" });
		await using var utf16 = new Resource(
			new MemoryStream(Encoding.Unicode.GetBytes(json)),
			new ContentType("application/json") { CharSet = "utf-16" });

		Assert.AreEqual(expected: "zażółć", (await DataModelConverter.FromJsonContentAsync(utf8)).AsList()["text"].AsString());
		Assert.AreEqual(expected: "zażółć", (await DataModelConverter.FromJsonContentAsync(utf16)).AsList()["text"].AsString());
	}

	[TestMethod]
	public async Task XmlStringByteAndStreamOverloadsRoundTripTypedValues()
	{
		var value = new DataModelList
					{
						["text"] = "value",
						["number"] = 42,
						["flag"] = true,
						["null"] = DataModelValue.Null,
						["undefined"] = DataModelValue.Undefined,
						["date"] = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2))
					};
		var xml = DataModelConverter.ToXml(value, DataModelConverter.XmlOptions.WriteIndented);
		StringAssert.Contains(xml, Environment.NewLine);
		var fromString = DataModelConverter.FromXml(xml).AsList();
		Assert.AreEqual(expected: "value", fromString["text"].AsString());
		Assert.AreEqual(expected: 42d, fromString["number"].AsNumber().ToDouble());
		Assert.IsTrue(fromString["flag"].AsBoolean());
		Assert.AreEqual(DataModelValueType.Null, fromString["null"].Type);
		Assert.IsTrue(fromString["undefined"].IsUndefined());

		var bytes = DataModelConverter.ToXmlUtf8Bytes(value);
		Assert.AreEqual(expected: "value", DataModelConverter.FromXml(bytes.AsSpan()).AsList()["text"].AsString());

		using var syncStream = new MemoryStream();
		DataModelConverter.ToXml(syncStream, value);
		syncStream.Position = 0;
		Assert.IsTrue((await DataModelConverter.FromXmlAsync(syncStream)).AsList()["flag"].AsBoolean());

		using var asyncStream = new MemoryStream();
		await DataModelConverter.ToXmlAsync(asyncStream, value, DataModelConverter.XmlOptions.WriteIndented);
		asyncStream.Position = 0;
		Assert.AreEqual(expected: 42d, (await DataModelConverter.FromXmlAsync(asyncStream)).AsList()["number"].AsNumber().ToDouble());
	}

	[TestMethod]
	public async Task XmlStringByteSyncStreamAndAsyncWriterOverloadsRoundTripTypedValues()
	{
		var value = new DataModelList { ["text"] = "value", ["number"] = 42, ["flag"] = true, ["null"] = DataModelValue.Null };
		var xml = DataModelConverter.ToXml(value, DataModelConverter.XmlOptions.WriteIndented);
		StringAssert.Contains(xml, Environment.NewLine);
		var fromString = DataModelConverter.FromXml(xml).AsList();
		Assert.AreEqual(expected: "value", fromString["text"].AsString());
		Assert.AreEqual(expected: 42d, fromString["number"].AsNumber().ToDouble());

		var bytes = DataModelConverter.ToXmlUtf8Bytes(value);
		Assert.IsTrue(DataModelConverter.FromXml(bytes.AsSpan()).AsList()["flag"].AsBoolean());

		using var syncStream = new MemoryStream();
		DataModelConverter.ToXml(syncStream, value);
		syncStream.Position = 0;
		using var syncReader = new StreamReader(syncStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
		Assert.AreEqual(expected: 42d, DataModelConverter.FromXml(await syncReader.ReadToEndAsync()).AsList()["number"].AsNumber().ToDouble());

		using var asyncStream = new MemoryStream();
		await DataModelConverter.ToXmlAsync(asyncStream, value, DataModelConverter.XmlOptions.WriteIndented);
		Assert.IsTrue(DataModelConverter.FromXml(asyncStream.ToArray().AsSpan()).AsList()["flag"].AsBoolean());
	}

	[TestMethod]
	public void XmlKeyTypeStringBufferAndSpanHelpersCoverScalarShapes()
	{
		Assert.AreEqual(expected: "item", XmlConverter.KeyToLocalName(key: null));
		Assert.AreEqual(expected: "empty", XmlConverter.KeyToLocalName(string.Empty));
		Assert.AreEqual(expected: "a_x0020_b", XmlConverter.KeyToLocalName("a b"));
		Assert.IsNull(XmlConverter.NsNameToKey(XmlConverter.XPathElementNamespace, "item"));
		Assert.AreEqual(string.Empty, XmlConverter.NsNameToKey(XmlConverter.XPathElementNamespace, "empty"));
		Assert.AreEqual(expected: "a b", XmlConverter.NsNameToKey(ns: string.Empty, "a_x0020_b"));
		Assert.AreEqual(XmlConverter.XPathElementNamespace, XmlConverter.KeyToNamespaceOrDefault(key: null));
		Assert.AreEqual(XmlConverter.XPathElementNamespace, XmlConverter.KeyToNamespaceOrDefault(string.Empty));
		Assert.IsNull(XmlConverter.KeyToNamespaceOrDefault("key"));
		Assert.AreEqual(expected: "x", XmlConverter.KeyToPrefixOrDefault(key: null));
		Assert.IsNull(XmlConverter.KeyToPrefixOrDefault("key"));

		DataModelValue[] values =
		[
			DataModelValue.Undefined,
			DataModelValue.Null,
			new DataModelValue("text"),
			new DataModelValue(42),
			new DataModelValue(4_000_000_000L),
			new DataModelValue(1.25d),
			new DataModelValue(true),
			new DataModelValue(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc)),
			new DataModelValue(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)))
		];

		foreach (var value in values)
		{
			var buffer = new char[Math.Max(XmlConverter.GetBufferSizeForValue(value), 1)];
			var count = XmlConverter.WriteValueToSpan(value, buffer);
			Assert.AreEqual(XmlConverter.ToString(value), new string(buffer, startIndex: 0, count));
		}

		Assert.AreEqual(expected: "bool", XmlConverter.GetTypeValue(new DataModelValue(true)).AsString());
		Assert.AreEqual(expected: "number", XmlConverter.GetTypeValue(new DataModelValue(1)).AsString());
		Assert.AreEqual(expected: "datetime", XmlConverter.GetTypeValue(new DataModelValue(DateTimeOffset.UtcNow)).AsString());
		Assert.AreEqual(expected: "null", XmlConverter.GetTypeValue(DataModelValue.Null).AsString());
		Assert.AreEqual(expected: "undefined", XmlConverter.GetTypeValue(DataModelValue.Undefined).AsString());
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => XmlConverter.GetTypeValue(new DataModelValue("text")));
	}

	[TestMethod]
	public async Task XmlParserReadsExplicitTypesMetadataEmptyElementsAndAsyncContent()
	{
		const string xml = "<root xmlns:x='http://xtate.net/xpath' attr='value'><b x:type='bool'>true</b><n x:type='number'>1.5</n><d x:type='datetime'>2026-01-02T03:04:05+02:00</d><z x:type='null'/><u x:type='undefined'/><empty/></root>";
		var root = DataModelConverter.FromXml(xml).AsList()["root"].AsList();
		Assert.IsTrue(root["b"].AsBoolean());
		Assert.AreEqual(expected: 1.5d, root["n"].AsNumber().ToDouble());
		Assert.AreEqual(DataModelValueType.Null, root["z"].Type);
		Assert.IsTrue(root["u"].IsUndefined());
		Assert.AreEqual(string.Empty, root["empty"].AsString());

		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
		var asyncRoot = (await DataModelConverter.FromXmlAsync(stream)).AsList()["root"].AsList();
		Assert.IsTrue(asyncRoot["b"].AsBoolean());
		Assert.AreEqual(expected: 1.5d, asyncRoot["n"].AsNumber().ToDouble());
	}

	[TestMethod]
	public void XmlParserReadsExplicitTypesMetadataAndEmptyElementsSynchronously()
	{
		const string xml = "<root xmlns:x='http://xtate.net/xpath' attr='value'><b x:type='bool'>true</b><n x:type='number'>1.5</n><d x:type='datetime'>2026-01-02T03:04:05+02:00</d><z x:type='null'/><u x:type='undefined'/><empty/></root>";
		var parsed = DataModelConverter.FromXml(xml).AsList();
		var rootValue = parsed["root"];
		var root = rootValue.AsList();

		Assert.IsNotNull(parsed.GetMetadata("root"));
		Assert.IsTrue(root["b"].AsBoolean());
		Assert.AreEqual(expected: 1.5d, root["n"].AsNumber().ToDouble());
		Assert.AreEqual(DataModelValueType.DateTime, root["d"].Type);
		Assert.AreEqual(DataModelValueType.Null, root["z"].Type);
		Assert.IsTrue(root["u"].IsUndefined());
		Assert.AreEqual(string.Empty, root["empty"].AsString());
	}
}
