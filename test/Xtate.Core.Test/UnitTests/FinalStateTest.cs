// Copyright � 2019-2024 Sergii Artemenko
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

using Xtate.Builder;
using Xtate.Core;
using Xtate.IoC;

namespace Xtate.Test;

[TestClass]
public class FinalStateTest
{
	[TestMethod]
	public async Task Final_state_with_number_as_done_data_Should_return_same_value()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddModule<StateMachineFluentBuilderModule>();
		services.AddModule<StateMachineProcessorModule>();
		var serviceProvider = services.BuildProvider();
		var builder = await serviceProvider.GetRequiredService<StateMachineFluentBuilder>();

		var stateMachine = builder
						   .BeginFinal()
						   .SetDoneData(22)
						   .EndFinal()
						   .Build();

		//var stateMachineHost = (IHostController) await serviceProvider.GetRequiredService<StateMachineHost>();
		var stateMachineScopeManager = await serviceProvider.GetRequiredService<IStateMachineScopeManager>();

		//await using var stateMachineHost = new StateMachineHost(new StateMachineHostOptions());

		//await stateMachineHost.StartHost();

		// Act
		var result = await stateMachineScopeManager.Execute(new RuntimeStateMachine(stateMachine), SecurityContextType.NewStateMachine);

		//Assert
		Assert.AreEqual(expected: 22, result.AsNumber());
	}

	[TestMethod]
	public async Task Input_argument_Should_be_passed_as_return_value()
	{
		var services = new ServiceCollection();
		services.AddModule<StateMachineFluentBuilderModule>();
		services.AddModule<StateMachineProcessorModule>();
		var serviceProvider = services.BuildProvider();
		var builder = await serviceProvider.GetRequiredService<StateMachineFluentBuilder>();

		// Arrange
		var stateMachine = builder
						   .BeginFinal()
						   .SetDoneData(
							   () =>
							   {
								   var val = Runtime.DataModel["_x"].AsListOrEmpty()["args"].AsNumber();

								   return new DataModelValue(val);
							   })
						   .EndFinal()
						   .Build();

		//var stateMachineHost = (IHostController) await serviceProvider.GetRequiredService<StateMachineHost>();
		var smc = new RuntimeStateMachine(stateMachine) { Arguments = 33 };
		var stateMachineScopeManager = await serviceProvider.GetRequiredService<IStateMachineScopeManager>();
		//await using var stateMachineHost = new StateMachineHost(new StateMachineHostOptions());

		//await stateMachineHost.StartHost();

		// Act
		var result = await stateMachineScopeManager.Execute(smc, SecurityContextType.NewStateMachine);


		//Assert
		Assert.AreEqual(expected: 33, result.AsNumber());
	}
}