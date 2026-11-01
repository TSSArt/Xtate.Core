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


using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.StateMachine;
using Xtate.IoC.Options;
using Xtate.IoProcessors.NamedPipe;
using Xtate.IoProcessors.NamedPipe.Services;
using Xtate.Persistence.Services;
using Xtate.Persistence.Extensions;

namespace Xtate.Core.Test.UnitTests.IoProcessors;

[TestClass]
public class NamedPipeFramingCoverageTest
{
	[TestMethod]
	public async Task OversizedOutgoingMessageIsRejectedBeforeWritingToThePipe()
	{
		var options = new NamedPipeIoProcessorOptions { Name = "test", MaxMessageSize = 1 };
		var controller = new NamedPipeController(Mock.Of<IOptions<NamedPipeIoProcessorOptions>>(value => value.Value == options));
		using var pipe = new ChunkedPipeStream([], 1);
		var message = new NamedPipeEventMessage(DateTime.UtcNow, SessionId.FromString("target"), new IncomingEvent { Data = "payload" });
		var method = typeof(NamedPipeController).GetMethod("SendMessage", BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(typeof(NamedPipeEventMessage));
		await Assert.ThrowsExactlyAsync<ProcessorException>(async () => await (ValueTask)method.Invoke(controller, [message, pipe, CancellationToken.None])!);
	}
	[TestMethod]
	public async Task FramedStoredMessageIsRestoredFromPayload()
	{
		using var storage = new InMemoryStorage(false);
		new Bucket(storage).Add("value", "restored");
		var size = storage.GetTransactionLogSize();
		var frame = new byte[sizeof(long) + size];
		Array.Copy(BitConverter.GetBytes((long)size), frame, sizeof(long));
		storage.WriteTransactionLogToSpan(frame.AsSpan(sizeof(long)), truncateLog: false);
		using var pipe = new ChunkedPipeStream(frame, int.MaxValue);
		Assert.AreEqual("restored", await Receive(pipe, size));
	}

	[TestMethod]
	[DataRow(-1L)]
	[DataRow(10L)]
	[DataRow(1L)]
	public async Task InvalidOrTruncatedMessagePayloadIsRejected(long size)
	{
		using var pipe = new ChunkedPipeStream(BitConverter.GetBytes(size), int.MaxValue);
		await Assert.ThrowsExactlyAsync<ProcessorException>(async () => await Receive(pipe, 2));
	}

	private static async Task<string> Receive(PipeStream pipe, long maximumSize)
	{
		var options = new NamedPipeIoProcessorOptions { Name = "test", MaxMessageSize = maximumSize };
		var controller = new NamedPipeController(Mock.Of<IOptions<NamedPipeIoProcessorOptions>>(value => value.Value == options));
		var method = typeof(NamedPipeController).GetMethod("ReceiveMessage", BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(typeof(string));
		static string Factory(Bucket bucket) => bucket.GetString("value")!;

		return await (ValueTask<string>)method.Invoke(controller, [pipe, (Func<Bucket, string>)Factory, CancellationToken.None])!;
	}

	[TestMethod]
	public async Task CompleteFrameIsReadIntoTheProvidedBuffer()
	{
		using var stream = new ChunkedPipeStream([1, 2, 3, 4], 4);
		var buffer = new byte[4];
		Assert.AreEqual(4L, await Read(stream, buffer, 4));
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, buffer);
	}

	[TestMethod]
	public async Task EndOfStreamReturnsActualByteCountForTruncatedLargeFrame()
	{
		using var stream = new ChunkedPipeStream([1, 2, 3], 2);
		Assert.AreEqual(3L, await Read(stream, new byte[1], int.MaxValue));
	}

	[TestMethod]
	public async Task EndOfStreamBeforeFrameReturnsZero()
	{
		using var stream = new ChunkedPipeStream([], 4);
		Assert.AreEqual(0L, await Read(stream, new byte[4], 4));
	}

	[TestMethod]
	public async Task FragmentedFrameIsReadUntilRequestedLength()
	{
		using var stream = new ChunkedPipeStream([1, 2, 3, 4], 2);
		var buffer = new byte[4];
		Assert.AreEqual(4L, await Read(stream, buffer, 4));
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, buffer);
	}

	private static async Task<long> Read(PipeStream stream, byte[] buffer, long length) =>
		await (ValueTask<long>)typeof(NamedPipeController).GetMethod("ReadAtLeast", BindingFlags.Static | BindingFlags.NonPublic)!
			.Invoke(null, [stream, buffer, length, CancellationToken.None])!;

	private sealed class ChunkedPipeStream(byte[] data, int chunkSize) : PipeStream(PipeDirection.In, 0)
	{
		private int _position;

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var length = Math.Min(Math.Min(chunkSize, count), data.Length - _position);
			Array.Copy(data, _position, buffer, offset, length);
			_position += length;
			return Task.FromResult(length);
		}
	}
}
