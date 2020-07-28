#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test
{
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

			var host = new StateMachineHostBuilder()
					   .AddResourceLoader(new WebResourceLoader())
					   .Build();

			await host.StartHostAsync();

			var _ = await host.ExecuteStateMachineAsync(xml);

			await host.WaitAllStateMachinesAsync();

			await host.StopHostAsync();
		}
	}
}