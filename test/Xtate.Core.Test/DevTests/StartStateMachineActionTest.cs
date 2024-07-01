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

<<<<<<<< Updated upstream:src/Xtate.DataModel.EcmaScript/src/Xtate.DataModel.EcmaScript/ServiceModule.cs
using Xtate.Core;
using Xtate.IoC;

namespace Xtate.DataModel.EcmaScript
{
	[UsedImplicitly]
	public class ServiceModule : IServiceModule
	{
		public void Register(IServiceCollection servicesCollection) => servicesCollection.AddEcmaScript();
========
namespace Xtate.Test.DevTests
{
	[TestClass]
	public class StartStateMachineActionTest
	{
		[TestMethod]
		public void RunStateMachineTest()
		{
			// arrange

			// act
			//await Host.ExecuteAsync(".\\Resources\\All.xml");
		}
>>>>>>>> Stashed changes:src/Xtate.Core/test/Xtate.Core.Test/DevTests/StartStateMachineActionTest.cs
	}
}