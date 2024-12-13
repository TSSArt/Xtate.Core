// Copyright © 2019-2024 Sergii Artemenko
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

using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtate.DataModel.XPath;

namespace Xtate;

public static class DataModelConverter
{
	[Flags]
	public enum JsonOptions
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
	public enum XmlOptions
	{
		WriteIndented = 1
	}

	private const string TypeMetaKey = @"type";

	private const string ObjectMetaValue = @"object";

	private const string ArrayMetaValue = @"array";

	private static readonly JsonSerializerOptions DefaultOptions = CreateOptions(JsonOptions.UndefinedNotAllowed);

	private static JsonSerializerOptions GetOptions(JsonOptions options) => options == JsonOptions.UndefinedNotAllowed ? DefaultOptions : CreateOptions(options);

	private static JsonSerializerOptions CreateOptions(JsonOptions options) =>
		new()
		{
			Converters =
			{
				new JsonValueConverter(options),
				new JsonListConverter(options)
			},
			WriteIndented = (options & JsonOptions.WriteIndented) != 0,
			MaxDepth = 64
		};

	public static bool IsArray(DataModelList list)
	{
		if (list.GetMetadata() is { } metadata && metadata[TypeMetaKey, caseInsensitive: false] is var value)
		{
			switch (value.AsStringOrDefault())
			{
				case ObjectMetaValue: return false;
				case ArrayMetaValue:  return true;
			}
		}

		return list is { Count: > 0, HasKeys: false };
	}

	public static bool IsObject(DataModelList list)
	{
		if (list.GetMetadata() is { } metadata && metadata[TypeMetaKey, caseInsensitive: false] is var value)
		{
			switch (value.AsStringOrDefault())
			{
				case ObjectMetaValue: return true;
				case ArrayMetaValue:  return false;
			}
		}

		return list is { Count: > 0, HasKeys: true };
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

	public static string ToJson(DataModelValue value, JsonOptions options = default) => JsonSerializer.Serialize(value, GetOptions(options));

	public static byte[] ToJsonUtf8Bytes(DataModelValue value, JsonOptions options = default) => JsonSerializer.SerializeToUtf8Bytes(value, GetOptions(options));

	public static Task ToJsonAsync(Stream stream,
								   DataModelValue value,
								   JsonOptions options = default,
								   CancellationToken token = default)
	{
		Infra.Requires(stream);

		return JsonSerializer.SerializeAsync(stream, value, GetOptions(options), token);
	}

	public static DataModelValue FromJson(string json)
	{
		Infra.Requires(json);

		return JsonSerializer.Deserialize<DataModelValue>(json, DefaultOptions);
	}

	public static async ValueTask<DataModelValue> FromJsonContentAsync(Resource resource)
	{
		Infra.Requires(resource);

		if (resource.Encoding.CodePage == 65001)
		{
			var stream = await resource.GetStream(true).ConfigureAwait(false);

			return await JsonSerializer.DeserializeAsync<DataModelValue>(stream, DefaultOptions).ConfigureAwait(false);
		}

		var content = await resource.GetContent().ConfigureAwait(false);

		return JsonSerializer.Deserialize<DataModelValue>(content, DefaultOptions);
	}

	public static DataModelValue FromJson(ReadOnlySpan<byte> utf8Json) => JsonSerializer.Deserialize<DataModelValue>(utf8Json, DefaultOptions);

	public static ValueTask<DataModelValue> FromJsonAsync(Stream stream, CancellationToken token = default)
	{
		Infra.Requires(stream);

		return JsonSerializer.DeserializeAsync<DataModelValue>(stream, DefaultOptions, token);
	}

	public static string ToXml(DataModelValue value, XmlOptions options = default) => XmlConverter.ToXml(value, (options & XmlOptions.WriteIndented) != 0);

	public static byte[] ToXmlUtf8Bytes(DataModelValue value, XmlOptions options = default)
	{
		using var memoryStream = new MemoryStream();
		XmlConverter.AsXmlToStream(value, (options & XmlOptions.WriteIndented) != 0, memoryStream);

		return memoryStream.ToArray();
	}

	public static Task ToXmlAsync(Stream stream,
								  DataModelValue value,
								  XmlOptions options = default,
								  CancellationToken token = default)
	{
		Infra.Requires(stream);

		return XmlConverter.AsXmlToStreamAsync(value, (options & XmlOptions.WriteIndented) != 0, stream.InjectCancellationToken(token));
	}

	public static DataModelValue FromXml(string xml)
	{
		Infra.Requires(xml);

		return XmlConverter.FromXml(xml);
	}

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

	public static ValueTask<DataModelValue> FromXmlAsync(Stream stream, CancellationToken token = default)
	{
		Infra.Requires(stream);

		return XmlConverter.FromXmlStreamAsync(stream.InjectCancellationToken(token));
	}

	private class JsonValueConverter(JsonOptions converterOptions) : JsonConverter<DataModelValue>
	{
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
				_ => throw Infra.Unmatched(reader.TokenType, Resources.Exception_NotExpectedTokenType)
			};

		public override void Write(Utf8JsonWriter writer, DataModelValue value, JsonSerializerOptions options)
		{
			switch (value.Type)
			{
				case DataModelValueType.Undefined when (converterOptions & JsonOptions.UndefinedToNull) != 0:
					writer.WriteNullValue();

					break;

				case DataModelValueType.Undefined when (converterOptions & JsonOptions.UndefinedToSkip) == 0:
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
					writer.WriteNumberValue(value.AsNumber().ToDouble());

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
							throw Infra.Unmatched(dataModelDateTime.Type);
					}

					break;

				case DataModelValueType.Boolean:
					writer.WriteBooleanValue(value.AsBoolean());

					break;

				case DataModelValueType.List:
					JsonSerializer.Serialize(writer, value.AsList(), options);

					break;

				default:
					throw Infra.Unmatched(value.Type, Resources.Exception_UnknownTypeForSerialization);
			}
		}
	}

	private class JsonListConverter(JsonOptions converterOptions) : JsonConverter<DataModelList>
	{
		public override DataModelList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType switch
			{
				JsonTokenType.StartObject => ReadObject(ref reader, options),
				JsonTokenType.StartArray  => ReadArray(ref reader, options),
				_                         => throw Infra.Unmatched(reader.TokenType)
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

			Infra.Assert(reader.TokenType == JsonTokenType.StartObject);

			reader.Read();

			while (reader.TokenType != JsonTokenType.EndObject)
			{
				var name = reader.GetString();
				Infra.NotNull(name);

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

			foreach (var (key, value) in list.KeyValuePairs)
			{
				if (!string.IsNullOrEmpty(key))
				{
					if (!value.IsUndefined())
					{
						writer.WritePropertyName(key);
						JsonSerializer.Serialize(writer, value, options);
					}
					else if ((converterOptions & JsonOptions.UndefinedToSkipOrNull) == JsonOptions.UndefinedToNull)
					{
						writer.WritePropertyName(key);
						writer.WriteNullValue();
					}
				}
			}

			writer.WriteEndObject();
		}

		private static DataModelList ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var list = new DataModelList();

			Infra.Assert(reader.TokenType == JsonTokenType.StartArray);

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
				else if ((converterOptions & JsonOptions.UndefinedToNull) != 0)
				{
					writer.WriteNullValue();
				}
				else if ((converterOptions & JsonOptions.UndefinedToSkip) == 0)
				{
					throw new JsonException(Resources.Exception_UndefinedValueNotAllowed);
				}
			}

			writer.WriteEndArray();
		}
	}
}