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
public class DocumentConfigurationValidationTest
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
	public async Task RootElementMissingNamespaceIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>([ExcludeFromCodeCoverage]() => Parse("<scxml version='1.0'/>"));
	}

	[TestMethod]
	public async Task RootElementMissingVersionIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>([ExcludeFromCodeCoverage]() => Parse("<scxml xmlns='http://www.w3.org/2005/07/scxml'/>"));
	}

	[TestMethod]
	public async Task RootInitialUnknownTargetIsRejectedDuringExecutionBuild()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(() => Execute(
																		  """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="missing">
																		    <final id="done"/>
																		  </scxml>
																		  """));
	}

	[TestMethod]
	public async Task UnsupportedDataModelFailsPredictablyWhenRuntimeNeedsAHandler()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(() => Execute(
																		  """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="unsupported" initial="done">
																		    <datamodel>
																		  	<data id="value" expr="'value'"/>
																		    </datamodel>
																		    <final id="done"/>
																		  </scxml>
																		  """));
	}

	[TestMethod]
	public async Task ExtensionNamespaceRootElementIsRejectedByCurrentParserPolicy()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>([ExcludeFromCodeCoverage]() => Parse(
																			 """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" xmlns:ext="https://xtate.net/test-extension" version="1.0">
																			   <ext:metadata/>
																			 </scxml>
																			 """));
	}

	[TestMethod]
	public async Task DuplicateStateIdsAreRejectedDuringExecutionBuild()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(() => Execute(
																		  """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="dup">
																		    <state id="dup">
																		  	<transition target="done"/>
																		    </state>
																		    <state id="dup"/>
																		    <final id="done"/>
																		  </scxml>
																		  """));
	}

	[TestMethod]
	public void GeneratedStateIdsAllowAnonymousStatesToRun()
	{
		Parse(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			  <state/>
			  <parallel/>
			  <final/>
			</scxml>
			""");
	}
}