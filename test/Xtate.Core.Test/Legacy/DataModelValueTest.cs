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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Legacy
{
	[TestClass]
	public class DataModelValueTest
	{
		[TestMethod]
		public void DataModelListNullTest()
		{
			// arrange
			DataModelList nullVal = null!;

			// act
			var v = (DataModelValue) nullVal;

			// assert
			Assert.AreEqual(DataModelValue.Null, v);
			Assert.AreEqual(expected: null, v.AsNullableList());
		}

		[TestMethod]
		public void DataModelStringNullTest()
		{
			// arrange
			string nullVal = null!;

			// act
			var v = (DataModelValue) nullVal;

			// assert
			Assert.AreEqual(DataModelValue.Null, v);
			Assert.AreEqual(expected: null, v.AsNullableString());
		}

		[TestMethod]
		public void EqualityInequalityTest()
		{
			Assert.AreEqual(expected: default, new DataModelValue());
			Assert.AreEqual(DataModelValue.Null, DataModelValue.Null);
			Assert.AreNotEqual(DataModelValue.Null, actual: default);
			Assert.AreNotEqual(notExpected: default, DataModelValue.Null);
		}

		[TestMethod]
		public void TypesTest()
		{
			Assert.AreEqual(DataModelValueType.Undefined, default(DataModelValue).Type);
			Assert.AreEqual(DataModelValueType.Undefined, new DataModelValue().Type);
			Assert.AreEqual(DataModelValueType.Null, DataModelValue.Null.Type);
			Assert.AreEqual(DataModelValueType.String, DataModelValue.FromString("str").Type);
			Assert.AreEqual(DataModelValueType.Boolean, DataModelValue.FromBoolean(false).Type);
			Assert.AreEqual(DataModelValueType.Number, DataModelValue.FromDouble(0).Type);
			Assert.AreEqual(DataModelValueType.DateTime, DataModelValue.FromDateTimeOffset(DateTimeOffset.MinValue).Type);
			Assert.AreEqual(DataModelValueType.List, DataModelValue.FromDataModelList(new DataModelList()).Type);
		}

		[TestMethod]
		public void FromListDictionaryCycleRefTest()
		{
			// arrange
			var dict = new Dictionary<string, object>();
			dict["self"] = dict;

			// act
			var dst = DataModelValue.FromObject(dict);

			// assert
			Assert.AreSame(dst.AsList(), dst.AsList()["self"].AsList());
		}

		[TestMethod]
		public void DeepCloneDictionaryCycleRefTest()
		{
			// arrange
			var dict = new DataModelList();
			dict["self"] = dict;
			var src = (DataModelValue) dict;

			// act
			var dst = src.CloneAsWritable();

			// assert
			Assert.AreSame(dst.AsList(), dst.AsList()["self"].AsList());
		}

		[TestMethod]
		public void MakeDeepConstantMakeDeepReadOnlyDictionaryCycleRefTest()
		{
			// arrange
			var dict = new DataModelList();
			dict["self"] = dict;
			var src = (DataModelValue) dict;

			// act
			src.MakeDeepConstant();

			// assert
			Assert.AreSame(src.AsList(), src.AsList()["self"].AsList());
		}

		[TestMethod]
		public void AnonymousTypeTest()
		{
			// arrange
			var at = new { Key = "Name" };

			// act
			var v = DataModelValue.FromObject(at);

			// assert
			Assert.AreEqual(expected: "Name", v.AsList()["Key"].AsString());
		}
	}
}