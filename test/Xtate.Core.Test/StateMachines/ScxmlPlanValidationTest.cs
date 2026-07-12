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
public class ScxmlPlanValidationTest
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
	public async Task TransitionToUnknownTargetIsRejectedDuringExecutionBuild()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(() => Execute(
																		  """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
																		    <state id="start">
																		  	<transition target="missing"/>
																		    </state>
																		  </scxml>
																		  """));
	}

	[TestMethod]
	public void ExecutableContentDirectlyUnderStateIsRejected()
	{
		Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																	   """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																	     <state id="start">
																	   	<assign location="$x" expr="'bad'"/>
																	     </state>
																	   </scxml>
																	   """));
	}

	[TestMethod]
	public void MutuallyExclusiveDataInitializersAreRejected()
	{
		Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																	   """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath">
																	     <datamodel>
																	   	<data id="value" src="memory://value" expr="'value'"/>
																	     </datamodel>
																	   </scxml>
																	   """));
	}

	[TestMethod]
	public async Task NullDataModelRejectsDataManipulationDuringExecutionBuild()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(() => Execute(
																		  """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="null" initial="start">
																		    <state id="start">
																		  	<onentry>
																		  	  <assign location="$value" expr="'bad'"/>
																		  	</onentry>
																		  	<transition target="done"/>
																		    </state>
																		    <final id="done"/>
																		  </scxml>
																		  """));
	}

	[TestMethod]
	public void MutuallyExclusiveSendEventFormsAreRejected()
	{
		Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																	   """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
																	     <state id="start">
																	   	<onentry>
																	   	  <send event="literal" eventexpr="'expr'"/>
																	   	</onentry>
																	     </state>
																	   </scxml>
																	   """));
	}

	[TestMethod]
	public void InvokeWithMutuallyExclusiveSourceFormsIsRejected()
	{
		Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																	   """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath">
																	     <state id="start">
																	   	<invoke src="machine.scxml" srcexpr="'machine.scxml'"/>
																	     </state>
																	   </scxml>
																	   """));
	}

	[TestMethod]
	public void FinalizeOutsideInvokeIsRejected()
	{
		Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																	   """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																	     <state id="start">
																	   	<finalize/>
																	     </state>
																	   </scxml>
																	   """));
	}

	[TestMethod]
	public void MalformedXmlIsRejected()
	{
		Assert.ThrowsExactlyAsync<StateMachineValidationException>(() => Parse(
																	   """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
																	     <state id="start">
																	   </scxml>
																	   """));
	}
}