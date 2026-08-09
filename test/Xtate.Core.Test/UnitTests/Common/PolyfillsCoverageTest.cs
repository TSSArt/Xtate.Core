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

// ReSharper disable MethodHasAsyncOverload

using System.IO;
using System.Text;
using System.Threading;
#if NET462
using System.Net.Http;
using System.Xml;
#endif

namespace Xtate.Core.Test.UnitTests.Common;

[TestClass]
public class PolyfillsCoverageTest
{
	[TestMethod]
	public void BitConverterTryWriteBytesWritesPrimitiveValuesAndRejectsSmallDestinations()
	{
		Assert.IsFalse(BitConverter.TryWriteBytes([], value: true));

		AssertWrites(value: true, sizeof(byte), BitConverter.GetBytes(true));
		AssertWrites(value: 'Ж', sizeof(char), BitConverter.GetBytes('Ж'));
		AssertWrites((short)-1234, sizeof(short), BitConverter.GetBytes((short)-1234));
		AssertWrites(value: 123456789, sizeof(int), BitConverter.GetBytes(123456789));
		AssertWrites(value: -1234567890123456789L, sizeof(long), BitConverter.GetBytes(-1234567890123456789L));
		AssertWrites((ushort)65000, sizeof(ushort), BitConverter.GetBytes((ushort)65000));
		AssertWrites(value: 4000000000U, sizeof(uint), BitConverter.GetBytes(4000000000U));
		AssertWrites(value: 18000000000000000000UL, sizeof(ulong), BitConverter.GetBytes(18000000000000000000UL));
		AssertWrites(value: 123.5F, sizeof(float), BitConverter.GetBytes(123.5F));
		AssertWrites(value: -9876.25D, sizeof(double), BitConverter.GetBytes(-9876.25D));
	}

	[TestMethod]
	public async Task StreamReaderReadToEndAsyncReadsContentAndHonorsCancellation()
	{
		using var stream = new MemoryStream([.. "first line\r\nsecond line"u8]);
		using var reader = new StreamReader(stream, Encoding.UTF8);

		Assert.AreEqual(expected: "first line\r\nsecond line", await reader.ReadToEndAsync(CancellationToken.None));

		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		using var canceledStream = new MemoryStream([.. "value"u8]);
		using var canceledReader = new StreamReader(canceledStream, Encoding.UTF8);

		try
		{
			await canceledReader.ReadToEndAsync(cancellationTokenSource.Token);
			Assert.Fail("Expected the canceled token to stop ReadToEndAsync.");
		}
		catch (OperationCanceledException)
		{
			// The net462 polyfill throws OperationCanceledException directly, while newer BCLs can throw TaskCanceledException.
		}
	}

	[TestMethod]
	public void ConcurrentDictionaryPolyfillsRemovePairsAndUseArgumentFactories()
	{
		var dictionary = new ConcurrentDictionary<string, int>();

		Assert.AreEqual(expected: 5, dictionary.GetOrAdd(key: "value", static (_, arg) => arg, factoryArgument: 5));
		Assert.AreEqual(expected: 5, dictionary.GetOrAdd(key: "value", static (_, arg) => arg, factoryArgument: 10));
		Assert.AreEqual(expected: 8, dictionary.AddOrUpdate(key: "value", static (_, arg) => arg, static (_, current, arg) => current + arg, factoryArgument: 3));
		Assert.AreEqual(expected: 4, dictionary.AddOrUpdate(key: "other", static (_, arg) => arg, static (_, current, arg) => current + arg, factoryArgument: 4));
		Assert.IsFalse(dictionary.TryRemove(new KeyValuePair<string, int>(key: "value", value: 5)));
		Assert.IsTrue(dictionary.TryRemove(new KeyValuePair<string, int>(key: "value", value: 8)));
		Assert.IsFalse(dictionary.ContainsKey("value"));
	}

#if NET462
	[TestMethod]
	public async Task Net462CompatibilityApisRemainExternallyUsable()
	{
		using var registration = CancellationToken.None.Register(static () => { });
		await registration.DisposeAsync();

		var hash3 = HashCode.Combine(value1: 1, value2: 2, value3: 3);
		var hash4 = HashCode.Combine(value1: 1, value2: 2, value3: 3, value4: 4);
		Assert.AreNotEqual(hash3, hash4);

		var completionWithState = new TaskCompletionSource(state: "state");
		var completionWithStateAndOptions = new TaskCompletionSource(state: "state", TaskCreationOptions.RunContinuationsAsynchronously);
		Assert.IsTrue(completionWithState.TrySetResult());
		completionWithStateAndOptions.SetResult();
		await Task.WhenAll(completionWithState.Task, completionWithStateAndOptions.Task);

		Assert.AreEqual(expected: 42, await ValueTask.FromResult(42));

		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.IsTrue(ValueTask.FromCanceled(cancellation.Token).AsTask().IsCanceled);
		Assert.IsTrue(ValueTask.FromCanceled<int>(cancellation.Token).AsTask().IsCanceled);

		using var writer = XmlWriter.Create(new StringWriter());
		await writer.DisposeAsync();

		using var content = new StringContent("value");

		// ReSharper disable AccessToDisposedClosure
		Assert.ThrowsExactly<OperationCanceledException>(() => content.ReadAsStreamAsync(cancellation.Token));

		// ReSharper restore AccessToDisposedClosure
	}
#endif

	private static void AssertWrites(bool value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
	}

	private static void AssertWrites(char value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(short value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(int value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(long value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(ushort value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(uint value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(ulong value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(float value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}

	private static void AssertWrites(double value, int size, byte[] expected)
	{
		var buffer = new byte[size];

		Assert.IsTrue(BitConverter.TryWriteBytes(buffer, value));
		CollectionAssert.AreEqual(expected, buffer);
		Assert.IsFalse(BitConverter.TryWriteBytes(buffer.AsSpan(start: 0, size - 1), value));
	}
}
