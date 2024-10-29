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

using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace Xtate;

[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay(value: "{ToObject()} ({Type})")]
[CollectionBuilder(typeof(DataModelList), nameof(DataModelList.Create))]
[Serializable]
public readonly struct DataModelValue : IObject, IEquatable<DataModelValue>, ISpanFormattable, IDynamicMetaObjectProvider, IConvertible, ISerializable
{
	private static readonly object NullValue = new Marker(DataModelValueType.Null);

	private static readonly object BooleanValue = new Marker(DataModelValueType.Boolean);

	public static readonly DataModelValue Null = new((string?) null);

	private readonly long _int64;

	private readonly object? _value;

	private DataModelValue(SerializationInfo info, StreamingContext context)
	{
		_int64 = (long) info.GetValue(name: @"L", typeof(long))!;
		_value = info.GetValue(name: @"V", typeof(object));
	}

	public DataModelValue(DataModelList? value)
	{
		_value = value ?? NullValue;
		_int64 = 0;
	}

	public DataModelValue(string? value)
	{
		_value = value ?? NullValue;
		_int64 = 0;
	}

	public DataModelValue(int value) => _value = NumberValue.GetNumberValue(value, out _int64);

	public DataModelValue(long value) => _value = NumberValue.GetNumberValue(value, out _int64);

	public DataModelValue(double value) => _value = NumberValue.GetNumberValue(value, out _int64);

	public DataModelValue(decimal value) => _value = NumberValue.GetNumberValue(value, out _int64);

	public DataModelValue(DataModelNumber value) => _value = NumberValue.GetNumberValue(value, out _int64);

	public DataModelValue(DateTimeOffset value) => _value = DateTimeValue.GetDateTimeValue(value, out _int64);

	public DataModelValue(DateTime value) => _value = DateTimeValue.GetDateTimeValue(value, out _int64);

	public DataModelValue(DataModelDateTime value) => _value = DateTimeValue.GetDateTimeValue(value, out _int64);

	public DataModelValue(bool value)
	{
		_value = BooleanValue;
		_int64 = value ? 1 : 0;
	}

	public DataModelValue(ILazyValue? lazyValue)
	{
		_value = lazyValue ?? NullValue;
		_int64 = 0;
	}

	internal bool IsLazyValue => _value is ILazyValue;

	public DataModelValueType Type =>
		_value switch
		{
			null                                 => DataModelValueType.Undefined,
			{ } value when value == NullValue    => DataModelValueType.Null,
			{ } value when value == BooleanValue => DataModelValueType.Boolean,
			string                               => DataModelValueType.String,
			NumberValue                          => DataModelValueType.Number,
			DateTimeValue                        => DataModelValueType.DateTime,
			DataModelList                        => DataModelValueType.List,
			ILazyValue lazyValue                 => lazyValue.Value.Type,
			_                                    => throw Infra.Unmatched(_value.GetType())
		};

#region Interface IConvertible

	public string ToString(IFormatProvider? provider) => ToString(format: null, provider);

	TypeCode IConvertible.GetTypeCode() =>
		Type switch
		{
			DataModelValueType.Undefined => TypeCode.Empty,
			DataModelValueType.Null      => TypeCode.Empty,
			DataModelValueType.String    => TypeCode.String,
			DataModelValueType.List      => TypeCode.Object,
			DataModelValueType.Number    => AsNumber().GetTypeCode(),
			DataModelValueType.DateTime  => AsDateTime().GetTypeCode(),
			DataModelValueType.Boolean   => TypeCode.Boolean,
			_                            => throw Infra.Unmatched(Type)
		};

	bool IConvertible.ToBoolean(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Boolean  => AsBoolean(),
			DataModelValueType.Number   => AsNumber().ToBoolean(provider),
			DataModelValueType.DateTime => AsDateTime().ToBoolean(provider),
			_                           => Convert.ToBoolean(ToObject(), provider)
		};

	byte IConvertible.ToByte(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToByte(provider),
			DataModelValueType.Boolean  => AsBoolean().ToByte(provider),
			DataModelValueType.DateTime => AsDateTime().ToByte(provider),
			_                           => Convert.ToByte(ToObject(), provider)
		};

	char IConvertible.ToChar(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToChar(provider),
			DataModelValueType.Boolean  => AsBoolean().ToChar(provider),
			DataModelValueType.DateTime => AsDateTime().ToChar(provider),
			_                           => Convert.ToChar(ToObject(), provider)
		};

	decimal IConvertible.ToDecimal(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToDecimal(provider),
			DataModelValueType.Boolean  => AsBoolean().ToDecimal(provider),
			DataModelValueType.DateTime => AsDateTime().ToDecimal(provider),
			_                           => Convert.ToDecimal(ToObject(), provider)
		};

	double IConvertible.ToDouble(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToDouble(provider),
			DataModelValueType.Boolean  => AsBoolean().ToDouble(provider),
			DataModelValueType.DateTime => AsDateTime().ToDouble(provider),
			_                           => Convert.ToDouble(ToObject(), provider)
		};

	short IConvertible.ToInt16(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToInt16(provider),
			DataModelValueType.Boolean  => AsBoolean().ToInt16(provider),
			DataModelValueType.DateTime => AsDateTime().ToInt16(provider),
			_                           => Convert.ToInt16(ToObject(), provider)
		};

	int IConvertible.ToInt32(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToInt32(provider),
			DataModelValueType.Boolean  => AsBoolean().ToInt32(provider),
			DataModelValueType.DateTime => AsDateTime().ToInt32(provider),
			_                           => Convert.ToInt32(ToObject(), provider)
		};

	long IConvertible.ToInt64(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToInt64(provider),
			DataModelValueType.Boolean  => AsBoolean().ToInt64(provider),
			DataModelValueType.DateTime => AsDateTime().ToInt64(provider),
			_                           => Convert.ToInt64(ToObject(), provider)
		};

	sbyte IConvertible.ToSByte(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToSByte(provider),
			DataModelValueType.Boolean  => AsBoolean().ToSByte(provider),
			DataModelValueType.DateTime => AsDateTime().ToSByte(provider),
			_                           => Convert.ToSByte(ToObject(), provider)
		};

	float IConvertible.ToSingle(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToSingle(provider),
			DataModelValueType.Boolean  => AsBoolean().ToSingle(provider),
			DataModelValueType.DateTime => AsDateTime().ToSingle(provider),
			_                           => Convert.ToSingle(ToObject(), provider)
		};

	ushort IConvertible.ToUInt16(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToUInt16(provider),
			DataModelValueType.Boolean  => AsBoolean().ToUInt16(provider),
			DataModelValueType.DateTime => AsDateTime().ToUInt16(provider),
			_                           => Convert.ToUInt16(ToObject(), provider)
		};

	uint IConvertible.ToUInt32(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToUInt32(provider),
			DataModelValueType.Boolean  => AsBoolean().ToUInt32(provider),
			DataModelValueType.DateTime => AsDateTime().ToUInt32(provider),
			_                           => Convert.ToUInt32(ToObject(), provider)
		};

	ulong IConvertible.ToUInt64(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.Number   => AsNumber().ToUInt64(provider),
			DataModelValueType.Boolean  => AsBoolean().ToUInt64(provider),
			DataModelValueType.DateTime => AsDateTime().ToUInt64(provider),
			_                           => Convert.ToUInt64(ToObject(), provider)
		};

	DateTime IConvertible.ToDateTime(IFormatProvider? provider) =>
		Type switch
		{
			DataModelValueType.DateTime => AsDateTime().ToDateTime(provider),
			DataModelValueType.Number   => AsNumber().ToDateTime(provider),
			DataModelValueType.Boolean  => AsBoolean().ToDateTime(provider),
			_                           => Convert.ToDateTime(ToObject(), provider)
		};

	object IConvertible.ToType(Type conversionType, IFormatProvider? provider) =>
		conversionType == typeof(DateTimeOffset)
			? Type switch
			  {
				  DataModelValueType.DateTime => AsDateTime().ToDateTimeOffset(),
				  DataModelValueType.Number   => new DateTimeOffset(AsNumber().ToDateTime(provider)),
				  DataModelValueType.Boolean  => new DateTimeOffset(AsBoolean().ToDateTime(provider)),
				  _                           => new DateTimeOffset(Convert.ToDateTime(ToObject(), provider))
			  }
			: Type switch
			  {
				  DataModelValueType.Number   => AsNumber().ToType(conversionType, provider),
				  DataModelValueType.Boolean  => AsBoolean().ToType(conversionType, provider),
				  DataModelValueType.DateTime => AsDateTime().ToType(conversionType, provider),
				  _                           => Convert.ChangeType(ToObject()!, conversionType, provider)
			  };

#endregion

#region Interface IDynamicMetaObjectProvider

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this, Dynamic.CreateMetaObject);

#endregion

#region Interface IEquatable<DataModelValue>

	public bool Equals(DataModelValue other)
	{
		if (_int64 == other._int64 && ReferenceEquals(_value, other._value))
		{
			return true;
		}

		var value = this;
		ResolveLazy(ref value);
		ResolveLazy(ref other);

		if (value._int64 == other._int64 && Equals(value._value, other._value))
		{
			return true;
		}

		if (value._value is IValueEqualityComparer valueEqualityComparer)
		{
			return valueEqualityComparer.Equals(value._int64, other._value, other._int64);
		}

		return false;
	}

#endregion

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider) =>
		Type switch
		{
			DataModelValueType.Undefined => string.Empty,
			DataModelValueType.Null      => string.Empty,
			DataModelValueType.String    => AsString(),
			DataModelValueType.Number    => AsNumber().ToString(format, formatProvider),
			DataModelValueType.DateTime  => AsDateTime().ToString(format, formatProvider),
			DataModelValueType.Boolean   => AsBoolean().ToString(),
			DataModelValueType.List      => AsList().ToString(format, formatProvider),
			_                            => throw Infra.Unmatched(Type)
		};

#endregion

#region Interface IObject

	public object? ToObject() =>
		_value switch
		{
			null                                 => null,
			{ } value when value == NullValue    => null,
			{ } value when value == BooleanValue => _int64 != 0,
			string str                           => str,
			NumberValue value                    => value.GetDataModelNumber(_int64).ToObject(),
			DateTimeValue value                  => value.GetDataModelDateTime(_int64).ToObject(),
			DataModelList list                   => list,
			ILazyValue lazyValue                 => lazyValue.Value.ToObject(),
			_                                    => throw Infra.Unmatched(_value?.GetType())
		};

#endregion

#region Interface ISerializable

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		var value = this;

		ResolveLazy(ref value);

		info.AddValue(name: @"L", value._int64);
		info.AddValue(name: @"V", value._value);
	}

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? formatProvider) =>
		Type switch
		{
			DataModelValueType.Undefined => string.Empty.TryCopyTo(destination, out charsWritten),
			DataModelValueType.Null      => string.Empty.TryCopyTo(destination, out charsWritten),
			DataModelValueType.String    => AsString().TryCopyTo(destination, out charsWritten),
			DataModelValueType.Number    => AsNumber().TryFormat(destination, out charsWritten, format, formatProvider),
			DataModelValueType.DateTime  => AsDateTime().TryFormat(destination, out charsWritten, format, formatProvider),
			DataModelValueType.Boolean   => AsBoolean().TryFormat(destination, out charsWritten),
			DataModelValueType.List      => AsList().TryFormat(destination, out charsWritten, format, formatProvider),
			_                            => throw Infra.Unmatched(Type)
		};

#endregion

	public static implicit operator DataModelValue(DataModelList? value) => new(value);

	public static implicit operator DataModelValue(string? value) => new(value);

	public static implicit operator DataModelValue(int value) => new(value);

	public static implicit operator DataModelValue(long value) => new(value);

	public static implicit operator DataModelValue(double value) => new(value);

	public static implicit operator DataModelValue(decimal value) => new(value);

	public static implicit operator DataModelValue(DataModelNumber value) => new(value);

	public static implicit operator DataModelValue(DataModelDateTime value) => new(value);

	public static implicit operator DataModelValue(DateTimeOffset value) => new(value);

	public static implicit operator DataModelValue(DateTime value) => new(value);

	public static implicit operator DataModelValue(bool value) => new(value);

	public static DataModelValue FromDataModelList(DataModelList? value) => value;

	public static DataModelValue FromString(string? value) => value;

	public static DataModelValue FromInt32(int value) => value;

	public static DataModelValue FromInt64(long value) => value;

	public static DataModelValue FromDouble(double value) => value;

	public static DataModelValue FromDecimal(decimal value) => value;

	public static DataModelValue FromDataModelNumber(DataModelNumber value) => value;

	public static DataModelValue FromDataModelDateTime(DataModelDateTime value) => value;

	public static DataModelValue FromDateTimeOffset(DateTimeOffset value) => value;

	public static DataModelValue FromDateTime(DateTime value) => value;

	public static DataModelValue FromBoolean(bool value) => value;

	public bool IsUndefinedOrNull() => _value is null || _value == NullValue || (_value is ILazyValue value && value.Value.IsUndefinedOrNull());

	public bool IsUndefined() => _value is null || (_value is ILazyValue lazyValue && lazyValue.Value.IsUndefined());

	public DataModelList AsList() =>
		_value switch
		{
			DataModelList list   => list,
			ILazyValue lazyValue => lazyValue.Value.AsList(),
			_                    => throw new ArgumentException(Resources.Exception_DataModelValueIsNotDataModelList)
		};

	public DataModelList? AsNullableList() =>
		_value switch
		{
			DataModelList list                => list,
			{ } value when value == NullValue => null,
			ILazyValue lazyValue              => lazyValue.Value.AsNullableList(),
			_                                 => throw new ArgumentException(Resources.Exception_DataModelValueIsNotDataModelList)
		};

	public DataModelList? AsListOrDefault() =>
		_value switch
		{
			null                 => null,
			DataModelList list   => list,
			ILazyValue lazyValue => lazyValue.Value.AsListOrDefault(),
			_                    => null
		};

	public DataModelList AsListOrEmpty() =>
		_value switch
		{
			null                 => DataModelList.Empty,
			DataModelList list   => list,
			ILazyValue lazyValue => lazyValue.Value.AsListOrEmpty(),
			_                    => DataModelList.Empty
		};

	public string AsString() =>
		_value switch
		{
			string str           => str,
			ILazyValue lazyValue => lazyValue.Value.AsString(),
			_                    => throw new ArgumentException(Resources.Exception_DataModelValueIsNotString)
		};

	public string? AsNullableString() =>
		_value switch
		{
			string str                        => str,
			{ } value when value == NullValue => null,
			ILazyValue lazyValue              => lazyValue.Value.AsNullableString(),
			_                                 => throw new ArgumentException(Resources.Exception_DataModelValueIsNotString)
		};

	public string? AsStringOrDefault() =>
		_value switch
		{
			null                 => null,
			string str           => str,
			ILazyValue lazyValue => lazyValue.Value.AsStringOrDefault(),
			_                    => null
		};

	public DataModelNumber AsNumber() =>
		_value switch
		{
			NumberValue value  => value.GetDataModelNumber(_int64),
			ILazyValue lazyVal => lazyVal.Value.AsNumber(),
			_                  => throw new ArgumentException(Resources.Exception_DataModelValueIsNotNumber)
		};

	public DataModelNumber? AsNumberOrDefault() =>
		_value switch
		{
			null               => null,
			NumberValue value  => value.GetDataModelNumber(_int64),
			ILazyValue lazyVal => lazyVal.Value.AsNumberOrDefault(),
			_                  => null
		};

	public bool AsBoolean() =>
		_value == BooleanValue
			? _int64 != 0
			: _value is ILazyValue lazyValue
				? lazyValue.Value.AsBoolean()
				: throw new ArgumentException(Resources.Exception_DataModelValueIsNotBoolean);

	public bool? AsBooleanOrDefault() =>
		_value == BooleanValue
			? _int64 != 0
			: _value is ILazyValue lazyValue
				? lazyValue.Value.AsBooleanOrDefault()
				: null;

	public DataModelDateTime AsDateTime() =>
		_value switch
		{
			DateTimeValue value => value.GetDataModelDateTime(_int64),
			ILazyValue lazyVal  => lazyVal.Value.AsDateTime(),
			_                   => throw new ArgumentException(Resources.Exception_DataModelValueIsNotDateTime)
		};

	public DataModelDateTime? AsDateTimeOrDefault() =>
		_value switch
		{
			null                => null,
			DateTimeValue value => value.GetDataModelDateTime(_int64),
			ILazyValue lazyVal  => lazyVal.Value.AsDateTimeOrDefault(),
			_                   => null
		};

	public IObject AsIObject() =>
		_value switch
		{
			ObjectContainer container => container.GetIObject(),
			_                         => this
		};

	public override bool Equals(object? obj) => obj is DataModelValue other && Equals(other);

	public override int GetHashCode()
	{
		var value = this;

		ResolveLazy(ref value);

		if (value._value is null)
		{
			return HashCode.Combine(value._int64);
		}

		if (value._value is IValueEqualityComparer valueEqualityComparer)
		{
			return valueEqualityComparer.GetHashCode(value._int64);
		}

		return HashCode.Combine(value._value, value._int64);
	}

	private static void ResolveLazy(ref DataModelValue value)
	{
		while (value._value is ILazyValue lazyValue)
		{
			value = lazyValue.Value;
		}
	}

	private static bool IsNaN(DataModelValue value) => value.AsNumberOrDefault()?.IsNaN() ?? false;

	public static bool operator ==(DataModelValue left, DataModelValue right) => left.Equals(right) && !IsNaN(left) && !IsNaN(right);

	public static bool operator !=(DataModelValue left, DataModelValue right) => !(left == right);

	public DataModelValue CloneAsWritable()
	{
		Dictionary<object, DataModelList>? map = default;

		return DeepCloneWithMap(DataModelAccess.Writable, ref map);
	}

	public DataModelValue CloneAsReadOnly()
	{
		Dictionary<object, DataModelList>? map = default;

		return DeepCloneWithMap(DataModelAccess.ReadOnly, ref map);
	}

	public DataModelValue AsConstant()
	{
		Dictionary<object, DataModelList>? map = default;

		return DeepCloneWithMap(DataModelAccess.Constant, ref map);
	}

	public dynamic AsDynamic() => this;

	internal DataModelValue DeepCloneWithMap(DataModelAccess targetAccess, ref Dictionary<object, DataModelList>? map) =>
		_value switch
		{
			null                 => this,
			DataModelList list   => new DataModelValue(list.DeepCloneWithMap(targetAccess, ref map)),
			ILazyValue lazyValue => lazyValue.Value.DeepCloneWithMap(targetAccess, ref map),
			_                    => this
		};

	public void MakeDeepConstant()
	{
		switch (_value)
		{
			case null:
				break;

			case DataModelList list:
				list.MakeDeepConstant();

				break;

			case ILazyValue lazyValue:
				lazyValue.Value.MakeDeepConstant();

				break;
		}
	}

	public static DataModelValue FromObject(object? value)
	{
		Dictionary<object, DataModelList>? map = default;

		return FromObjectWithMap(value, ref map);
	}

	private static DataModelValue FromObjectWithMap(object? value, ref Dictionary<object, DataModelList>? map)
	{
		if (value is null)
		{
			return Null;
		}

		var type = value.GetType();

		return System.Type.GetTypeCode(type) switch
			   {
				   TypeCode.SByte    => (sbyte) value,
				   TypeCode.Int16    => (short) value,
				   TypeCode.Int32    => (int) value,
				   TypeCode.Int64    => (long) value,
				   TypeCode.Byte     => (byte) value,
				   TypeCode.UInt16   => (ushort) value,
				   TypeCode.UInt32   => (uint) value,
				   TypeCode.UInt64   => (decimal) (ulong) value,
				   TypeCode.Single   => (float) value,
				   TypeCode.Double   => (double) value,
				   TypeCode.Decimal  => (decimal) value,
				   TypeCode.Boolean  => (bool) value,
				   TypeCode.DateTime => (DateTime) value,
				   TypeCode.String   => (string) value,
				   TypeCode.Object   => FromUnknownObjectWithMap(value, ref map),
				   _                 => throw new ArgumentException(Resources.Exception_UnsupportedObjectType, nameof(value))
			   };
	}

	private static DataModelValue FromUnknownObjectWithMap(object obj, ref Dictionary<object, DataModelList>? map) =>
		obj switch
		{
			DateTimeOffset dateTimeOffset                                   => new DataModelValue(dateTimeOffset),
			DataModelValue value                                            => value,
			IObject value                                                   => new DataModelValue(new ObjectContainer(value)),
			DataModelList list                                              => new DataModelValue(list),
			IDictionary<string, object> dictionary                          => CreateDataModelObject(dictionary, ref map),
			IDictionary<string, string> dictionary                          => CreateDataModelObject(dictionary, ref map),
			IEnumerable array                                               => CreateDataModelList(array, ref map),
			ILazyValue lazyValue                                            => new DataModelValue(lazyValue),
			not null when TryFromAnonymousType(obj, ref map, out var value) => value,
			_                                                               => throw new ArgumentException(Resources.Exception_UnsupportedObjectType, nameof(obj))
		};

	private static bool TryFromAnonymousType(object obj, ref Dictionary<object, DataModelList>? map, out DataModelValue result)
	{
		var type = obj.GetType();

		if ((!type.Name.StartsWith(@"VB$") && !type.Name.StartsWith(@"<>")) || !type.Name.Contains(@"AnonymousType") ||
			type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Length == 0)
		{
			result = default;

			return false;
		}

		map ??= [];

		if (map.TryGetValue(obj, out var value))
		{
			result = new DataModelValue(value);

			return true;
		}

		var caseInsensitive = type.Name.StartsWith(@"VB$");
		var list = new DataModelList(caseInsensitive);

		map[obj] = list;

		foreach (var propertyInfo in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
		{
			list.Add(propertyInfo.Name, FromObjectWithMap(propertyInfo.GetValue(obj), ref map), metadata: default);
		}

		result = new DataModelValue(list);

		return true;
	}

	private static DataModelValue CreateDataModelObject(IDictionary<string, object> dictionary, ref Dictionary<object, DataModelList>? map)
	{
		map ??= [];

		if (map.TryGetValue(dictionary, out var value))
		{
			return new DataModelValue(value);
		}

		var list = new DataModelList();

		map[dictionary] = list;

		foreach (var pair in dictionary)
		{
			list.Add(pair.Key, FromObjectWithMap(pair.Value, ref map), metadata: default);
		}

		return new DataModelValue(list);
	}

	private static DataModelValue CreateDataModelObject(IDictionary<string, string> dictionary, ref Dictionary<object, DataModelList>? map)
	{
		map ??= [];

		if (map.TryGetValue(dictionary, out var value))
		{
			return new DataModelValue(value);
		}

		var list = new DataModelList();

		map[dictionary] = list;

		foreach (var pair in dictionary)
		{
			list.Add(pair.Key, new DataModelValue(pair.Value), metadata: default);
		}

		return new DataModelValue(list);
	}

	private static DataModelValue CreateDataModelList(IEnumerable enumerable, ref Dictionary<object, DataModelList>? map)
	{
		map ??= [];

		if (map.TryGetValue(enumerable, out var value))
		{
			return new DataModelValue(value);
		}

		var list = new DataModelList();

		map[enumerable] = list;

		foreach (var item in enumerable)
		{
			list.Add(FromObjectWithMap(item, ref map));
		}

		return new DataModelValue(list);
	}

	public override string ToString() => ToString(format: null, formatProvider: null);

	private interface IValueEqualityComparer
	{
		bool Equals(long int64, object? otherValue, long otherInt64);

		int GetHashCode(long int64);
	}

	[Serializable]
	public class ObjectContainer(IObject obj) : ILazyValue
	{
		private DataModelValue? _value;

	#region Interface ILazyValue

		public DataModelValue Value => _value ??= FromObject(obj.ToObject());

	#endregion

		public IObject GetIObject() => obj;
	}

	[Serializable]
	private sealed class Marker(DataModelValueType mark)
	{
		private readonly DataModelValueType _mark = mark;

		private bool Equals(Marker other) => _mark == other._mark;

		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is Marker other && Equals(other));

		public override int GetHashCode() => HashCode.Combine(_mark);
	}

	[Serializable]
	private sealed class NumberValue : IValueEqualityComparer
	{
		private static readonly NumberValue Int32 = new(DataModelNumberType.Int32, int64Ext: 0);

		private static readonly NumberValue Int64 = new(DataModelNumberType.Int64, int64Ext: 0);

		private static readonly NumberValue Double = new(DataModelNumberType.Double, int64Ext: 0);

		private static ImmutableDictionary<long, NumberValue> _cachedDecimals = ImmutableDictionary<long, NumberValue>.Empty;

		private readonly long _int64Ext;

		private readonly DataModelNumberType _type;

		private NumberValue(DataModelNumberType type, long int64Ext)
		{
			_type = type;
			_int64Ext = int64Ext;
		}

	#region Interface IValueEqualityComparer

		public bool Equals(long int64, object? otherValue, long otherInt64) => otherValue is NumberValue other && GetDataModelNumber(int64).Equals(other.GetDataModelNumber(otherInt64));

		public int GetHashCode(long int64) => GetDataModelNumber(int64).GetHashCode();

	#endregion

		public static NumberValue GetNumberValue(int value, out long int64)
		{
			int64 = value;

			return Int32;
		}

		public static NumberValue GetNumberValue(long value, out long int64)
		{
			int64 = value;

			return Int64;
		}

		public static NumberValue GetNumberValue(double value, out long int64)
		{
			int64 = BitConverter.DoubleToInt64Bits(value);

			return Double;
		}

		public static NumberValue GetNumberValue(decimal value, out long int64)
		{
			var rawDecimal = (RawDecimal) value;

			int64 = rawDecimal.Lo64;

			if (rawDecimal.DoNotCache)
			{
				return new NumberValue(DataModelNumberType.Decimal, rawDecimal.Hi64);
			}

			var cachedDecimals = _cachedDecimals;

			if (!cachedDecimals.TryGetValue(rawDecimal.Hi64, out var numberValue))
			{
				numberValue = new NumberValue(DataModelNumberType.Decimal, rawDecimal.Hi64);

				_cachedDecimals = cachedDecimals.Add(rawDecimal.Hi64, numberValue);
			}

			return numberValue;
		}

		public static NumberValue GetNumberValue(DataModelNumber value, out long int64) =>
			value.Type switch
			{
				DataModelNumberType.Int32   => GetNumberValue(value.ToInt32(), out int64),
				DataModelNumberType.Int64   => GetNumberValue(value.ToInt64(), out int64),
				DataModelNumberType.Double  => GetNumberValue(value.ToDouble(), out int64),
				DataModelNumberType.Decimal => GetNumberValue(value.ToDecimal(), out int64),
				_                           => throw Infra.Unmatched(value.Type)
			};

		public DataModelNumber GetDataModelNumber(long int64) =>
			_type switch
			{
				DataModelNumberType.Int32   => (int) int64,
				DataModelNumberType.Int64   => int64,
				DataModelNumberType.Double  => BitConverter.Int64BitsToDouble(int64),
				DataModelNumberType.Decimal => (decimal) new RawDecimal(int64, _int64Ext),
				_                           => throw Infra.Unmatched(_type)
			};

		public override int GetHashCode() => HashCode.Combine(_type, _int64Ext);

		public override bool Equals(object? obj) => obj is NumberValue other && _type == other._type && _int64Ext == other._int64Ext;
	}

	[Serializable]
	private sealed class DateTimeValue
	{
		private const int Base = 120000; // should be multiple of CacheGranularity and great then Boundary

		private const int CacheGranularity = 15;

		private const int Boundary = 65536;

		private static ImmutableDictionary<int, DateTimeValue> _cachedOffsets = ImmutableDictionary<int, DateTimeValue>.Empty;

		private readonly int _data;

		private DateTimeValue(int data) => _data = data;

		public static DateTimeValue GetDateTimeValue(DataModelDateTime dataModelDateTime, out long utcTicks) =>
			dataModelDateTime.Type switch
			{
				DataModelDateTimeType.DateTime       => GetDateTimeValue(dataModelDateTime.ToDateTime(), out utcTicks),
				DataModelDateTimeType.DateTimeOffset => GetDateTimeValue(dataModelDateTime.ToDateTimeOffset(), out utcTicks),
				_                                    => throw Infra.Unmatched(dataModelDateTime.Type)
			};

		public static DateTimeValue GetDateTimeValue(DateTime dateTime, out long utcTicks)
		{
			utcTicks = dateTime.Ticks;

			return GetDateTimeValue(CacheGranularity * (int) dateTime.Kind);
		}

		public static DateTimeValue GetDateTimeValue(DateTimeOffset dateTimeOffset, out long utcTicks)
		{
			utcTicks = dateTimeOffset.UtcTicks;

			return GetDateTimeValue((int) (dateTimeOffset.Offset.Ticks / TimeSpan.TicksPerMinute + Base));
		}

		private static DateTimeValue GetDateTimeValue(int data)
		{
			if (data % CacheGranularity != 0)
			{
				return new DateTimeValue(data);
			}

			var cachedOffsets = _cachedOffsets;

			if (!cachedOffsets.TryGetValue(data, out var dateTimeValue))
			{
				dateTimeValue = new DateTimeValue(data);

				_cachedOffsets = cachedOffsets.Add(data, dateTimeValue);
			}

			return dateTimeValue;
		}

		public DataModelDateTime GetDataModelDateTime(long utcTicks)
		{
			if (_data <= Boundary)
			{
				return new DateTime(utcTicks, (DateTimeKind) (_data / CacheGranularity));
			}

			var offsetTicks = (_data - Base) * TimeSpan.TicksPerMinute;

			return new DateTimeOffset(utcTicks + offsetTicks, new TimeSpan(offsetTicks));
		}

		public override int GetHashCode() => HashCode.Combine(_data);

		public override bool Equals(object? obj) => obj is DateTimeValue other && _data == other._data;
	}

	[ExcludeFromCodeCoverage]
	private class DebugView(DataModelValue value)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly DataModelValue _value = value;

		[UsedImplicitly]
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object? Value => _value.ToObject();
	}

	internal class Dynamic(DataModelValue value) : DynamicObject
	{
		private static readonly Dynamic Instance = new(default);

		private static readonly ConstructorInfo ConstructorInfo = typeof(Dynamic).GetConstructor([typeof(DataModelValue)])!;

		private readonly DataModelValue _value = value;

		public static DynamicMetaObject CreateMetaObject(Expression expression)
		{
			var newExpression = Expression.New(ConstructorInfo, Expression.Convert(expression, typeof(DataModelValue)));

			return Instance.GetMetaObject(newExpression);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object? result)
		{
			if (_value._value is DataModelList list)
			{
				return new DataModelList.Dynamic(list).TryGetMember(binder, out result);
			}

			result = default;

			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object? value)
		{
			if (_value._value is DataModelList list)
			{
				return new DataModelList.Dynamic(list).TrySetMember(binder, value);
			}

			return false;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
		{
			if (_value._value is DataModelList list)
			{
				return new DataModelList.Dynamic(list).TryGetIndex(binder, indexes, out result);
			}

			result = default;

			return false;
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
		{
			if (_value._value is DataModelList list)
			{
				return new DataModelList.Dynamic(list).TrySetIndex(binder, indexes, value);
			}

			return false;
		}

		public override bool TryConvert(ConvertBinder binder, out object? result)
		{
			var typeCode = System.Type.GetTypeCode(binder.Type);

			switch (typeCode)
			{
				case TypeCode.Boolean:
					result = _value.AsBoolean();

					return true;

				case TypeCode.DateTime:
					result = _value.AsDateTime().ToDateTime();

					return true;

				case TypeCode.String:
					result = _value.AsString();

					return true;

				case TypeCode.Byte:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					result = Convert.ChangeType(_value.AsNumber(), typeCode, NumberFormatInfo.InvariantInfo);

					return true;
			}

			if (binder.Type == typeof(DateTimeOffset))
			{
				result = _value.AsDateTime().ToDateTimeOffset();

				return true;
			}

			if (binder.Type == typeof(DataModelList))
			{
				result = _value.AsList();

				return true;
			}

			result = default;

			return false;
		}
	}
}