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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	[DebuggerTypeProxy(typeof(DebugView))]
	[DebuggerDisplay(value: "{ToObject()} ({Type})")]
	[Serializable]
	public readonly struct DataModelValue : IObject, IEquatable<DataModelValue>, IFormattable, IDynamicMetaObjectProvider, IConvertible, ISerializable
	{
		private static readonly object NullValue    = new Marker(DataModelValueType.Null);
		private static readonly object NumberValue  = new Marker(DataModelValueType.Number);
		private static readonly object BooleanValue = new Marker(DataModelValueType.Boolean);

		public static readonly DataModelValue Null = new((string?) null);

		private readonly long    _int64;
		private readonly object? _value;

		private DataModelValue(SerializationInfo info, StreamingContext context)
		{
			var val = info.GetValue(name: @"L", typeof(long));
			_int64 = val is long int64 ? int64 : 0;
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

		public DataModelValue(double value)
		{
			_value = NumberValue;
			_int64 = BitConverter.DoubleToInt64Bits(value);
		}

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
						null => DataModelValueType.Undefined,
						{ } val when val == NullValue => DataModelValueType.Null,
						{ } val when val == NumberValue => DataModelValueType.Number,
						{ } val when val == BooleanValue => DataModelValueType.Boolean,
						string => DataModelValueType.String,
						DateTimeValue => DataModelValueType.DateTime,
						DataModelList => DataModelValueType.List,
						ILazyValue lazyValue => lazyValue.Value.Type,
						_ => Infrastructure.UnexpectedValue<DataModelValueType>(_value)
				};

	#region Interface IConvertible

		public string ToString(IFormatProvider? provider) => ToString(format: null, provider);

		TypeCode IConvertible.GetTypeCode() =>
				Type switch
				{
						DataModelValueType.Undefined => TypeCode.Empty,
						DataModelValueType.Null => TypeCode.Empty,
						DataModelValueType.String => TypeCode.String,
						DataModelValueType.List => TypeCode.Object,
						DataModelValueType.Number => TypeCode.Double,
						DataModelValueType.DateTime => AsDateTime().GetTypeCode(),
						DataModelValueType.Boolean => TypeCode.Boolean,
						_ => Infrastructure.UnexpectedValue<TypeCode>(Type)
				};

		bool IConvertible.ToBoolean(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToBoolean(provider),
						DataModelValueType.DateTime => AsDateTime().ToBoolean(provider),
						DataModelValueType.Boolean => AsBoolean().ToBoolean(provider),
						_ => Convert.ToBoolean(ToObject(), provider)
				};

		byte IConvertible.ToByte(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToByte(provider),
						DataModelValueType.DateTime => AsDateTime().ToByte(provider),
						DataModelValueType.Boolean => AsBoolean().ToByte(provider),
						_ => Convert.ToByte(ToObject(), provider)
				};

		char IConvertible.ToChar(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToChar(provider),
						DataModelValueType.DateTime => AsDateTime().ToChar(provider),
						DataModelValueType.Boolean => AsBoolean().ToChar(provider),
						_ => Convert.ToChar(ToObject(), provider)
				};

		decimal IConvertible.ToDecimal(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToDecimal(provider),
						DataModelValueType.DateTime => AsDateTime().ToDecimal(provider),
						DataModelValueType.Boolean => AsBoolean().ToDecimal(provider),
						_ => Convert.ToDecimal(ToObject(), provider)
				};

		double IConvertible.ToDouble(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToDouble(provider),
						DataModelValueType.DateTime => AsDateTime().ToDouble(provider),
						DataModelValueType.Boolean => AsBoolean().ToDouble(provider),
						_ => Convert.ToDouble(ToObject(), provider)
				};

		short IConvertible.ToInt16(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToInt16(provider),
						DataModelValueType.DateTime => AsDateTime().ToInt16(provider),
						DataModelValueType.Boolean => AsBoolean().ToInt16(provider),
						_ => Convert.ToInt16(ToObject(), provider)
				};

		int IConvertible.ToInt32(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToInt32(provider),
						DataModelValueType.DateTime => AsDateTime().ToInt32(provider),
						DataModelValueType.Boolean => AsBoolean().ToInt32(provider),
						_ => Convert.ToInt32(ToObject(), provider)
				};

		long IConvertible.ToInt64(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToInt64(provider),
						DataModelValueType.DateTime => AsDateTime().ToInt64(provider),
						DataModelValueType.Boolean => AsBoolean().ToInt64(provider),
						_ => Convert.ToInt64(ToObject(), provider)
				};

		sbyte IConvertible.ToSByte(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToSByte(provider),
						DataModelValueType.DateTime => AsDateTime().ToSByte(provider),
						DataModelValueType.Boolean => AsBoolean().ToSByte(provider),
						_ => Convert.ToSByte(ToObject(), provider)
				};

		float IConvertible.ToSingle(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToSingle(provider),
						DataModelValueType.DateTime => AsDateTime().ToSingle(provider),
						DataModelValueType.Boolean => AsBoolean().ToSingle(provider),
						_ => Convert.ToSingle(ToObject(), provider)
				};

		ushort IConvertible.ToUInt16(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToUInt16(provider),
						DataModelValueType.DateTime => AsDateTime().ToUInt16(provider),
						DataModelValueType.Boolean => AsBoolean().ToUInt16(provider),
						_ => Convert.ToUInt16(ToObject(), provider)
				};

		uint IConvertible.ToUInt32(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToUInt32(provider),
						DataModelValueType.DateTime => AsDateTime().ToUInt32(provider),
						DataModelValueType.Boolean => AsBoolean().ToUInt32(provider),
						_ => Convert.ToUInt32(ToObject(), provider)
				};

		ulong IConvertible.ToUInt64(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToUInt64(provider),
						DataModelValueType.DateTime => AsDateTime().ToUInt64(provider),
						DataModelValueType.Boolean => AsBoolean().ToUInt64(provider),
						_ => Convert.ToUInt64(ToObject(), provider)
				};

		DateTime IConvertible.ToDateTime(IFormatProvider? provider) =>
				Type switch
				{
						DataModelValueType.Number => AsNumber().ToDateTime(provider),
						DataModelValueType.DateTime => AsDateTime().ToDateTime(provider),
						DataModelValueType.Boolean => AsBoolean().ToDateTime(provider),
						_ => Convert.ToDateTime(ToObject(), provider)
				};

		object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
		{
			if (conversionType == typeof(DateTimeOffset))
			{
				return ToDateTimeOffset(this);
			}

			return Type switch
			{
					DataModelValueType.Number => AsNumber().ToType(conversionType, provider),
					DataModelValueType.DateTime => AsDateTime().ToType(conversionType, provider),
					DataModelValueType.Boolean => AsBoolean().ToType(conversionType, provider),
					_ => Convert.ChangeType(ToObject()!, conversionType, provider)
			};

			DateTimeOffset ToDateTimeOffset(in DataModelValue val) =>
					val.Type switch
					{
							DataModelValueType.Number => new DateTimeOffset(val.AsNumber().ToDateTime(provider)),
							DataModelValueType.DateTime => val.AsDateTime().ToDateTimeOffset(),
							DataModelValueType.Boolean => new DateTimeOffset(val.AsBoolean().ToDateTime(provider)),
							_ => new DateTimeOffset(Convert.ToDateTime(val.ToObject(), provider))
					};
		}

	#endregion

	#region Interface IDynamicMetaObjectProvider

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this, Dynamic.CreateMetaObject);

	#endregion

	#region Interface IEquatable<DataModelValue>

		public bool Equals(DataModelValue other)
		{
			if (ReferenceEquals(_value, other._value) && _int64 == other._int64)
			{
				return true;
			}

			var val = this;

			while (val._value is ILazyValue lazyValue)
			{
				val = lazyValue.Value;
			}

			while (other._value is ILazyValue lazyValue)
			{
				other = lazyValue.Value;
			}

			return val._int64 == other._int64 && Equals(val._value, other._value);
		}

	#endregion

	#region Interface IFormattable

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			return Type switch
			{
					DataModelValueType.Number => AsNumber().ToString(format, formatProvider),
					DataModelValueType.DateTime => AsDateTime().ToString(format, formatProvider),
					DataModelValueType.Boolean => AsBoolean().ToString(formatProvider),
					_ => ObjectToString(ToObject(), format, formatProvider)
			};

			static string ObjectToString(object? obj, string? format, IFormatProvider? formatProvider) =>
					(!string.IsNullOrEmpty(format) && obj is IFormattable formattable
							? formattable.ToString(format, formatProvider)
							: Convert.ToString(obj, formatProvider)) ?? string.Empty;
		}

	#endregion

	#region Interface IObject

		public object? ToObject() =>
				_value switch
				{
						null => null,
						{ } val when val == NullValue => null,
						{ } val when val == NumberValue => BitConverter.Int64BitsToDouble(_int64),
						{ } val when val == BooleanValue => _int64 != 0,
						string str => str,
						DateTimeValue val => val.GetDataModelDateTime(_int64).ToObject(),
						DataModelList list => list,
						ILazyValue lazyValue => lazyValue.Value.ToObject(),
						_ => Infrastructure.UnexpectedValue<object>(_value)
				};

	#endregion

	#region Interface ISerializable

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			var val = this;

			while (val._value is ILazyValue lazyValue)
			{
				val = lazyValue.Value;
			}

			info.AddValue(name: @"L", val._int64);
			info.AddValue(name: @"V", val._value);
		}

	#endregion

		public static implicit operator DataModelValue(DataModelList? val)    => new(val);
		public static implicit operator DataModelValue(string? val)           => new(val);
		public static implicit operator DataModelValue(double val)            => new(val);
		public static implicit operator DataModelValue(DataModelDateTime val) => new(val);
		public static implicit operator DataModelValue(DateTimeOffset val)    => new(val);
		public static implicit operator DataModelValue(DateTime val)          => new(val);
		public static implicit operator DataModelValue(bool val)              => new(val);

		public static DataModelValue FromDataModelList(DataModelList? val)        => val;
		public static DataModelValue FromString(string? val)                      => val;
		public static DataModelValue FromDouble(double val)                       => val;
		public static DataModelValue FromDataModelDateTime(DataModelDateTime val) => val;
		public static DataModelValue FromDateTimeOffset(DateTimeOffset val)       => val;
		public static DataModelValue FromDateTime(DateTime val)                   => val;
		public static DataModelValue FromBoolean(bool val)                        => val;

		public bool IsUndefinedOrNull() => _value is null || _value == NullValue || _value is ILazyValue val && val.Value.IsUndefinedOrNull();

		public bool IsUndefined() => _value is null || _value is ILazyValue val && val.Value.IsUndefined();

		public DataModelList AsList() =>
				_value switch
				{
						DataModelList list => list,
						ILazyValue val => val.Value.AsList(),
						_ => throw new ArgumentException(Resources.Exception_DataModelValueIsNotDataModelList)
				};

		public DataModelList? AsNullableList() =>
				_value switch
				{
						DataModelList list => list,
						{ } val when val == NullValue => null,
						ILazyValue val => val.Value.AsNullableList(),
						_ => throw new ArgumentException(Resources.Exception_DataModelValueIsNotDataModelList)
				};

		public DataModelList? AsListOrDefault() =>
				_value switch
				{
						null => null,
						DataModelList list => list,
						ILazyValue val => val.Value.AsListOrDefault(),
						_ => null
				};

		public DataModelList AsListOrEmpty() =>
				_value switch
				{
						null => DataModelList.Empty,
						DataModelList list => list,
						ILazyValue val => val.Value.AsListOrEmpty(),
						_ => DataModelList.Empty
				};

		public string AsString() =>
				_value switch
				{
						string str => str,
						ILazyValue val => val.Value.AsString(),
						_ => throw new ArgumentException(Resources.Exception_DataModelValueIsNotString)
				};

		public string? AsNullableString() =>
				_value switch
				{
						string str => str,
						{ } val when val == NullValue => null,
						ILazyValue val => val.Value.AsNullableString(),
						_ => throw new ArgumentException(Resources.Exception_DataModelValueIsNotString)
				};

		public string? AsStringOrDefault() =>
				_value switch
				{
						null => null,
						string str => str,
						ILazyValue val => val.Value.AsStringOrDefault(),
						_ => null
				};

		public double AsNumber() =>
				_value == NumberValue
						? BitConverter.Int64BitsToDouble(_int64)
						: _value is ILazyValue val
								? val.Value.AsNumber()
								: throw new ArgumentException(Resources.Exception_DataModelValueIsNotNumber);

		public double? AsNumberOrDefault() =>
				_value == NumberValue
						? BitConverter.Int64BitsToDouble(_int64)
						: _value is ILazyValue val
								? val.Value.AsNumberOrDefault()
								: null;

		public bool AsBoolean() =>
				_value == BooleanValue
						? _int64 != 0
						: _value is ILazyValue val
								? val.Value.AsBoolean()
								: throw new ArgumentException(Resources.Exception_DataModelValueIsNotBoolean);

		public bool? AsBooleanOrDefault() =>
				_value == BooleanValue
						? _int64 != 0
						: _value is ILazyValue val
								? val.Value.AsBooleanOrDefault()
								: null;

		public DataModelDateTime AsDateTime() =>
				_value switch
				{
						DateTimeValue val => val.GetDataModelDateTime(_int64),
						ILazyValue lazyVal => lazyVal.Value.AsDateTime(),
						_ => throw new ArgumentException(Resources.Exception_DataModelValueIsNotDateTime)
				};

		public DataModelDateTime? AsDateTimeOrDefault() =>
				_value switch
				{
						null => null,
						DateTimeValue val => val.GetDataModelDateTime(_int64),
						ILazyValue lazyVal => lazyVal.Value.AsDateTimeOrDefault(),
						_ => null
				};

		public override bool Equals(object? obj) => obj is DataModelValue other && Equals(other);

		public override int GetHashCode()
		{
			var val = this;

			while (val._value is ILazyValue lazyValue)
			{
				val = lazyValue.Value;
			}

			return (val._value is not null ? val._value.GetHashCode() : 0) + val._int64.GetHashCode();
		}

		public static bool operator ==(DataModelValue left, DataModelValue right) => left.Equals(right);

		public static bool operator !=(DataModelValue left, DataModelValue right) => !left.Equals(right);

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

		internal DataModelValue DeepCloneWithMap(DataModelAccess targetAccess, ref Dictionary<object, DataModelList>? map) =>
				_value switch
				{
						null => this,
						DataModelList list => new DataModelValue(list.DeepCloneWithMap(targetAccess, ref map)),
						ILazyValue val => val.Value.DeepCloneWithMap(targetAccess, ref map),
						_ => this
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

				case ILazyValue val:
					val.Value.MakeDeepConstant();
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
					TypeCode.SByte => (sbyte) value,
					TypeCode.Int16 => (short) value,
					TypeCode.Int32 => (int) value,
					TypeCode.Int64 => (long) value,
					TypeCode.Byte => (byte) value,
					TypeCode.UInt16 => (ushort) value,
					TypeCode.UInt32 => (uint) value,
					TypeCode.UInt64 => (ulong) value,
					TypeCode.Single => (float) value,
					TypeCode.Double => (double) value,
					TypeCode.Decimal => (double) (decimal) value,
					TypeCode.Boolean => (bool) value,
					TypeCode.DateTime => (DateTime) value,
					TypeCode.String => (string) value,
					TypeCode.Object => FromUnknownObjectWithMap(value, ref map),
					_ => throw new ArgumentException(Resources.Exception_UnsupportedObjectType, nameof(value))
			};
		}

		private static DataModelValue FromUnknownObjectWithMap(object value, ref Dictionary<object, DataModelList>? map) =>
				value switch
				{
						DateTimeOffset val => new DataModelValue(val),
						DataModelValue val => val,
						IObject obj => FromObjectWithMap(obj.ToObject(), ref map),
						DataModelList list => new DataModelValue(list),
						IDictionary<string, object> dict => CreateDataModelObject(dict, ref map),
						IDictionary<string, string> dict => CreateDataModelObject(dict, ref map),
						IEnumerable array => CreateDataModelList(array, ref map),
						ILazyValue val => new DataModelValue(val),
						{ } when TryFromAnonymousType(value, ref map, out var val) => val,
						_ => throw new ArgumentException(Resources.Exception_UnsupportedObjectType, nameof(value))
				};

		private static bool TryFromAnonymousType(object value, ref Dictionary<object, DataModelList>? map, out DataModelValue result)
		{
			var type = value.GetType();

			if (!type.Name.StartsWith(@"VB$") && !type.Name.StartsWith(@"<>") || !type.Name.Contains(@"AnonymousType") ||
				type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Length == 0)
			{
				result = default;

				return false;
			}

			map ??= new Dictionary<object, DataModelList>();

			if (map.TryGetValue(value, out var val))
			{
				result = new DataModelValue(val);

				return true;
			}

			var caseInsensitive = type.Name.StartsWith(@"VB$");
			var list = new DataModelList(caseInsensitive);

			map[value] = list;

			foreach (var propertyInfo in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				list.Add(propertyInfo.Name, FromObjectWithMap(propertyInfo.GetValue(value), ref map), metadata: default);
			}

			result = new DataModelValue(list);

			return true;
		}

		private static DataModelValue CreateDataModelObject(IDictionary<string, object> dictionary, ref Dictionary<object, DataModelList>? map)
		{
			map ??= new Dictionary<object, DataModelList>();

			if (map.TryGetValue(dictionary, out var val))
			{
				return new DataModelValue(val);
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
			map ??= new Dictionary<object, DataModelList>();

			if (map.TryGetValue(dictionary, out var val))
			{
				return new DataModelValue(val);
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
			map ??= new Dictionary<object, DataModelList>();

			if (map.TryGetValue(enumerable, out var val))
			{
				return new DataModelValue(val);
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

		[Serializable]
		private sealed class Marker
		{
			private readonly DataModelValueType _mark;

			public Marker(DataModelValueType mark) => _mark = mark;

			private bool Equals(Marker other) => _mark == other._mark;

			public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Marker other && Equals(other);

			public override int GetHashCode() => (int) _mark;
		}

		[Serializable]
		private sealed class DateTimeValue
		{
			private const int Base             = 120000; // should be multiple of CacheGranularity and great then Boundary
			private const int CacheGranularity = 15;
			private const int Boundary         = 65536;

			private static ImmutableDictionary<int, DateTimeValue> _cachedOffsets = ImmutableDictionary<int, DateTimeValue>.Empty;

			private readonly int _data;

			private DateTimeValue(int data) => _data = data;

			public static DateTimeValue GetDateTimeValue(DataModelDateTime dataModelDateTime, out long utcTicks)
			{
				int data;

				switch (dataModelDateTime.Type)
				{
					case DataModelDateTimeType.DateTime:

						var dateTime = dataModelDateTime.ToDateTime();
						utcTicks = dateTime.Ticks;
						data = CacheGranularity * (int) dateTime.Kind;
						break;

					case DataModelDateTimeType.DateTimeOffset:

						var dateTimeOffset = dataModelDateTime.ToDateTimeOffset();
						utcTicks = dateTimeOffset.UtcTicks;
						data = (int) (dateTimeOffset.Offset.Ticks / TimeSpan.TicksPerMinute + Base);
						break;

					default:
						utcTicks = 0;
						return Infrastructure.UnexpectedValue<DateTimeValue>(dataModelDateTime.Type);
				}

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

			public override int GetHashCode() => 0;

			public override bool Equals(object? obj) => obj is DateTimeValue;
		}

		[PublicAPI]
		[ExcludeFromCodeCoverage]
		private class DebugView
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly DataModelValue _dataModelValue;

			public DebugView(DataModelValue dataModelValue) => _dataModelValue = dataModelValue;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public object? Value => _dataModelValue.ToObject();
		}

		internal class Dynamic : DynamicObject
		{
			private static readonly IDynamicMetaObjectProvider Instance = new Dynamic(default);

			private static readonly ConstructorInfo ConstructorInfo = typeof(Dynamic).GetConstructor(new[] { typeof(DataModelValue) })!;

			private readonly DataModelValue _value;

			public Dynamic(DataModelValue value) => _value = value;

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
}