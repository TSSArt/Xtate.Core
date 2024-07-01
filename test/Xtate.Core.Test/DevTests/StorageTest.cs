#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System.Text;
using Xtate.Persistence;
using TypeInfo = Xtate.Persistence.TypeInfo;

namespace Xtate.Test
{
	[TestClass]
	public class StorageTest
	{
		public static string Dump(IStorage storage, string delimiter = "", bool hex = false)
		{
			var inMemoryStorage = (InMemoryStorage) storage;

			var logBytes = new byte[inMemoryStorage.GetTransactionLogSize()];
			inMemoryStorage.WriteTransactionLogToSpan(logBytes, truncateLog: false);

			var log = logBytes.AsSpan();
			var sb = new StringBuilder();

			while (!log.IsEmpty)
			{
				var keyLengthLength = GetLength(log[0]);
				var keyLength = DecodeLength(log[..keyLengthLength]);
				var key = log.Slice(keyLengthLength, keyLength);

				var valueLengthLength = GetLength(log[keyLengthLength + keyLength]);
				var valueLength = DecodeLength(log.Slice(keyLengthLength + keyLength, valueLengthLength));
				var value = log.Slice(keyLengthLength + keyLength + valueLengthLength, valueLength);

				AppendKey(sb, key.ToArray());
				sb.Append('=');
				AppendValue(sb, value.ToArray(), hex);
				sb.Append(delimiter);

				var rowSize = keyLengthLength + keyLength + valueLengthLength + valueLength;
				log = log[rowSize..];
			}

			return sb.ToString();
		}

		public static int GetEntriesCount(IStorage storage)
		{
			var inMemoryStorage = (InMemoryStorage) storage;

			var logBytes = new byte[inMemoryStorage.GetTransactionLogSize()];
			inMemoryStorage.WriteTransactionLogToSpan(logBytes, truncateLog: false);

			var log = logBytes.AsSpan();
			var count = 0;
			while (!log.IsEmpty)
			{
				var keyLengthLength = GetLength(log[0]);
				var keyLength = DecodeLength(log[..keyLengthLength]);

				var valueLengthLength = GetLength(log[keyLengthLength + keyLength]);
				var valueLength = DecodeLength(log.Slice(keyLengthLength + keyLength, valueLengthLength));

				var rowSize = keyLengthLength + keyLength + valueLengthLength + valueLength;
				log = log[rowSize..];
				count ++;
			}

			return count;
		}

		private static int GetLength(byte value)
		{
			if ((value & 0x80) == 0x00) return 1;
			if ((value & 0xE0) == 0xC0) return 2;
			if ((value & 0xF0) == 0xE0) return 3;
			if ((value & 0xF8) == 0xF0) return 4;
			if ((value & 0xFC) == 0xF8) return 5;
			if ((value & 0xFE) == 0xFC) return 6;

			throw new ArgumentException("Incorrect key encoding");
		}

		private static int DecodeLength(ReadOnlySpan<byte> key)
		{
			switch (key.Length)
			{
				case 1: return key[0] & 0x7F;

				case 2 when (key[0] & 0xE0) == 0xC0 && (key[1] & 0xC0) == 0x80:
					return ((key[0] & 0x1F) << 6) + (key[1] & 0x3F);

				case 3 when (key[0] & 0xF0) == 0xE0 && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80:
					return ((key[0] & 0x0F) << 12) + ((key[1] & 0x3F) << 6) + (key[2] & 0x3F);

				case 4 when (key[0] & 0xF8) == 0xF0 && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80:
					return ((key[0] & 0x07) << 18) + ((key[1] & 0x3F) << 12) + ((key[2] & 0x3F) << 6) + (key[3] & 0x3F);

				case 5 when (key[0] & 0xFC) == 0xF8 && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80 && (key[4] & 0xC0) == 0x80:
					return ((key[0] & 0x03) << 24) + ((key[1] & 0x3F) << 18) + ((key[2] & 0x3F) << 12) + ((key[3] & 0x3F) << 6) + (key[4] & 0x3F);

				case 6 when (key[0] & 0xFE) == 0xFC && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80 && (key[4] & 0xC0) == 0x80 && (key[5] & 0xC0) == 0x80:
					return ((key[0] & 0x01) << 30) + ((key[1] & 0x3F) << 24) + ((key[2] & 0x3F) << 18) + ((key[3] & 0x3F) << 12) + ((key[4] & 0x3F) << 6) + (key[5] & 0x3F);
			}

			throw new ArgumentException("Incorrect key encoding");
		}

		private static void AppendKey(StringBuilder sb, byte[] key)
		{
			for (var i = 0; i < key.Length;)
			{
				var len = GetLength(key, i);

				if (len == 0)
				{
					break;
				}

				Append(sb, new Span<byte>(key, i, len));

				i += len;
			}
		}

		private static int GetBytesLength(byte[] key, int start)
		{
			for (var i = start; i < key.Length; i ++)
			{
				if (key[i] == 0xFF)
				{
					return i - start + 1;
				}
			}

			throw new ArgumentException("0xFF byte not detected");
		}

		private static ulong DecodeUInt64(Span<byte> key)
		{
			switch (key.Length)
			{
				case 1: return key[0] & 0x7FUL;

				case 2 when (key[0] & 0xE0) == 0xC0 && (key[1] & 0xC0) == 0x80:
					return ((key[0] & 0x1FUL) << 6) + (key[1] & 0x3FUL);

				case 3 when (key[0] & 0xF0) == 0xE0 && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80:
					return ((key[0] & 0x0FUL) << 12) + ((key[1] & 0x3FUL) << 6) + (key[2] & 0x3FUL);

				case 4 when (key[0] & 0xF8) == 0xF0 && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80:
					return ((key[0] & 0x07UL) << 18) + ((key[1] & 0x3FUL) << 12) + ((key[2] & 0x3FUL) << 6) + (key[3] & 0x3FUL);

				case 5 when (key[0] & 0xFC) == 0xF8 && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80 && (key[4] & 0xC0) == 0x80:
					return ((key[0] & 0x03UL) << 24) + ((key[1] & 0x3FUL) << 18) + ((key[2] & 0x3FUL) << 12) + ((key[3] & 0x3FUL) << 6) + (key[4] & 0x3FUL);

				case 6 when (key[0] & 0xFE) == 0xFC && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80 && (key[4] & 0xC0) == 0x80 && (key[5] & 0xC0) == 0x80:
					return ((key[0] & 0x01UL) << 30) + ((key[1] & 0x3FUL) << 24) + ((key[2] & 0x3FUL) << 18) + ((key[3] & 0x3FUL) << 12) + ((key[4] & 0x3FUL) << 6) + (key[5] & 0x3FUL);

				case 7 when (key[0] & 0xFF) == 0xFE && (key[1] & 0xC0) == 0x80 && (key[2] & 0xC0) == 0x80 && (key[3] & 0xC0) == 0x80 && (key[4] & 0xC0) == 0x80 && (key[5] & 0xC0) == 0x80 &&
							(key[6] & 0xC0) == 0x80:
					return ((key[0] & 0x00UL) << 36) + ((key[1] & 0x3FUL) << 30) + ((key[2] & 0x3FUL) << 24) + ((key[3] & 0x3FUL) << 18) + ((key[4] & 0x3FUL) << 12) + ((key[5] & 0x3FUL) << 6) +
						   (key[6] & 0x3FUL);
			}

			throw new ArgumentException("Incorrect key encoding");
		}

		private static string DecodeUtf8String(Span<byte> key) => Encoding.UTF8.GetString(key.ToArray());

		private static int GetLength(byte[] key, int start)
		{
			var value = key[start];

			if (value == 0) return 0;

			if (value == 7) return GetBytesLength(key, start + 1) + 1;

			if ((value & 0x80) == 0x00) return 1;
			if ((value & 0xE0) == 0xC0) return 2;
			if ((value & 0xF0) == 0xE0) return 3;
			if ((value & 0xF8) == 0xF0) return 4;
			if ((value & 0xFC) == 0xF8) return 5;
			if ((value & 0xFE) == 0xFC) return 6;
			if ((value & 0xFF) == 0xFE) return 7;

			throw new ArgumentException("Incorrect key encoding");
		}

		private static void Append(StringBuilder sb, Span<byte> bytes)
		{
			if (bytes[0] == 7)
			{
				sb.Append("/'").Append(DecodeUtf8String(bytes[1..^1])).Append('\'');

				return;
			}

			var key = DecodeUInt64(bytes);

			switch (key & 3)
			{
				case 1:
					sb.Append('/').Append(Enum.ToObject(typeof(Key), unchecked((int) (key >> 2))));
					break;

				case 2:
					var index = unchecked((int) (key >> 2));
					sb.Append("/[").Append(index).Append(']');
					break;

				default:
					throw new ArgumentException("Incorrect key encoding");
			}
		}

		private static void AppendValue(StringBuilder sb, byte[] value, bool hex)
		{
			if (hex)
			{
				foreach (var b in value)
				{
					if (b is < 32 or > 127)
					{
						sb.Append('%').Append(((int) b).ToString("X2"));
					}
					else if (b == '%')
					{
						sb.Append("%%");
					}
					else
					{
						sb.Append((char) b);
					}
				}
			}
			else
			{
				foreach (var b in value)
				{
					sb.Append((char) b);
				}
			}
		}

		[TestMethod]
		public void KeyIdStoreTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add(Key.Id, TypeInfo.AssignNode);
			Assert.AreEqual(expected: "/Id=\0", Dump(storage));
		}

		[TestMethod]
		public void IndexEnumStoreTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add(key: 5, TypeInfo.AssignNode);
			Assert.AreEqual(expected: "/[5]=\0", Dump(storage));
		}

		[TestMethod]
		public void IndexBoolStoreTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add(key: 5, value: true);
			Assert.AreEqual(expected: "/[5]=\x01", Dump(storage));
		}

		[TestMethod]
		public void Index5StoreTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add(key: 5, value: "2");
			Assert.AreEqual(expected: "/[5]=2", Dump(storage));
		}

		private static string ConvertInt(int value)
		{
			if (value == 0)
			{
				return "\0";
			}

			var uval = unchecked((uint) value);

			var str = ((char) (uval & 0xFF)).ToString();
			if (uval > 0xFFU) str += ((char) ((uval >> 8) & 0xFF)).ToString();
			if (uval > 0xFFFFU) str += ((char) ((uval >> 16) & 0xFF)).ToString();
			if (uval > 0xFFFFFFU) str += ((char) ((uval >> 24) & 0xFF)).ToString();

			return str;
		}

		[TestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(0x1A)]
		[DataRow(0x1A5)]
		[DataRow(0x2A5A)]
		[DataRow(0x5A5A5)]
		[DataRow(0xA5A5A5)]
		[DataRow(0x1A5A5A5A)]
		[DataRow(int.MaxValue)]
		[DataRow(int.MinValue)]
		public void IndexEncodingStoreTest(int index)
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add(index, index);
			Assert.AreEqual($"/[{index}]={ConvertInt(index)}", Dump(storage));
		}

		[TestMethod]
		public void SubBucketIndexStoreTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage).Nested(11);
			bucket.Add(key: 5, new Uri("xtate://localhost"));
			Assert.AreEqual(expected: "/[11]/[5]=xtate://localhost/", Dump(storage));
		}

		[TestMethod]
		public void SubBucketKeyStoreTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage).Nested(Key.Uri);
			bucket.Add(key: 5, value: 2);
			Assert.AreEqual(expected: "/Uri/[5]=\x02", Dump(storage));
		}

		[TestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(0x1A)]
		[DataRow(0x1A5)]
		[DataRow(0x2A5A)]
		[DataRow(0x5A5A5)]
		[DataRow(0xA5A5A5)]
		[DataRow(0x1A5A5A5A)]
		[DataRow(int.MaxValue)]
		[DataRow(int.MinValue)]
		public void TwoIndexStoreTest(int index)
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage).Nested(index);
			bucket.Add(index, index);
			Assert.AreEqual($"/[{index}]/[{index}]={ConvertInt(index)}", Dump(storage));
		}

		[TestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(0x1A)]
		[DataRow(0x1A5)]
		[DataRow(0x2A5A)]
		[DataRow(0x5A5A5)]
		[DataRow(0xA5A5A5)]
		[DataRow(0x1A5A5A5A)]
		[DataRow(int.MaxValue)]
		[DataRow(int.MinValue)]
		public void ThreeIndexStoreTest(int index)
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage).Nested(index);
			bucket = bucket.Nested(index);
			bucket.Add(index, index);
			Assert.AreEqual($"/[{index}]/[{index}]/[{index}]={ConvertInt(index)}", Dump(storage));
		}

		[TestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(0x1A)]
		[DataRow(0x1A5)]
		[DataRow(0x2A5A)]
		[DataRow(0x5A5A5)]
		[DataRow(0xA5A5A5)]
		[DataRow(0x1A5A5A5A)]
		[DataRow(int.MaxValue)]
		[DataRow(int.MinValue)]
		public void FourIndexStoreTest(int index)
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage).Nested(index);
			bucket = bucket.Nested(index);
			bucket = bucket.Nested(index);
			bucket.Add(index, index);
			Assert.AreEqual($"/[{index}]/[{index}]/[{index}]/[{index}]={ConvertInt(index)}", Dump(storage));
		}

		[TestMethod]
		public void Issue1ValidationTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket = bucket.Nested(Key.States);
			bucket = bucket.Nested(0);
			bucket = bucket.Nested(Key.DataModel);
			bucket = bucket.Nested(Key.DataList);
			bucket = bucket.Nested(0);
			bucket = bucket.Nested(Key.Source);
			bucket.Add(Key.TypeInfo, TypeInfo.AssignNode);
			Assert.AreEqual(expected: "/States/[0]/DataModel/DataList/[0]/Source/TypeInfo=\0", Dump(storage));
		}

		[TestMethod]
		public void StringKeyTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket = bucket.Nested("A");
			bucket.Add(key: 1, value: 1);
			Assert.AreEqual(expected: "/'A'/[1]=\x01", Dump(storage));
		}

		[TestMethod]
		public void StringWithZeroKeyTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket = bucket.Nested("A\0B");
			bucket.Add(key: 1, value: 1);
			Assert.AreEqual(expected: "/'A\0B'/[1]=\x01", Dump(storage));
		}

		[TestMethod]
		public void GuidStringKeyTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket = bucket.Nested("{F5F7A5BE}");
			bucket.Add(key: 1, value: "{6213C41D}");
			Assert.AreEqual(expected: "/'{F5F7A5BE}'/[1]={6213C41D}", Dump(storage));
		}

		[TestMethod]
		public void GuidStringDoubleKeyTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket = bucket.Nested("1");
			bucket.Add(key: "2", value: "3");
			Assert.AreEqual(expected: "/'1'/'2'=3", Dump(storage));
		}

		[TestMethod]
		[DataRow("A")]
		[DataRow("\r")]
		[DataRow("{9B782080-3152-4677-9E33-4E8434F39BB2}")]
		[DataRow("{9B782080-3152-4677-9E33-4E8434F39BB2}{9B782080-3152-4677-9E33-4E8434F39BB2}")]
		public void GuidStringTripleKeyTest(string key)
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket = bucket.Nested(key);
			bucket = bucket.Nested(key);
			bucket.Add(key, key);
			Assert.AreEqual($"/'{key}'/'{key}'/'{key}'={key}", Dump(storage));
		}

		[TestMethod]
		public void UnicodeReadWriteTest()
		{
			var storage = new InMemoryStorage(false);
			var bucket = new Bucket(storage);

			const string s = "aeiouçéüß";
			bucket.Add(s, s);
			var flag = bucket.TryGet(s, out string? value);
			Assert.IsTrue(flag);
			Assert.AreEqual(s, value);
		}

		[TestMethod]
		public void GuidEmptyStringKeyTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add(key: "", value: "a");
			Assert.AreEqual(expected: "/''=a", Dump(storage));
		}

		[TestMethod]
		public void GuidEmptyStringValueTest()
		{
			var storage = new InMemoryStorage(false);
			var bucket = new Bucket(storage);
			bucket.Add(key: "a", value: "");
			var flag = bucket.TryGet(key: "a", out string? value);

			Assert.IsTrue(flag);
			Assert.AreEqual(expected: "", value);
			Assert.AreEqual(expected: "/'a'=\xFF", Dump(storage));
		}

		[TestMethod]
		public void GuidNullStringValueTest()
		{
			var storage = new InMemoryStorage(false);
			var bucket = new Bucket(storage);
			bucket.Add(key: "a", (string) null!);
			var flag = bucket.TryGet(key: "a", out string? value);

			Assert.IsFalse(flag);
			Assert.IsNull(value);
			Assert.AreEqual(expected: "/'a'=", Dump(storage));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GuidNullStringKeyTest()
		{
			var storage = new InMemoryStorage();
			var bucket = new Bucket(storage);
			bucket.Add((string) null!, value: "d");
		}

		[TestMethod]
		public void RemoveKeyTest()
		{
			var storage = new InMemoryStorage(false);
			var bucket = new Bucket(storage);
			bucket.Add(key: "f", value: "f");
			Assert.IsTrue(bucket.TryGet(key: "f", out _));
			bucket.Remove("f");
			Assert.IsFalse(bucket.TryGet(key: "f", out _));
		}

		[TestMethod]
		public void RemoveAllTest()
		{
			var storage = new InMemoryStorage(false);
			var bucket = new Bucket(storage);
			bucket.Add(key: "f", value: "f");

			storage.RemoveAll([]);

			Assert.IsFalse(bucket.TryGet(key: "f", out _));
		}

		[TestMethod]
		public void RemoveSubtreeTest()
		{
			var storage = new InMemoryStorage(false);

			storage.Set("\t"u8, "7"u8);
			storage.Set("\n"u8, "7"u8);
			storage.Set("\n\r"u8, "7"u8);
			storage.Set("A"u8, "7"u8);

			storage.RemoveAll("\n"u8);

			Assert.IsFalse(storage.Get("\t"u8).IsEmpty);
			Assert.IsTrue(storage.Get("\n"u8).IsEmpty);
			Assert.IsTrue(storage.Get("\n\r"u8).IsEmpty);
			Assert.IsFalse(storage.Get("A"u8).IsEmpty);
		}

		[TestMethod]
		public void RemoveSubtreeIncludeUpperBoundaryTest()
		{
			var storage = new InMemoryStorage(false);

			storage.Set("\t"u8, "7"u8);
			storage.Set("\n"u8, "7"u8);
			storage.Set("\n\r"u8, "7"u8);
			storage.Set("\r"u8, "7"u8);
			storage.Set("A"u8, "7"u8);

			storage.RemoveAll("\n"u8);

			Assert.IsFalse(storage.Get("\t"u8).IsEmpty);
			Assert.IsTrue(storage.Get("\n"u8).IsEmpty);
			Assert.IsTrue(storage.Get("\n\r"u8).IsEmpty);
			Assert.IsFalse(storage.Get("\r"u8).IsEmpty);
			Assert.IsFalse(storage.Get("A"u8).IsEmpty);
		}

		[TestMethod]
		public void RemoveSubtreeSub255Test()
		{
			var storage = new InMemoryStorage(false);

			storage.Set("\t"u8, "7"u8);
			storage.Set(new byte[] { 10, 255 }, "7"u8);
			storage.Set(new byte[] { 10, 255, 13 }, "7"u8);
			storage.Set("\r"u8, "7"u8);
			storage.Set("A"u8, "7"u8);

			storage.RemoveAll(new byte[] { 10, 255 });

			Assert.IsFalse(storage.Get("\t"u8).IsEmpty);
			Assert.IsTrue(storage.Get(new byte[] { 10, 255 }).IsEmpty);
			Assert.IsTrue(storage.Get(new byte[] { 10, 255, 13 }).IsEmpty);
			Assert.IsFalse(storage.Get("\r"u8).IsEmpty);
			Assert.IsFalse(storage.Get("A"u8).IsEmpty);
		}

		[TestMethod]
		public void RemoveSubtree255Test()
		{
			var storage = new InMemoryStorage(false);

			storage.Set("\t"u8, "7"u8);
			storage.Set(new byte[] { 255 }, "7"u8);
			storage.Set(new byte[] { 255, 13 }, "7"u8);

			storage.RemoveAll(new byte[] { 255 });

			Assert.IsFalse(storage.Get("\t"u8).IsEmpty);
			Assert.IsTrue(storage.Get(new byte[] { 255 }).IsEmpty);
			Assert.IsTrue(storage.Get(new byte[] { 255, 13 }).IsEmpty);
		}

		[TestMethod]
		public void RemoveSubtreeBucketTest()
		{
			var storage = new InMemoryStorage(false);
			var bucket = new Bucket(storage);
			bucket.Add(key: "a", value: "f");
			bucket.Nested("f").Add(key: "a", value: "f");
			bucket.Nested("f").Add(key: "b", value: "f");
			bucket.Add(key: "z", value: "f");

			Assert.IsTrue(bucket.Nested("f").TryGet(key: "a", out _));
			Assert.IsTrue(bucket.Nested("f").TryGet(key: "b", out _));

			bucket.RemoveSubtree("f");

			Assert.IsTrue(bucket.TryGet(key: "a", out _));
			Assert.IsFalse(bucket.Nested("f").TryGet(key: "a", out _));
			Assert.IsFalse(bucket.Nested("f").TryGet(key: "b", out _));
			Assert.IsTrue(bucket.TryGet(key: "z", out _));
		}
	}
}