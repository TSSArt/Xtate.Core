// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.IoC;

namespace Xtate.Core.Test;

[TestClass]
public class XPathDataModelTest
{
	[TestMethod]
	public async Task M1()
	{
		const string xml = @"<scxml version='1.0' xmlns='http://www.w3.org/2005/07/scxml' datamodel='xpath' initial='errorSwitch'>
<datamodel>
  <data id='company'>
    <about xmlns=''>
      <name>Example company</name>
      <website>example.com</website>
      <CEO>John Doe</CEO>
    </about>
  </data>
  <!--data id='employees' src='http://example.com/employees.xml'/-->
  <data id='defaultdata'/>
</datamodel>
<state id='currentBehavior'/>
<final id='newBehavior'/>
<state id='errorSwitch' xmlns:fn='http://www.w3.org/2005/xpath-functions'>
					<datamodel>
						<data id='str'/>
					</datamodel>
          
					<onentry>
						<assign location='$str' expr=""'errorSwitch'""/>
					</onentry>
          
					<transition cond='In($str)' target='newBehavior'/>
					<transition target='currentBehavior'/>

					</state>
</scxml>
					";

		var services = new ServiceCollection();
		services.AddModule<StateMachineHostModule>();

		//services.AddConstant<IServiceProviderDebugger>(_ => new ServiceProviderDebugger(new StreamWriter(File.Create(@"D:\Ser\s1.txt"))));
		var serviceProvider = services.BuildProvider();

		var host = (IHostController) await serviceProvider.GetRequiredService<StateMachineHost>();
		var stateMachineScopeManager = await serviceProvider.GetRequiredService<IStateMachineScopeManager>();

		await host.StartHost();

		var smc = new ScxmlStringStateMachine(xml);
		_ = await stateMachineScopeManager.ExecuteStateMachine(smc, SecurityContextType.NewStateMachine);

		//await host.WaitAllStateMachinesAsync();

		await host.StopHost();
	}

	[TestMethod]
	public async Task M2()
	{
		const string xml = @"<scxml version='1.0' xmlns='http://www.w3.org/2005/07/scxml' datamodel='xpath'>
<datamodel>
  <data id='src'>
    textValue
  </data>
  <data id='dst'/>
</datamodel>
<final>
  <onentry>
    <assign location='dst' expr='$src'/>
  </onentry>
  <donedata>
	<param name='result' expr='$dst'/>
  </donedata>
</final>
</scxml>
					";

		var ub = new Mock<IUnhandledErrorBehaviour>();
		ub.Setup(s => s.Behaviour).Returns(UnhandledErrorBehaviour.HaltStateMachine);

		var services = new ServiceCollection();

		//var fileLogWriter = new FileLogWriter("D:\\Ser\\sss5.txt");
		//var d = new ServiceProviderDebugger(new StreamWriter(File.Create("D:\\Ser\\sss6.txt", 1, FileOptions.WriteThrough), Encoding.UTF8, 1));
		//services.AddConstant<ILogWriter>(_ => fileLogWriter);
		services.AddConstant(ub.Object);

		//services.AddConstant<IServiceProviderDebugger>(_ => d);
		services.AddModule<StateMachineHostModule>();
		var serviceProvider = services.BuildProvider();
		var smc = new ScxmlStringStateMachine(xml);

		var host = (IHostController) await serviceProvider.GetRequiredService<StateMachineHost>();
		var stateMachineScopeManager = await serviceProvider.GetRequiredService<IStateMachineScopeManager>();
		await host.StartHost();

		_ = await stateMachineScopeManager.ExecuteStateMachine(smc, SecurityContextType.NewStateMachine);

		//await host.WaitAllStateMachinesAsync();

		await host.StopHost();
	}
}