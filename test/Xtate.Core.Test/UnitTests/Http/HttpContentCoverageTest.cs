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
using System.Threading;
using Xtate.DataTypes;
using Xtate.Http;

namespace Xtate.Test.UnitTests.Http;

[TestClass]
public class HttpContentCoverageTest
{
	[TestMethod]
	public async Task JsonHttpContentSetsHeadersAndSerializesDataModelValue()
	{
		using var content = new TestJsonHttpContent(new DataModelValue("hello"));

		Assert.AreEqual(expected: "application/json", content.Headers.ContentType!.MediaType);
		Assert.AreEqual(Encoding.UTF8.WebName, content.Headers.ContentType!.CharSet);
		Assert.IsNull(content.Headers.ContentLength);

		var serialized = await content.ReadAsStringAsync();

		Assert.AreEqual(expected: "\"hello\"", serialized);

		using var legacyStream = new MemoryStream();
		await content.SerializeLegacy(legacyStream);
		Assert.AreEqual(expected: "\"hello\"", Encoding.UTF8.GetString(legacyStream.ToArray()));
		using var syncStream = new MemoryStream();
		content.SerializeSync(syncStream, CancellationToken.None);
		Assert.AreEqual(expected: "\"hello\"", Encoding.UTF8.GetString(syncStream.ToArray()));
	}

	[TestMethod]
	public async Task XmlHttpContentSetsHeadersAndSerializesDataModelValue()
	{
		using var content = new TestXmlHttpContent(new DataModelValue("hello"));

		Assert.AreEqual(MediaTypeNames.Text.Xml, content.Headers.ContentType!.MediaType);
		Assert.AreEqual(Encoding.UTF8.WebName, content.Headers.ContentType!.CharSet);
		Assert.IsNull(content.Headers.ContentLength);

		var serialized = await content.ReadAsStringAsync();

		Assert.AreEqual(expected: "hello", serialized);

		using var legacyStream = new MemoryStream();
		await content.SerializeLegacy(legacyStream);
		Assert.AreEqual(expected: "hello", Encoding.UTF8.GetString(legacyStream.ToArray()).TrimStart('\uFEFF'));
		using var syncStream = new MemoryStream();
		content.SerializeSync(syncStream, CancellationToken.None);
		Assert.AreEqual(expected: "hello", Encoding.UTF8.GetString(syncStream.ToArray()).TrimStart('\uFEFF'));
	}

	private sealed class TestJsonHttpContent(DataModelValue value) : JsonHttpContent(value)
	{
		public Task SerializeLegacy(Stream stream) => SerializeToStreamAsync(stream, context: null);

		public void SerializeSync(Stream stream, CancellationToken token) => SerializeToStreamAsync(stream, context: null).Wait(token);
	}

	private sealed class TestXmlHttpContent(DataModelValue value) : XmlHttpContent(value)
	{
		public Task SerializeLegacy(Stream stream) => SerializeToStreamAsync(stream, context: null);

		public void SerializeSync(Stream stream, CancellationToken token) => SerializeToStreamAsync(stream, context: null).Wait(token);
	}
}