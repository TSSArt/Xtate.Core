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

using System.Collections;
using System.Globalization;
using System.Reflection;
using Xtate.DataTypes;

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
public class DataModelValueEdgeCoverageTest
{
	[TestMethod]
	public void LazyAndObjectContainersForwardTypedAccessAndDefaultPaths()
	{
		var numberLazy = new LazySource(new DataModelValue(12));
		var number = new DataModelValue(numberLazy);
		Assert.AreEqual(DataModelNumber.FromInt32(12), number.AsNullableNumber());
		Assert.IsTrue(number.TryGetAs<ILazyValue>(out var returnedLazy));
		Assert.AreSame(numberLazy, returnedLazy);

		var dateTime = new DateTime(year: 2026, month: 8, day: 6, hour: 12, minute: 30, second: 0, DateTimeKind.Utc);
		var date = new DataModelValue(new LazySource(new DataModelValue(dateTime)));
		Assert.AreEqual(dateTime, date.AsDateTime().ToDateTime());
		Assert.AreEqual(dateTime, date.AsNullableDateTime()!.Value.ToDateTime());
		Assert.AreEqual(dateTime, date.AsDateTimeOrDefault()!.Value.ToDateTime());

		Assert.IsNull(DataModelValue.Undefined.AsDateTimeOrDefault());
		Assert.IsNull(new DataModelValue("not a date").AsDateTimeOrDefault());
		Assert.ThrowsExactly<ArgumentException>(() => new DataModelValue("not a number").AsNullableNumber());
		Assert.ThrowsExactly<ArgumentException>(() => new DataModelValue("not a date").AsDateTime());
		Assert.ThrowsExactly<ArgumentException>(() => new DataModelValue("not a date").AsNullableDateTime());

		var custom = new CustomObject("payload");
		var wrapped = DataModelValue.FromObject(custom);
		Assert.AreSame(custom, wrapped.AsIObject());
		Assert.IsTrue(wrapped.TryGetAs<CustomObject>(out var returnedCustom));
		Assert.AreSame(custom, returnedCustom);
		Assert.AreEqual(expected: "payload", wrapped.ToObject());
		Assert.IsFalse(wrapped.TryGetAs<IDictionary>(out _));
	}

	[TestMethod]
	public void ConvertibleAndFormattingPathsCoverMarkersListsAndFallbackConversions()
	{
		IConvertible undefined = DataModelValue.Undefined;
		IConvertible nullValue = DataModelValue.Null;
		IConvertible list = new DataModelValue([]);

		Assert.AreEqual(TypeCode.Empty, undefined.GetTypeCode());
		Assert.AreEqual(TypeCode.Empty, nullValue.GetTypeCode());
		Assert.AreEqual(TypeCode.Object, list.GetTypeCode());

		Assert.ThrowsExactly<InvalidCastException>(() => ((IConvertible)new DataModelValue(1)).ToDateTime(CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<InvalidCastException>(() => ((IConvertible)new DataModelValue(true)).ToDateTime(CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<InvalidCastException>(() => ((IConvertible)new DataModelValue(1)).ToType(typeof(DateTimeOffset), CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<InvalidCastException>(() => ((IConvertible)new DataModelValue(true)).ToType(typeof(DateTimeOffset), CultureInfo.InvariantCulture));

		Span<char> destination = stackalloc char[64];
		DataModelValue[] values =
		[
			DataModelValue.Undefined,
			DataModelValue.Null,
			"text",
			42,
			new DateTime(year: 2026, month: 8, day: 6),
			true,
			new DataModelList { [0] = "value" }
		];

		foreach (var value in values)
		{
			Assert.IsTrue(value.TryFormat(destination, out _, format: default, CultureInfo.InvariantCulture));
		}
	}

	[TestMethod]
	public void ObjectConversionHandlesPrimitiveCollectionsCyclesAnonymousTypesAndUnsupportedValues()
	{
		object[] primitives = [(sbyte)-1, (short)-2, (byte)3, (ushort)4, 5U, 6UL, 7F];
		Assert.IsTrue(primitives.Select(DataModelValue.FromObject).All(static value => value.Type == DataModelValueType.Number));

		var objectDictionary = new Dictionary<string, object>();
		objectDictionary["self"] = objectDictionary;
		var objectValue = DataModelValue.FromObject(objectDictionary).AsList();
		Assert.AreSame(objectValue, objectValue["self"].AsList());

		var stringDictionary = new Dictionary<string, string> { ["key"] = "value" };
		Assert.AreEqual(expected: "value", DataModelValue.FromObject(stringDictionary).AsList()["key"].AsString());

		var array = new ArrayList();
		array.Add(array);
		var arrayValue = DataModelValue.FromObject(array).AsList();
		Assert.AreSame(arrayValue, arrayValue[0].AsList());

		var anonymous = new { Name = "anonymous", Count = 2 };
		var anonymousValue = DataModelValue.FromObject(anonymous).AsList();
		Assert.AreEqual(expected: "anonymous", anonymousValue["Name"].AsString());
		Assert.AreEqual(expected: 2, anonymousValue["Count"].AsNumber().ToInt32());

		Assert.ThrowsExactly<ArgumentException>(() => DataModelValue.FromObject('x'));
		Assert.ThrowsExactly<ArgumentException>(() => DataModelValue.FromObject(new UnsupportedObject()));
	}

	[TestMethod]
	public void CorruptedBackingValueUsesDefensiveTypeFormattingAndObjectBranches()
	{
		object boxed = default(DataModelValue);
		typeof(DataModelValue).GetField(name: "_value", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(boxed, new UnsupportedObject());
		var corrupted = (DataModelValue)boxed;

		Assert.ThrowsExactly<InvalidOperationException>(() => _ = corrupted.Type);
		Assert.ThrowsExactly<InvalidOperationException>(() => corrupted.ToString(CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<InvalidOperationException>(corrupted.ToObject);
	}

	private sealed class LazySource(DataModelValue value) : ILazyValue
	{
	#region Interface ILazyValue

		public DataModelValue Value => value;

	#endregion
	}

	private sealed class CustomObject(object? value) : IObject
	{
	#region Interface IObject

		public object? ToObject() => value;

	#endregion
	}

	private sealed class UnsupportedObject;
}
