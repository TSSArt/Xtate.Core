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
using System.Threading.Tasks;
using Xtate.ResourceLoaders;
using Xtate.ResourceLoaders.Extensions;
using Xtate.ResourceLoaders.Internal;

namespace Xtate.Test.UnitTests.ResourceLoaders;

[TestClass]
public class ResourceCoverageTest
{
	[TestMethod]
	public async Task ResourceReadsContentWithDeclaredEncodingAndCachesBytes()
	{
		var text = "zażółć";
		var contentType = new ContentType("text/plain") { CharSet = "utf-16" };
		await using var resource = new Resource(new MemoryStream(Encoding.Unicode.GetBytes(text)), contentType);

		Assert.AreSame(contentType, resource.ContentType);
		Assert.AreEqual(Encoding.Unicode, resource.Encoding);
		Assert.AreEqual(text, await resource.GetContent());
		CollectionAssert.AreEqual(Encoding.Unicode.GetBytes(text), await resource.GetBytes());

		using var cachedStream = await resource.GetStream(doNotCache: false);
		using var reader = new StreamReader(cachedStream, Encoding.Unicode);

		Assert.AreEqual(text, await reader.ReadToEndAsync());
	}

	[TestMethod]
	public async Task ResourceReadsBytesFirstThenBuildsContentAndStreamsFromCache()
	{
		var bytes = Encoding.UTF8.GetBytes("cached text");
		await using var resource = new Resource(new MemoryStream(bytes), contentType: null);

		CollectionAssert.AreEqual(bytes, await resource.GetBytes());
		Assert.AreEqual(Encoding.UTF8, resource.Encoding);
		Assert.AreEqual("cached text", await resource.GetContent());

		using var stream = await resource.GetStream(doNotCache: true);

		Assert.AreNotSame(Stream.Null, stream);
		CollectionAssert.AreEqual(bytes, await stream.ReadToEndAsync(CancellationToken.None));
	}

	[TestMethod]
	public async Task ResourceCanReturnOriginalStreamWhenCachingIsDisabled()
	{
		var source = new MemoryStream([1, 2, 3]);
		await using var resource = new Resource(source, contentType: null);

		var stream = await resource.GetStream(doNotCache: true);

		Assert.AreSame(source, stream);
		Assert.AreEqual(1, stream.ReadByte());
	}

	[TestMethod]
	public async Task ResourceDisposePreventsFurtherReads()
	{
		var resource = new Resource(new MemoryStream([1]), contentType: null);

		resource.Dispose();

		await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () => await resource.GetBytes());
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () => await resource.GetContent());
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () => await resource.GetStream(doNotCache: false));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () => _ = new Resource(null!, contentType: null));
	}

	[TestMethod]
	public async Task StreamExtensionsReadToEndHonorsCancellationAndUnknownLengthStreams()
	{
		using var nonSeekable = new NonSeekableStream([1, 2, 3, 4]);

		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, nonSeekable.ReadToEnd(CancellationToken.None));

		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		Assert.ThrowsExactly<OperationCanceledException>([ExcludeFromCodeCoverage] () => new NonSeekableStream([1]).ReadToEnd(cancellationTokenSource.Token));
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await new TokenObservingStream([1]).ReadToEndAsync(cancellationTokenSource.Token));
	}

	[TestMethod]
	public async Task InjectedCancellationStreamUsesExternalOrLinkedCancellationToken()
	{
		using var externalCancellation = new CancellationTokenSource();
		externalCancellation.Cancel();

		var externallyCanceledStream = new InjectedCancellationStream(new TokenObservingStream([1]), externalCancellation.Token);

		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await externallyCanceledStream.ReadAsync(new byte[1], 0, 1, CancellationToken.None));

		using var perCallCancellation = new CancellationTokenSource();
		perCallCancellation.Cancel();

		var linkedTokenInnerStream = new TokenObservingStream([1]);
		var linkedStream = new InjectedCancellationStream(linkedTokenInnerStream, CancellationToken.None);

		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await linkedStream.WriteAsync([1], 0, 1, perCallCancellation.Token));
		Assert.IsTrue(linkedTokenInnerStream.LastWriteToken.IsCancellationRequested);
	}

	private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override bool CanSeek => false;
	}

	private sealed class TokenObservingStream(byte[] bytes) : MemoryStream(bytes)
	{
		public CancellationToken LastWriteToken { get; private set; }

		[ExcludeFromCodeCoverage]
		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return cancellationToken.IsCancellationRequested ? Task.FromCanceled<int>(cancellationToken) : base.ReadAsync(buffer, offset, count, cancellationToken);
		}

		[ExcludeFromCodeCoverage]
		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			LastWriteToken = cancellationToken;

			return cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : base.WriteAsync(buffer, offset, count, cancellationToken);
		}
	}
}
