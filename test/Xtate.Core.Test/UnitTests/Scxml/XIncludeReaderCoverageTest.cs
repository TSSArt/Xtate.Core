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

using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Xml;
using Xtate.ResourceLoaders;
using Xtate.Scxml;
using Xtate.Scxml.Services;

namespace Xtate.Core.Test.UnitTests.Scxml;

[TestClass]
public class XIncludeReaderCoverageTest
{
	[TestMethod]
	public async Task ReaderIncludesTextAndXmlResourcesAsynchronously()
	{
		var resolver = new MemoryResolver();
		using var reader = CreateReader(resolver, useAsync: true);
		var nodes = new List<string>();

		while (await reader.ReadAsync())
		{
			nodes.Add($"{reader.NodeType}:{reader.LocalName}:{reader.Value}");
		}

		Assert.IsTrue(nodes.Any(static node => node.Contains(value: "Element:included:", StringComparison.Ordinal)));
		Assert.IsTrue(nodes.Any(static node => node.Contains(value: "Text::xml text", StringComparison.Ordinal)));
	}

	[TestMethod]
	public void ReaderIncludesResourcesSynchronouslyAndClosesNestedReader()
	{
		var resolver = new MemoryResolver();
		using var reader = CreateReader(resolver, useAsync: false);

		while (reader.Read() && reader.NodeType != XmlNodeType.Text) { }

		Assert.AreEqual(expected: "plain text", reader.Value);
		Assert.IsGreaterThan(lowerBound: 0, reader.Depth);
		Assert.AreEqual(expected: "text/plain", resolver.Accept);
		Assert.AreEqual(expected: "en-US", resolver.AcceptLanguage);
		reader.Close();
		Assert.AreEqual(ReadState.Closed, reader.ReadState);
	}

	[TestMethod]
	public async Task ReaderShouldIncludePlainTextAsynchronously()
	{
		using var reader = CreateReader(new MemoryResolver(), useAsync: true, includeText: true);

		while (await reader.ReadAsync()) { }
	}

	[TestMethod]
	public void ReaderShouldIncludeXmlMediaAsTextSynchronously()
	{
		using var reader = CreateReader(new MemoryResolver(), useAsync: false, includeXmlAsText: true);

		while (reader.Read()) { }
	}

	[TestMethod]
	[DataRow(false, false)]
	[DataRow(false, true)]
	[DataRow(true, false)]
	[DataRow(true, true)]
	public async Task StandardResolverCanReturnStreamOrResource(bool useAsync, bool resource)
	{
		using var reader = CreateReader(new StandardResolver(resource), useAsync);
		var values = new List<string>();
		while (useAsync ? await reader.ReadAsync() : reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Text) values.Add(reader.Value);
		}
		CollectionAssert.AreEqual(new[] { useAsync ? "included" : "plain" }, values);
	}

	[TestMethod]
	[DataRow("null", false)]
	[DataRow("null", true)]
	[DataRow("load", false)]
	[DataRow("load", true)]
	[DataRow("uri", false)]
	[DataRow("resolve", true)]
	public async Task ResolverFailuresAreWrappedAsXIncludeExceptions(string failure, bool useAsync)
	{
		using var reader = CreateReader(new StandardResolver(false, failure), useAsync);
		await Assert.ThrowsExactlyAsync<XIncludeException>(async () =>
		{
			while (useAsync ? await reader.ReadAsync() : reader.Read()) { }
		});
	}

	[TestMethod]
	public async Task AsyncXmlAsTextPreservesMarkupAndIgnoresNamespacedExtensionAttributes()
	{
		const string include = "<xi:include xmlns:p='urn:extension' p:ignored='value' href='included.xml' parse='text'/>";
		using var reader = CreateReader(new MemoryResolver(), true, includeOverride: include);
		var values = new List<string>();
		while (await reader.ReadAsync())
		{
			if (reader.NodeType == XmlNodeType.Text) values.Add(reader.Value);
		}
		StringAssert.Contains(string.Concat(values), "<included>xml text</included>");
	}

	[TestMethod]
	public void NamespacedIncludeExtensionAttributesDoNotAffectTextLoading()
	{
		const string include = "<xi:include xmlns:p='urn:extension' p:ignored='value' href='text.txt' parse='text'/>";
		using var reader = CreateReader(new MemoryResolver(), false, includeOverride: include);
		var values = new List<string>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Text) values.Add(reader.Value);
		}
		Assert.AreEqual("plain text", string.Concat(values));
	}

	private sealed class StandardResolver(bool resource, string? failure = null) : XmlResolver
	{
		public override ICredentials? Credentials { set { } }

		public override bool SupportsType(Uri absoluteUri, Type? type) => resource;

		public override Uri ResolveUri(Uri? baseUri, string? relativeUri) => failure switch
		{
			"uri" => throw new UriFormatException("invalid URI"),
			"resolve" => throw new InvalidOperationException("cannot resolve"),
			_ => base.ResolveUri(baseUri, relativeUri)
		};

		public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
		{
			if (failure == "null") return null;
			if (failure == "load") throw new IOException("load failed");
			var xml = absoluteUri.AbsolutePath.EndsWith("included.xml", StringComparison.Ordinal);
			var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml ? "<included>included</included>" : "plain"));
			return resource ? new Resource(stream, new ContentType(xml ? "application/xml" : "text/plain")) : stream;
		}

		public override Task<object> GetEntityAsync(Uri absoluteUri, string? role, Type? ofObjectToReturn) =>
			Task.FromResult(GetEntity(absoluteUri, role, ofObjectToReturn)!);
	}

	private static XIncludeReader CreateReader(XmlResolver resolver,
											   bool useAsync,
											   bool includeText = false,
											   bool includeXmlAsText = false,
											   string? includeOverride = null)
	{
		var include = includeXmlAsText
			? "<xi:include href='included.xml' parse='text'/>"
			: includeText || !useAsync
				? "<xi:include href='text.txt' parse='text' encoding='utf-8' accept='text/plain' accept-language='en-US'/>"
				: "<xi:include href='included.xml'/>";
		var source = "<root xmlns:xi='http://www.w3.org/2001/XInclude'>" + (includeOverride ?? include) + "</root>";
		var settings = new XmlReaderSettings { Async = useAsync, XmlResolver = resolver };
		var innerReader = XmlReader.Create(new StringReader(source), settings, baseUri: "https://example.test/root.xml");

		return new XIncludeReader(innerReader, reader => new XmlBaseReader(reader) { XmlResolver = resolver })
			   {
				   ResourceFactory = static (stream, contentType) => new Resource(stream, contentType),
				   XmlResolver = resolver,
				   XIncludeOptions = new IncludeOptions()
			   };
	}

	private sealed class MemoryResolver : XmlResolver, IExternalEntityGetter
	{
		public string? Accept { get; private set; }

		public string? AcceptLanguage { get; private set; }

		public override ICredentials? Credentials
		{
			set { }
		}

	#region Interface IExternalEntityGetter

		public override bool SupportsType(Uri absoluteUri, Type? type) => type == typeof(Resource);

		public object GetEntity(Uri absoluteUri, NameValueCollection? headers, Type? ofObjectToReturn)
		{
			Accept = headers?["Accept"];
			AcceptLanguage = headers?["Accept-Language"];
			var isXml = absoluteUri.AbsolutePath.EndsWith(value: "included.xml", StringComparison.Ordinal);
			var text = isXml ? "<included>xml text</included>" : "plain text";
			var contentType = new ContentType(isXml ? "application/xml" : "text/plain") { CharSet = "utf-8" };

			return new Resource(new MemoryStream(Encoding.UTF8.GetBytes(text)), contentType);
		}

		public ValueTask<object> GetEntityAsync(Uri absoluteUri, NameValueCollection? headers, Type? ofObjectToReturn) => new(GetEntity(absoluteUri, headers, ofObjectToReturn));

	#endregion

		public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn) => GetEntity(absoluteUri, headers: null, ofObjectToReturn);
	}

	private sealed record IncludeOptions : IXIncludeOptions
	{
	#region Interface IXIncludeOptions

		public bool XIncludeAllowed => true;

		public int MaxNestingLevel => 10;

	#endregion
	}
}
