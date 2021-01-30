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

using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.DataModel.XPath;

namespace Xtate
{
	[Flags]
	[PublicAPI]
	public enum DataModelConverterJsonOptions
	{
		/// <summary>
		///     Serialization of undefined value will cause an exception. Array with undefined element will cause an exception.
		///     Properties with undefined values in object will be skipped.
		/// </summary>
		UndefinedNotAllowed = 0,

		/// <summary>
		///     Undefined value will serialize to empty string. Undefined elements in array will be skipped. Properties with
		///     undefined values in object will be skipped.
		/// </summary>
		UndefinedToSkip = 1,

		/// <summary>
		///     Undefined value will serialize to (null) value. Undefined elements in array will be serialized as (null).
		///     Properties with undefined values in object will be serialized as properties with (null).
		/// </summary>
		UndefinedToNull = 2,

		/// <summary>
		///     Undefined value will serialize to (null) value. Undefined elements in array will be serialized as (null).
		///     Properties with undefined values in object will be skipped.
		/// </summary>
		UndefinedToSkipOrNull = UndefinedToSkip | UndefinedToNull,

		WriteIndented = 4
	}

	[Flags]
	[PublicAPI]
	public enum DataModelConverterXmlOptions
	{
		WriteIndented = 1
	}

	[PublicAPI]
	public static class DataModelConverter
	{
		private const string TypeMetaKey     = @"type";
		private const string ObjectMetaValue = @"object";
		private const string ArrayMetaValue  = @"array";

		private static readonly JsonSerializerOptions DefaultOptions = CreateOptions(DataModelConverterJsonOptions.UndefinedNotAllowed);

		private static JsonSerializerOptions GetOptions(DataModelConverterJsonOptions options) =>
				options == DataModelConverterJsonOptions.UndefinedNotAllowed ? DefaultOptions : CreateOptions(options);

		private static JsonSerializerOptions CreateOptions(DataModelConverterJsonOptions options) =>
				new()
				{
						Converters =
						{
								new JsonValueConverter(options),
								new JsonListConverter(options)
						},
						WriteIndented = (options & DataModelConverterJsonOptions.WriteIndented) != 0,
						MaxDepth = 64
				};

		public static bool IsArray(DataModelList list)
		{
			if (list is null) throw new ArgumentNullException(nameof(list));

			if (list.GetMetadata() is { } metadata && metadata[TypeMetaKey, caseInsensitive: false] is { } val)
			{
				switch (val.AsStringOrDefault())
				{
					case ObjectMetaValue: return false;
					case ArrayMetaValue: return true;
				}
			}

			return list.Count > 0 && !list.HasKeys;
		}

		public static bool IsObject(DataModelList list)
		{
			if (list is null) throw new ArgumentNullException(nameof(list));

			if (list.GetMetadata() is { } metadata && metadata[TypeMetaKey, caseInsensitive: false] is { } val)
			{
				switch (val.AsStringOrDefault())
				{
					case ObjectMetaValue: return true;
					case ArrayMetaValue: return false;
				}
			}

			return list.Count > 0 && list.HasKeys;
		}

		public static DataModelList CreateAsObject()
		{
			var list = new DataModelList();

			list.SetMetadata(new DataModelList { { TypeMetaKey, ObjectMetaValue } });

			return list;
		}

		public static DataModelList CreateAsArray()
		{
			var list = new DataModelList();

			list.SetMetadata(new DataModelList { { TypeMetaKey, ArrayMetaValue } });

			return list;
		}

		public static string ToJson(DataModelValue value, DataModelConverterJsonOptions options = default) => JsonSerializer.Serialize(value, GetOptions(options));

		public static byte[] ToJsonUtf8Bytes(DataModelValue value, DataModelConverterJsonOptions options = default) => JsonSerializer.SerializeToUtf8Bytes(value, GetOptions(options));

		public static Task ToJsonAsync(Stream stream, DataModelValue value, DataModelConverterJsonOptions options = default, CancellationToken token = default) =>
				JsonSerializer.SerializeAsync(stream, value, GetOptions(options), token);

		public static DataModelValue FromJson(string json) => JsonSerializer.Deserialize<DataModelValue>(json, DefaultOptions)!;

		public static DataModelValue FromJson(ReadOnlySpan<byte> utf8Json) => JsonSerializer.Deserialize<DataModelValue>(utf8Json, DefaultOptions)!;

		public static ValueTask<DataModelValue> FromJsonAsync(Stream stream, CancellationToken token = default) => JsonSerializer.DeserializeAsync<DataModelValue>(stream, DefaultOptions, token);

		public static string ToXml(DataModelValue value, DataModelConverterXmlOptions options = default) => XmlConverter.ToXml(value, (options & DataModelConverterXmlOptions.WriteIndented) != 0);

		public static byte[] ToXmlUtf8Bytes(DataModelValue value, DataModelConverterXmlOptions options = default)
		{
			using var memoryStream = new MemoryStream();
			XmlConverter.AsXmlToStream(value, (options & DataModelConverterXmlOptions.WriteIndented) != 0, memoryStream);

			return memoryStream.ToArray();
		}

		public static Task ToXmlAsync(Stream stream, DataModelValue value, DataModelConverterXmlOptions options = default, CancellationToken token = default) =>
				XmlConverter.AsXmlToStreamAsync(value, (options & DataModelConverterXmlOptions.WriteIndented) != 0, stream.InjectCancellationToken(token));

		public static DataModelValue FromXml(string xml) => XmlConverter.FromXml(xml);

		public static DataModelValue FromXml(ReadOnlySpan<byte> xml)
		{
			var bytes = ArrayPool<byte>.Shared.Rent(xml.Length);
			try
			{
				xml.CopyTo(bytes);
				var memoryStream = new MemoryStream(bytes, index: 0, xml.Length);

				return XmlConverter.FromXmlStream(memoryStream);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(bytes);
			}
		}

		public static ValueTask<DataModelValue> FromXmlAsync(Stream stream, CancellationToken token = default) => XmlConverter.FromXmlStreamAsync(stream.InjectCancellationToken(token));

		private class JsonValueConverter : JsonConverter<DataModelValue>
		{
			private readonly DataModelConverterJsonOptions _options;

			public JsonValueConverter(DataModelConverterJsonOptions options) => _options = options;

			public override DataModelValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
					reader.TokenType switch
					{
							JsonTokenType.True => true,
							JsonTokenType.False => false,
							JsonTokenType.Null => DataModelValue.Null,
							JsonTokenType.String when reader.TryGetDateTime(out var datetime) && datetime.Kind != DateTimeKind.Local => datetime,
							JsonTokenType.String when reader.TryGetDateTimeOffset(out var datetimeOffset) => datetimeOffset,
							JsonTokenType.String => reader.GetString(),
							JsonTokenType.Number => reader.GetDouble(),
							JsonTokenType.StartObject => JsonSerializer.Deserialize<DataModelList>(ref reader, options),
							JsonTokenType.StartArray => JsonSerializer.Deserialize<DataModelList>(ref reader, options),
							_ => Infrastructure.UnexpectedValue<DataModelValue>(reader.TokenType, Resources.Exception_NotExpectedTokenType)
					};

			public override void Write(Utf8JsonWriter writer, DataModelValue value, JsonSerializerOptions options)
			{
				switch (value.Type)
				{
					case DataModelValueType.Undefined when (_options & DataModelConverterJsonOptions.UndefinedToNull) != 0:
						writer.WriteNullValue();
						break;

					case DataModelValueType.Undefined when (_options & DataModelConverterJsonOptions.UndefinedToSkip) == 0:
						throw new JsonException(Resources.Exception_UndefinedValueNotAllowed);

					case DataModelValueType.Undefined:
						break;

					case DataModelValueType.Null:
						writer.WriteNullValue();
						break;

					case DataModelValueType.String:
						writer.WriteStringValue(value.AsString());
						break;

					case DataModelValueType.Number:
						writer.WriteNumberValue(value.AsNumber());
						break;

					case DataModelValueType.DateTime:
						var dataModelDateTime = value.AsDateTime();
						switch (dataModelDateTime.Type)
						{
							case DataModelDateTimeType.DateTime:
								writer.WriteStringValue(dataModelDateTime.ToDateTime());
								break;

							case DataModelDateTimeType.DateTimeOffset:
								writer.WriteStringValue(dataModelDateTime.ToDateTimeOffset());
								break;

							default:
								Infrastructure.UnexpectedValue(dataModelDateTime.Type);
								break;
						}

						break;

					case DataModelValueType.Boolean:
						writer.WriteBooleanValue(value.AsBoolean());
						break;

					case DataModelValueType.List:
						JsonSerializer.Serialize(writer, value.AsList(), options);
						break;

					default:
						Infrastructure.UnexpectedValue(value.Type, Resources.Exception_UnknownTypeForSerialization);
						break;
				}
			}
		}

		private class JsonListConverter : JsonConverter<DataModelList>
		{
			private readonly DataModelConverterJsonOptions _options;

			public JsonListConverter(DataModelConverterJsonOptions options) => _options = options;

			public override DataModelList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
					reader.TokenType switch
					{
							JsonTokenType.StartObject => ReadObject(ref reader, options),
							JsonTokenType.StartArray => ReadArray(ref reader, options),
							_ => Infrastructure.UnexpectedValue<DataModelList>(reader.TokenType)
					};

			public override void Write(Utf8JsonWriter writer, DataModelList list, JsonSerializerOptions options)
			{
				if (IsArray(list))
				{
					WriteArray(writer, list, options);
				}
				else
				{
					WriteObject(writer, list, options);
				}
			}

			private static DataModelList ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
			{
				var list = new DataModelList();

				Infrastructure.Assert(reader.TokenType == JsonTokenType.StartObject);

				reader.Read();

				while (reader.TokenType != JsonTokenType.EndObject)
				{
					var name = reader.GetString();
					Infrastructure.NotNull(name);

					var value = JsonSerializer.Deserialize<DataModelValue>(ref reader, options);

					list.Add(name, value);

					reader.Read();
				}

				reader.Read();

				return list;
			}

			private void WriteObject(Utf8JsonWriter writer, DataModelList list, JsonSerializerOptions options)
			{
				if (writer.CurrentDepth > options.MaxDepth)
				{
					throw new JsonException(Resources.Exception_CycleReferenceDetected);
				}

				writer.WriteStartObject();

				foreach (var pair in list.KeyValuePairs)
				{
					if (!string.IsNullOrEmpty(pair.Key))
					{
						if (!pair.Value.IsUndefined())
						{
							writer.WritePropertyName(pair.Key);
							JsonSerializer.Serialize(writer, pair.Value, options);
						}
						else if ((_options & DataModelConverterJsonOptions.UndefinedToSkipOrNull) == DataModelConverterJsonOptions.UndefinedToNull)
						{
							writer.WritePropertyName(pair.Key);
							writer.WriteNullValue();
						}
					}
				}

				writer.WriteEndObject();
			}

			private static DataModelList ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
			{
				var list = new DataModelList();

				Infrastructure.Assert(reader.TokenType == JsonTokenType.StartArray);

				reader.Read();

				while (reader.TokenType != JsonTokenType.EndArray)
				{
					var value = JsonSerializer.Deserialize<DataModelValue>(ref reader, options);

					list.Add(value);

					reader.Read();
				}

				reader.Read();

				return list;
			}

			private void WriteArray(Utf8JsonWriter writer, DataModelList list, JsonSerializerOptions options)
			{
				writer.WriteStartArray();

				var arrayLength = list.Count;
				for (var i = 0; i < arrayLength; i ++)
				{
					var value = list[i];
					if (!value.IsUndefined())
					{
						JsonSerializer.Serialize(writer, value, options);
					}
					else if ((_options & DataModelConverterJsonOptions.UndefinedToNull) != 0)
					{
						writer.WriteNullValue();
					}
					else if ((_options & DataModelConverterJsonOptions.UndefinedToSkip) == 0)
					{
						throw new JsonException(Resources.Exception_UndefinedValueNotAllowed);
					}
				}

				writer.WriteEndArray();
			}
		}
	}
}