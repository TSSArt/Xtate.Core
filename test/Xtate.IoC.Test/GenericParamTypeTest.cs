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

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ClassNeverInstantiated.Local

namespace Xtate.IoC.Test;

[TestClass]
public class GenericParamTypeTest
{
	[TestMethod]
	public void SubTypeInt32Test()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddTypeSync<Gen<Any>, Any>();
		var serviceProvider = services.BuildProvider();

		// Act
		var service = serviceProvider.GetRequiredServiceSync<Gen<int>, int>(6);

		// Assert
		Assert.AreEqual(expected: 6, service.Val);
	}

	private class Gen<T>
	{
		public Gen(T val) => Val = val;

		public T Val { get; }
	}
}