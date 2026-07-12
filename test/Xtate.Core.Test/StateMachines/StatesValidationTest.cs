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

using Xtate.Class;
using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.StateMachines;

[TestClass]
public class StatesValidationTest
{
	private static Task<IStateMachine> Parse(string scxml)
	{
		var services = new ServiceCollection();
		var smc = new ScxmlStringStateMachine(scxml);
		smc.AddServices(services);
		var serviceProvider = services.BuildProvider();

		return serviceProvider.GetRequiredService<IStateMachine>().AsTask();
	}

	[ExcludeFromCodeCoverage]
	private static async Task Execute(string scxml)
	{
		var smc = new ScxmlStringStateMachine(scxml);

		var idle = new Mock<IDestroyOnIdleTimeout>();
		idle.Setup(x => x.IdleTimeout).Returns(TimeSpan.FromMilliseconds(5000));

		var container = Container.Create<StateMachineProcessorModule>(services =>
																		  services.AddConstant(idle.Object));

		var stateMachineScopeManager = await container.GetRequiredService<IStateMachineScopeManager>();

		await stateMachineScopeManager.Execute(smc, SecurityContextType.NewStateMachine);
	}

	[TestMethod]
	public async Task InitialChildWithMultipleTransitionsIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																			 """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																			   <state id="outer">
																			 	<initial>
																			 	  <transition target="first"/>
																			 	  <transition target="second"/>
																			 	</initial>
																			 	<state id="first"/>
																			 	<state id="second"/>
																			   </state>
																			 </scxml>
																			 """));
	}

	[TestMethod]
	public async Task InitialAttributeOnAtomicStateIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																			 """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																			   <state id="atomic" initial="missing"/>
																			 </scxml>
																			 """));
	}

	[TestMethod]
	public async Task IllegalChildUnderFinalIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																			 """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																			   <final id="done">
																			 	<transition target="other"/>
																			   </final>
																			   <final id="other"/>
																			 </scxml>
																			 """));
	}

	[TestMethod]
	public async Task UnknownHistoryTypeIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																			 """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																			   <state id="outer">
																			 	<history id="hist" type="full">
																			 	  <transition target="child"/>
																			 	</history>
																			 	<state id="child"/>
																			   </state>
																			 </scxml>
																			 """));
	}

	[TestMethod]
	public async Task HistoryAtRootIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																			 """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																			   <history id="hist">
																			 	<transition target="done"/>
																			   </history>
																			   <final id="done"/>
																			 </scxml>
																			 """));
	}

	[TestMethod]
	public async Task HistoryTransitionToUnknownTargetIsRejectedDuringExecutionBuild()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(() => Execute(
																		  """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																		    <state id="outer">
																		  	<history id="hist">
																		  	  <transition target="missing"/>
																		  	</history>
																		  	<transition target="hist"/>
																		    </state>
																		  </scxml>
																		  """));
	}
}