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
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Xml;
using Xtate.DataModel.XPath.Services;
using Xtate.IoC.Tools;
using Xtate.Logging.Services;
using Xtate.NameTable;
using Xtate.ResourceLoaders;
using Xtate.Scxml;
using Xtate.Scxml.Services;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class XmlAndConsoleInfrastructureCoverageTest
{
	[TestMethod]
	public void XPathParserContextUsesProvidedOrFallbackNameTableAndAncestorNamespaces()
	{
		var nameTable = new System.Xml.NameTable();
		var source = new NamespaceSource
					 {
						 Namespaces = [("a", "urn:a"), ("b", "urn:b")]
					 };
		var providedFactory = new XPathXmlParserContextFactory
						  {
							  NameTableProvider = Mock.Of<INameTableProvider>(provider => provider.GetNameTable() == nameTable)
						  };

		var provided = providedFactory.CreateContext(source);
		var fallback = new XPathXmlParserContextFactory { NameTableProvider = null }.CreateContext(new object());

		Assert.AreSame(nameTable, provided.NameTable);
		Assert.AreEqual("urn:a", provided.NamespaceManager!.LookupNamespace("a"));
		Assert.AreEqual("urn:b", provided.NamespaceManager.LookupNamespace("b"));
		Assert.IsInstanceOfType<System.Xml.NameTable>(fallback.NameTable);
		Assert.AreEqual(XmlSpace.None, fallback.XmlSpace);
	}

	[TestMethod]
	public async Task RedirectResolverLoadsStreamsAndResourcesAndRejectsUnsupportedRequests()
	{
		var uri = new Uri("https://example.test/entity.xml");
		var headers = new NameValueCollection { ["X-Test"] = "value" };
		var loader = new Mock<IResourceLoader>();
		loader.SetupSequence(l => l.Request(uri, It.IsAny<NameValueCollection?>()))
			  .ReturnsAsync(new Resource(new MemoryStream([1, 2]), new ContentType("text/plain")))
			  .ReturnsAsync(new Resource(new MemoryStream([3, 4]), new ContentType("application/xml")));
		var resolver = CreateResolver(loader.Object, allowed: true);

		Assert.IsTrue(resolver.SupportsType(uri, typeof(Stream)));
		Assert.IsTrue(resolver.SupportsType(uri, typeof(Resource)));
		Assert.IsFalse(resolver.SupportsType(uri, typeof(string)));

		await using var stream = (Stream) await resolver.GetEntityAsync(uri, role: null, typeof(Stream));
		var bytes = new byte[2];
		Assert.AreEqual(expected: 2, await stream.ReadAsync(bytes));
		CollectionAssert.AreEqual(new byte[] { 1, 2 }, bytes);

		await using var resource = (Resource) await resolver.GetEntityAsync(uri, headers, typeof(Resource));
		CollectionAssert.AreEqual(new byte[] { 3, 4 }, await resource.GetBytes());
		Assert.AreEqual("application/xml", resource.ContentType!.MediaType);
		loader.Verify(l => l.Request(uri, headers), Times.Once);

		Assert.ThrowsExactly<NotSupportedException>(
			[ExcludeFromCodeCoverage] () => resolver.GetEntity(uri, role: null, typeof(Stream)));
		await Assert.ThrowsExactlyAsync<ArgumentException>(
			[ExcludeFromCodeCoverage] async () => await resolver.GetEntityAsync(uri, headers, typeof(string)));
		var disabled = CreateResolver(loader.Object, allowed: false);
		await Assert.ThrowsExactlyAsync<XmlException>(
			[ExcludeFromCodeCoverage] async () => await disabled.GetEntityAsync(uri, headers, typeof(Stream)));
	}

	[TestMethod]
	public void ConsoleTraceListenerPrivateInstanceUsesConsoleWriterAndClearsItOnDispose()
	{
		var nestedType = typeof(ConsoleLogProvider<>).GetNestedType("ConsoleTraceListener", BindingFlags.NonPublic);
		Assert.IsNotNull(nestedType);
		if (nestedType.ContainsGenericParameters)
		{
			nestedType = nestedType.MakeGenericType(typeof(ConsoleSource));
		}

		var listener = (TextWriterTraceListener?) Activator.CreateInstance(nestedType, nonPublic: true);
		Assert.IsNotNull(listener);
		Assert.AreSame(Console.Out, listener.Writer);

		listener.Dispose();

		Assert.IsNull(listener.Writer);
	}

	private static RedirectXmlResolver CreateResolver(IResourceLoader loader, bool allowed) =>
		new()
		{
			DisposeToken = new DisposeToken(CancellationToken.None),
			ResourceLoaderFactory = () => new ValueTask<IResourceLoader>(loader),
			ResourceFactory = static (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptionsFactory = () => new ValueTask<IXIncludeOptions?>(new XIncludeOptions(allowed))
		};

	private sealed class NamespaceSource : IXmlNamespacesInfo
	{
		public ImmutableArray<(string Prefix, string Namespace)> Namespaces { get; init; }
	}

	private sealed record XIncludeOptions(bool XIncludeAllowed) : IXIncludeOptions
	{
		public int MaxNestingLevel => 10;
	}

	private sealed class ConsoleSource;
}
