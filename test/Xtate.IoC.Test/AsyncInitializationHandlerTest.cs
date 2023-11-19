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

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class AsyncInitializationHandlerTest
{
	[TestMethod]
	public async Task AsyncInitializationHandlerInterfaceTest()
	{
		// Arrange
		var obj = new Class();
		var objAsyncInit = new ClassAsyncInit();
		ClassAsyncInit? nullObjAsyncInit = null;

		// Act
		var init1 = AsyncInitializationHandler.Instance.Initialize(obj);
		var init2 = AsyncInitializationHandler.Instance.Initialize(objAsyncInit);
		var init3 = AsyncInitializationHandler.Instance.Initialize(nullObjAsyncInit);

		await AsyncInitializationHandler.Instance.InitializeAsync(obj);
		await AsyncInitializationHandler.Instance.InitializeAsync(objAsyncInit);
		await AsyncInitializationHandler.Instance.InitializeAsync(nullObjAsyncInit);


		// Assert
		Assert.IsFalse(init1);
		Assert.IsTrue(init2);
		Assert.IsFalse(init3);
		Assert.IsTrue(objAsyncInit.Init);
	}

	// ReSharper disable All
	public class Class { }

	public class ClassAsyncInit : IAsyncInitialization
	{
		public bool Init;

	#region Interface IAsyncInitialization

		public Task Initialization
		{
			get
			{
				Init = true;

				return Task.CompletedTask;
			}
		}

	#endregion
	}

	// ReSharper restore All
}