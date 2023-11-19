#region Copyright © 2019-2023 Sergii Artemenko

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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NET6_0
#pragma warning disable SYSLIB0011 // Type or member is obsolete
#endif

namespace Xtate.IoC.Test;

[TestClass]
public class ExceptionsTest
{
	[TestMethod]
	public void InfrastructureException0Test()
	{
		// Arrange
		var ex = new InfrastructureException();

		// Act

		// Assert
		Assert.IsNotNull(ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void InfrastructureException1Test()
	{
		// Arrange
		var ex = new InfrastructureException(@"text");

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void InfrastructureException2Test()
	{
		// Arrange
		var exInner = new ApplicationException();
		var ex = new InfrastructureException(message: @"text", exInner);

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.AreSame(exInner, ex.InnerException);
	}

	[TestMethod]
	public void InfrastructureExceptionSerializationTest()
	{
		// Arrange
		var exInner = new ApplicationException(@"inner");
		var ex = new InfrastructureException(message: @"text", exInner);
		var bf = new BinaryFormatter();
		var ms = new MemoryStream();

		// Act
		bf.Serialize(ms, ex);
		ms.Position = 0;
		var newEx = (InfrastructureException) bf.Deserialize(ms);

		// Assert
		Assert.AreEqual(expected: @"text", newEx.Message);
		Assert.IsNotNull(newEx.InnerException);
		Assert.AreEqual(expected: @"inner", newEx.InnerException.Message);
		Assert.AreEqual(typeof(ApplicationException), newEx.InnerException.GetType());
	}

	[TestMethod]
	public void DependencyInjectionException0Test()
	{
		// Arrange
		var ex = new DependencyInjectionException();

		// Act

		// Assert
		Assert.IsNotNull(ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void DependencyInjectionException1Test()
	{
		// Arrange
		var ex = new DependencyInjectionException(@"text");

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void DependencyInjectionException2Test()
	{
		// Arrange
		var exInner = new ApplicationException();
		var ex = new DependencyInjectionException(message: @"text", exInner);

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.AreSame(exInner, ex.InnerException);
	}

	[TestMethod]
	public void DependencyInjectionExceptionSerializationTest()
	{
		// Arrange
		var exInner = new ApplicationException(@"inner");
		var ex = new DependencyInjectionException(message: @"text", exInner);
		var bf = new BinaryFormatter();
		var ms = new MemoryStream();

		// Act
		bf.Serialize(ms, ex);
		ms.Position = 0;
		var newEx = (DependencyInjectionException) bf.Deserialize(ms);

		// Assert
		Assert.AreEqual(expected: @"text", newEx.Message);
		Assert.IsNotNull(newEx.InnerException);
		Assert.AreEqual(expected: @"inner", newEx.InnerException.Message);
		Assert.AreEqual(typeof(ApplicationException), newEx.InnerException.GetType());
	}
}