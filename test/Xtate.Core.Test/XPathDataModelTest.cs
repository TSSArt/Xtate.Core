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