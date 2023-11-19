using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test
{
	[TestClass]
	public class ServiceProviderDebuggerTest
	{
		private class Debugger : IServiceProviderDebugger
		{
			public void AfterFactory(TypeKey serviceKey)
			{
			}

			public void BeforeFactory(TypeKey serviceKey)
			{
			}

			public void FactoryCalled(TypeKey serviceKey)
			{
			}

			public void RegisterService(ServiceEntry serviceEntry)
			{
			}
		}

		[TestMethod]
		public async Task RegisterServiceProviderDebuggerTest()
		{
			// Arrange
			var dbg = new Debugger();
			var sc = new ServiceCollection();
			sc.AddTransient<IServiceProviderDebugger>(_ => dbg);
			sc.AddType<ServiceProviderDebuggerTest>();
			var sp = sc.BuildProvider();

			// Act
			var rService = await sp.GetRequiredService<IServiceProviderDebugger>();
			var oService = await sp.GetOptionalService<IServiceProviderDebugger>();
			var rServiceSync = sp.GetRequiredServiceSync<IServiceProviderDebugger>();
			var oServiceSync = sp.GetOptionalServiceSync<IServiceProviderDebugger>();

			// Assert
			Assert.AreSame(rService, dbg);
			Assert.AreSame(oService, dbg);
			Assert.AreSame(rServiceSync, dbg);
			Assert.AreSame(oServiceSync, dbg);
		}
	}
}
