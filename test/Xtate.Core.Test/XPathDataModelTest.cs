﻿#region Copyright © 2019-2023 Sergii Artemenko

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

<<<<<<< Updated upstream
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.DataModel.XPath;
=======
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
			var hostOld = new StateMachineHostBuilder()
					   //TODO:
					   //.AddResourceLoaderFactory(WebResourceLoaderFactory.Instance)
					   .Build(ServiceLocator.Create(s => s.AddXPath()));

			var services = new ServiceCollection();
			services.RegisterStateMachineHost();
			var serviceProvider = services.BuildProvider();

			var host = await serviceProvider.GetRequiredService<StateMachineHost>();
=======
		var services = new ServiceCollection();
		services.RegisterStateMachineHost();
		//services.AddForwarding<IServiceProviderDebugger>(_ => new ServiceProviderDebugger(new StreamWriter(File.Create(@"D:\Ser\s1.txt"))));
		var serviceProvider = services.BuildProvider();
>>>>>>> Stashed changes

		var host = await serviceProvider.GetRequiredService<StateMachineHost>();

		await host.StartHostAsync();

		_ = await host.ExecuteStateMachineAsync(xml);

		await host.WaitAllStateMachinesAsync();

		await host.StopHostAsync();
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

<<<<<<< Updated upstream
			var ub = new Moq.Mock<IUnhandledErrorBehaviour>();
			ub.Setup(s => s.Behaviour).Returns(UnhandledErrorBehaviour.HaltStateMachine);

			var services = new ServiceCollection();
			//var fileLogWriter = new FileLogWriter("D:\\Ser\\sss5.txt");
			//var d = new ServiceProviderDebugger(new StreamWriter(File.Create("D:\\Ser\\sss6.txt", 1, FileOptions.WriteThrough), Encoding.UTF8, 1));
			//services.AddForwarding<ILogWriter>(_ => fileLogWriter);
			services.AddForwarding(_ => ub.Object);
			//services.AddForwarding<IServiceProviderDebugger>(_ => d);
			services.RegisterStateMachineHost();
			var serviceProvider = services.BuildProvider();

			var host = await serviceProvider.GetRequiredService<StateMachineHost>();
			/*
			var host = new StateMachineHostBuilder()
					   //TODO:
					   //.AddResourceLoaderFactory(WebResourceLoaderFactory.Instance)
					   .Build(ServiceLocator.Create(s => s.AddXPath()));
			*/
			await host.StartHostAsync();
=======
		var ub = new Mock<IUnhandledErrorBehaviour>();
		ub.Setup(s => s.Behaviour).Returns(UnhandledErrorBehaviour.HaltStateMachine);

		var services = new ServiceCollection();
>>>>>>> Stashed changes

		//var fileLogWriter = new FileLogWriter("D:\\Ser\\sss5.txt");
		//var d = new ServiceProviderDebugger(new StreamWriter(File.Create("D:\\Ser\\sss6.txt", 1, FileOptions.WriteThrough), Encoding.UTF8, 1));
		//services.AddForwarding<ILogWriter>(_ => fileLogWriter);
		services.AddForwarding(_ => ub.Object);

		//services.AddForwarding<IServiceProviderDebugger>(_ => d);
		services.RegisterStateMachineHost();
		var serviceProvider = services.BuildProvider();

		var host = await serviceProvider.GetRequiredService<StateMachineHost>();
		await host.StartHostAsync();

		_ = await host.ExecuteStateMachineAsync(xml);

		await host.WaitAllStateMachinesAsync();

		await host.StopHostAsync();
	}
}