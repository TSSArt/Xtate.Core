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

using System.Threading;
using Xtate.Class;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Persistence.DependencyInjection;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;

namespace Xtate.Core.Test.DevTests;

[TestClass]
public class HostStateMachineResumptionTest
{
	[TestMethod]
	[DoNotParallelize]
	public async Task HostStartDispatchesDelayedEventThatBecameOverdueWhileStopped()
	{
		var storageProvider = new StateMachinePersistenceTest.TestStorage();
		var persistenceOptions = Mock.Of<IPersistenceOptions>(options => options.PersistenceLevel == PersistenceLevel.StableState);
		var sessionId = SessionId.FromString("host-scheduled-event-session");
		var stateMachinePartition = "sm-" + sessionId;

		await using (var firstContainer = CreateContainer(storageProvider, persistenceOptions))
		{
			var host = await firstContainer.GetRequiredService<IStateMachineHost>();
			await host.Start();

			var scopeManager = await firstContainer.GetRequiredService<IStateMachineScopeManager>();
			var stateMachine = new ScxmlStringStateMachine(
								   """
								   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
								     <state id="waiting">
								       <onentry><send event="wake" delay="1s"/></onentry>
								       <transition event="wake" target="fin"/>
								     </state>
								     <final id="fin"/>
								   </scxml>
								   """)
							   {
								   SessionId = sessionId
							   };
			var result = await scopeManager.Start(stateMachine, SecurityContextType.NewStateMachine);

			await Task.Delay(millisecondsDelay: 100);

			var suspendEventDispatcher = await firstContainer.GetRequiredService<SuspendEventDispatcher>();
			suspendEventDispatcher.Suspend(setSuspendRequestedFlag: true);

			await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await result.GetResult());
			await host.Stop();
		}

		await Task.Delay(TimeSpan.FromSeconds(1));

		await using (var secondContainer = CreateContainer(storageProvider, persistenceOptions))
		{
			var host = await secondContainer.GetRequiredService<IStateMachineHost>();
			await host.Start();

			var timeout = DateTime.UtcNow.AddSeconds(15);

			while (storageProvider.ContainsPartition(stateMachinePartition) && DateTime.UtcNow < timeout)
			{
				await Task.Delay(millisecondsDelay: 10);
			}

			Assert.IsFalse(storageProvider.ContainsPartition(stateMachinePartition));
			await host.Stop();
		}
	}

	[TestMethod]
	public async Task HostStartResumesStateMachinePreservedBySuspend()
	{
		var storageProvider = new StateMachinePersistenceTest.TestStorage();
		var persistenceOptions = Mock.Of<IPersistenceOptions>(options => options.PersistenceLevel == PersistenceLevel.StableState);
		var sessionId = SessionId.FromString("host-resumption-session");
		var location = new Uri("https://example.test/state-machines/host-resumption.scxml");
		DataModelValue arguments = "host-resumption arguments";
		var stateMachinePartition = "sm-" + sessionId;

		await using (var firstContainer = CreateContainer(storageProvider, persistenceOptions))
		{
			var scopeManager = await firstContainer.GetRequiredService<IStateMachineScopeManager>();
			var collection = await firstContainer.GetRequiredService<IStateMachineCollection>();
			var stateMachine = new ScxmlStringStateMachine(
								   """
								   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
								     <state id="before">
								       <transition event="step" target="after"/>
								     </state>
								     <state id="after">
								       <transition event="complete" target="fin"/>
								     </state>
								     <final id="fin"/>
								   </scxml>
								   """)
							   {
								   SessionId = sessionId,
								   Location = location,
								   Arguments = arguments
							   };
			var result = await scopeManager.Start(stateMachine, SecurityContextType.NewStateMachine);

			await collection.Dispatch(
				sessionId,
				new IncomingEvent(new EventEntity("step")) { Type = EventType.External },
				CancellationToken.None);

			var suspendEventDispatcher = await firstContainer.GetRequiredService<SuspendEventDispatcher>();
			suspendEventDispatcher.Suspend(setSuspendRequestedFlag: true);

			await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await result.GetResult());
		}

		Assert.IsTrue(storageProvider.ContainsPartition(stateMachinePartition));

		await using (var secondContainer = CreateContainer(storageProvider, persistenceOptions))
		{
			var host = await secondContainer.GetRequiredService<IStateMachineHost>();
			await host.Start();

			var collection = await secondContainer.GetRequiredService<IStateMachineCollection>();
			await collection.Dispatch(
				sessionId,
				new IncomingEvent(new EventEntity("complete")) { Type = EventType.External },
				CancellationToken.None);

			var timeout = DateTime.UtcNow.AddSeconds(10);

			while (storageProvider.ContainsPartition(stateMachinePartition) && DateTime.UtcNow < timeout)
			{
				await Task.Delay(millisecondsDelay: 10);
			}

			Assert.IsFalse(storageProvider.ContainsPartition(stateMachinePartition));
			await host.Stop();
		}
	}

	private static Container CreateContainer(IStorageProvider storageProvider, IPersistenceOptions persistenceOptions) =>
		Container.Create<StateMachineProcessorModule, PersistenceModule>(services =>
																		 {
																			 services.AddConstant(storageProvider);
																			 services.AddConstant(persistenceOptions);
																		 });
}
