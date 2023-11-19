using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.IoC;

namespace Xtate.Core.Test.DI
{
	[TestClass]
	public class StateMachineInterpreterDiTest
	{
		[TestMethod]
		public async Task EmptyRun()
		{
			var services = new ServiceCollection();
			services.AddTransient<IStateMachine>(sp => new StateMachineEntity { States = ImmutableArray.Create<IStateEntity>(new FinalEntity()) });
			services.RegisterStateMachineInterpreter();

			var serviceProvider = services.BuildProvider();

			var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();

			await stateMachineInterpreter.RunAsync();
		}

		[TestMethod]
		public async Task XpathDataModelRun()
		{
			var services = new ServiceCollection();
			var stateMachineEntity = new StateMachineEntity
									 {
										 DataModelType = "xpath",
										 States = ImmutableArray.Create<IStateEntity>(
											 new FinalEntity
											 {
												 DoneData = new DoneDataEntity
															{
																Content = new ContentEntity
																		  {
																			  Body = new ContentBody { Value = "qwerty" }
																		  }
															}
											 })
									 };

			
			services.AddTransient<IStateMachine>(sp => stateMachineEntity);
			services.RegisterStateMachineInterpreter();

			var serviceProvider = services.BuildProvider();

			var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();

			var dataModelValue = await stateMachineInterpreter.RunAsync();

			Assert.AreEqual("qwerty", dataModelValue);
		}
	}
}
