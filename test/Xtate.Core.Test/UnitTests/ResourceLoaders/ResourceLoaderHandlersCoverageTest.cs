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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Xtate.IoBoundTask;
using Xtate.IoC.Tools;
using Xtate.Interpreter;
using Xtate.ResourceLoaders;
using Xtate.ResourceLoaders.File.Services;
using Xtate.ResourceLoaders.Resx.Services;
using Xtate.ResourceLoaders.Services;
using Xtate.ResourceLoaders.Web.Services;

namespace Xtate.Test.UnitTests.ResourceLoaders;

[TestClass]
public class ResourceLoaderHandlersCoverageTest
{
	[TestMethod]
	public async Task FileResourceLoaderReadsRelativeAndAbsoluteFileUris()
	{
		var name = System.Reflection.Assembly.GetAssembly(typeof(ResourceLoaderHandlersCoverageTest))!.GetName().Name;
		var relativePath = name + @".runtime" + "config.json";
		var loader = CreateFileLoader();

		await using var relativeResource = await loader.Request(new Uri(relativePath, UriKind.Relative), headers: null);
		var relativeContent = await relativeResource.GetContent();
		StringAssert.Contains(relativeContent, "runtimeOptions");

		var absoluteUri = new Uri(Path.GetFullPath(relativePath));
		await using var absoluteResource = await loader.Request(absoluteUri, new NameValueCollection { ["Ignored"] = "header" });
		var absoluteContent = await absoluteResource.GetContent();
		StringAssert.Contains(absoluteContent, "runtimeOptions");
	}

	[TestMethod]
	public async Task FileResourceLoaderReadsPathsRelativeToWorkingDirectoryAndAbsoluteFileUris()
	{
		const string relativePath = "Xtate.Core.Test.dll";
		var loader = CreateFileLoader();

		await using var relativeResource = await loader.Request(new Uri(relativePath, UriKind.Relative), headers: null);
		var relativeBytes = await relativeResource.GetBytes();
		Assert.IsGreaterThan(0, relativeBytes.Length);

		var absoluteUri = new Uri(Path.GetFullPath(relativePath));
		await using var absoluteResource = await loader.Request(absoluteUri, new NameValueCollection { ["Ignored"] = "header" });
		var absoluteBytes = await absoluteResource.GetBytes();
		CollectionAssert.AreEqual(relativeBytes, absoluteBytes);
	}

	[TestMethod]
	public void FileResourceLoaderCreateFileStreamRejectsNullPath()
	{
		var loader = new TestFileResourceLoader
					 {
						 ExternalResources = Mock.Of<IIoBoundTask>(),
						 ResourceFactory = [ExcludeFromCodeCoverage] (stream, contentType) => new ValueTask<Resource>(new Resource(stream, contentType))
					 };

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () => loader.Open(null!));
	}

	[TestMethod]
	public async Task FileResourceLoaderProviderMatchesRelativeFileAndUncUris()
	{
		var loader = CreateFileLoader();
		IResourceLoaderProvider provider = new FileResourceLoader.Provider { ResourceLoaderFactory = () => new ValueTask<FileResourceLoader>(loader) };

		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("relative.scxml", UriKind.Relative)));
		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("file:///C:/state.scxml")));
		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("file://server/share/state.scxml")));
		Assert.IsNull(await provider.TryGetResourceLoader(new Uri("https://example.test/state.scxml")));
	}

	[TestMethod]
	public async Task WebResourceLoaderRequestsContentAndForwardsHeaders()
	{
		var handler = new CapturingHttpMessageHandler();
		var loader = new WebResourceLoader
					 {
						 DisposeToken = new DisposeToken(CancellationToken.None),
						 HttpClientFactory = () => new HttpClient(handler, disposeHandler: false),
						 ResourceFactory = (stream, contentType) => new Resource(stream, contentType)
					 };
		var headers = new NameValueCollection { ["X-Test"] = "first", ["X-Second"] = "second" };

		await using var resource = await loader.Request(new Uri("https://example.test/resource"), headers);

		Assert.AreEqual("web payload", await resource.GetContent());
		Assert.AreEqual("text/plain", resource.ContentType!.MediaType);
		Assert.AreEqual(HttpMethod.Get, handler.Request!.Method);
		Assert.AreEqual(new Uri("https://example.test/resource"), handler.Request.RequestUri);
		Assert.AreEqual("first", handler.Request.Headers.GetValues("X-Test").Single());
		Assert.AreEqual("second", handler.Request.Headers.GetValues("X-Second").Single());
		Assert.IsFalse(handler.CancellationToken.IsCancellationRequested);
	}

	[TestMethod]
	public async Task WebResourceLoaderHandlesNullHeadersAndMissingContentType()
	{
		var handler = new CapturingHttpMessageHandler(includeContentType: false);
		var loader = new WebResourceLoader
					 {
						 DisposeToken = default,
						 HttpClientFactory = () => new HttpClient(handler, disposeHandler: false),
						 ResourceFactory = (stream, contentType) => new Resource(stream, contentType)
					 };

		await using var resource = await loader.Request(new Uri("http://example.test/resource"), headers: null);

		Assert.IsNull(resource.ContentType);
		Assert.AreEqual("web payload", await resource.GetContent());
	}

	[TestMethod]
	public async Task WebResourceLoaderProviderMatchesOnlyAbsoluteHttpUris()
	{
		var loader = new WebResourceLoader
					 {
						 DisposeToken = default,
						 HttpClientFactory = [ExcludeFromCodeCoverage] () => new HttpClient(),
						 ResourceFactory = [ExcludeFromCodeCoverage] (stream, contentType) => new Resource(stream, contentType)
					 };
		IResourceLoaderProvider provider = new WebResourceLoader.Provider { ResourceLoaderFactory = () => new ValueTask<WebResourceLoader>(loader) };

		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("http://example.test/resource")));
		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("https://example.test/resource")));
		Assert.IsNull(await provider.TryGetResourceLoader(new Uri("ftp://example.test/resource")));
		Assert.IsNull(await provider.TryGetResourceLoader(new Uri("relative", UriKind.Relative)));
	}

	[TestMethod]
	public async Task ResourceLoaderServiceResolvesRelativeUriAndUsesFirstMatchingProvider()
	{
		var expectedUri = new Uri("https://example.test/machines/child.scxml");
		var expectedResource = new Resource(new MemoryStream([1, 2, 3]), contentType: null);
		var loader = new Mock<IResourceLoader>();
		loader.Setup(l => l.Request(expectedUri, It.IsAny<NameValueCollection?>())).ReturnsAsync(expectedResource);
		var skippedProvider = new Mock<IResourceLoaderProvider>();
		skippedProvider.Setup(p => p.TryGetResourceLoader(expectedUri)).ReturnsAsync((IResourceLoader?) null);
		var matchingProvider = new Mock<IResourceLoaderProvider>();
		matchingProvider.Setup(p => p.TryGetResourceLoader(expectedUri)).ReturnsAsync(loader.Object);
		var service = new ResourceLoaderService
					  {
						  StateMachineLocation = Mock.Of<IStateMachineLocation>(l => l.Location == new Uri("https://example.test/machines/root.scxml")),
						  ResourceLoaderProviders = ToAsyncEnumerable(skippedProvider.Object, matchingProvider.Object)
					  };
		var headers = new NameValueCollection { ["X-Test"] = "value" };

		await using var resource = await service.Request(new Uri("child.scxml", UriKind.Relative), headers);

		Assert.AreSame(expectedResource, resource);
		loader.Verify(l => l.Request(expectedUri, headers), Times.Once);
	}

	[TestMethod]
	public async Task ResxResourceLoaderReturnsResourceFromResolvedManifestStream()
	{
		var ioBoundTask = new Mock<IIoBoundTask>();
		ioBoundTask.SetupGet(static task => task.Factory).Returns(new TaskFactory());
		var loader = new TestResxResourceLoader
					 {
						 IoBoundTask = ioBoundTask.Object,
						 ResourceFactory = static (stream, contentType) => new Resource(stream, contentType)
					 };

		await using var resource = await loader.Request(new Uri("res://assembly/resource.name"), headers: null);

		CollectionAssert.AreEqual(new byte[] { 4, 5, 6 }, await resource.GetBytes());
		Assert.IsNull(resource.ContentType);
		Assert.AreEqual(new Uri("res://assembly/resource.name"), loader.RequestedUri);
	}

	[TestMethod]
	public async Task ResxResourceLoaderProviderMatchesOnlyAbsoluteResSchemes()
	{
		var loader = new TestResxResourceLoader
					 {
						 IoBoundTask = Mock.Of<IIoBoundTask>(),
						 ResourceFactory = [ExcludeFromCodeCoverage] static (stream, contentType) => new Resource(stream, contentType)
					 };
		IResourceLoaderProvider provider = new ResxResourceLoader.Provider
										   {
											   ResourceLoaderFactory = () => new ValueTask<ResxResourceLoader>(loader)
										   };

		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("res://assembly/resource")));
		Assert.AreSame(loader, await provider.TryGetResourceLoader(new Uri("resx://assembly/resource")));
		Assert.IsNull(await provider.TryGetResourceLoader(new Uri("https://example.test/resource")));
		Assert.IsNull(await provider.TryGetResourceLoader(new Uri("relative", UriKind.Relative)));
	}

	[ExcludeFromCodeCoverage]
	private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] values)
	{
		await Task.Yield();

		foreach (var value in values)
		{
			yield return value;
		}
	}

	private static FileResourceLoader CreateFileLoader()
	{
		var ioBoundTask = new Mock<IIoBoundTask>();
		ioBoundTask.SetupGet(task => task.Factory).Returns(new TaskFactory());

		return new FileResourceLoader
			   {
				   ExternalResources = ioBoundTask.Object,
				   ResourceFactory = (stream, contentType) => new ValueTask<Resource>(new Resource(stream, contentType))
			   };
	}

	private sealed class TestFileResourceLoader : FileResourceLoader
	{
		[ExcludeFromCodeCoverage]
		public FileStream Open(string path) => CreateFileStream(path);
	}

	private sealed class TestResxResourceLoader : ResxResourceLoader
	{
		public Uri? RequestedUri { get; private set; }

		protected override Stream GetResourceStream(Uri uri)
		{
			RequestedUri = uri;
			return new MemoryStream([4, 5, 6]);
		}
	}

	private sealed class CapturingHttpMessageHandler(bool includeContentType = true) : HttpMessageHandler
	{
		public HttpRequestMessage? Request { get; private set; }

		public CancellationToken CancellationToken { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Request = request;
			CancellationToken = cancellationToken;

			var content = new ByteArrayContent(Encoding.UTF8.GetBytes("web payload"));
			if (includeContentType)
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
			}

			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
		}
	}
}
