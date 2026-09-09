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
	[Timeout(30000)]
	public async Task HostStartResumesNestedStateMachineInvoke()
	{
		var storageProvider = new StateMachinePersistenceTest.TestStorage();
		var persistenceOptions = Mock.Of<IPersistenceOptions>(options => options.PersistenceLevel == PersistenceLevel.StableState);
		var sessionId = SessionId.FromString("host-invoke-resumption-session");
		var stateMachinePartition = "sm-" + sessionId;

		await using (var firstContainer = CreateContainer(storageProvider, persistenceOptions))
		{
			var scopeManager = await firstContainer.GetRequiredService<IStateMachineScopeManager>();
			var stateMachine = new ScxmlStringStateMachine(
								   """
								   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
								     <state id="waiting">
								       <invoke id="child" type="scxml">
								         <content>
								           <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
								             <state id="child-waiting">
								               <transition event="complete" target="child-final"/>
								             </state>
								             <final id="child-final"/>
								           </scxml>
								         </content>
								       </invoke>
								       <transition event="go">
								         <send event="complete" target="#_child"/>
								       </transition>
								       <transition event="done.invoke.child" target="parent-final"/>
								     </state>
								     <final id="parent-final"/>
								   </scxml>
								   """)
							   {
								   SessionId = sessionId
							   };
			var result = await scopeManager.Start(stateMachine, SecurityContextType.NewStateMachine);

			await Task.Delay(millisecondsDelay: 500);

			await using (var activeInvokeStorage = await storageProvider.GetTransactionalStorage(stateMachinePartition, key: "inv"))
			{
				var activeInvokeBucket = new Bucket(activeInvokeStorage);
				var hasPrematureRemoval = activeInvokeBucket.Nested(1).TryGet(key: 0, out int prematureOperation);
				Assert.IsFalse(hasPrematureRemoval, $"Invoke completed before suspension with operation: {prematureOperation}");
			}

			var suspendEventDispatcher = await firstContainer.GetRequiredService<SuspendEventDispatcher>();
			suspendEventDispatcher.Suspend(setSuspendRequestedFlag: true);

			await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await result.GetResult());
		}

		Assert.IsTrue(storageProvider.ContainsPartition(stateMachinePartition));
		PersistedInvokeData persistedInvoke;

		await using (var invokeStorage = await storageProvider.GetTransactionalStorage(stateMachinePartition, key: "inv"))
		{
			var invokeBucket = new Bucket(invokeStorage);
			persistedInvoke = new PersistedInvokeData(invokeBucket.Nested(0).Nested(3));
			var hasSecondRecord = invokeBucket.Nested(1).TryGet(key: 0, out int secondOperation);
			Assert.IsFalse(hasSecondRecord, $"Unexpected second invoke record operation: {secondOperation}");
		}

		var suspendedInvokedPartition = "sm-" + SessionId.FromString(persistedInvoke.InvokeId.UniqueId.Value);
		Assert.IsTrue(storageProvider.ContainsPartition(suspendedInvokedPartition), $"Invoked state machine was not persisted in {suspendedInvokedPartition}.");

		await using (var secondContainer = CreateContainer(storageProvider, persistenceOptions))
		{
			var host = await secondContainer.GetRequiredService<IStateMachineHost>();
			var hostStart = host.Start().AsTask();
			var hostStartCompleted = await Task.WhenAny(hostStart, Task.Delay(TimeSpan.FromSeconds(5))) == hostStart;
			Assert.IsTrue(hostStartCompleted, message: "Host start did not finish while restoring the parent and its invoked state machine.");
			await hostStart;
			Assert.IsTrue(storageProvider.ContainsPartition(suspendedInvokedPartition), message: "Invoked state machine completed or was removed during restoration.");

			var stateMachines = await secondContainer.GetRequiredService<IStateMachineCollection>();
			await stateMachines.Dispatch(
				sessionId,
				new IncomingEvent(new EventEntity("go")) { Type = EventType.External },
				CancellationToken.None);

			var timeout = DateTime.UtcNow.AddSeconds(10);

			while (storageProvider.ContainsPartition(stateMachinePartition) && DateTime.UtcNow < timeout)
			{
				await Task.Delay(millisecondsDelay: 10);
			}

			var invokedPartition = "sm-" + SessionId.FromString(persistedInvoke.InvokeId.UniqueId.Value);
			Assert.IsFalse(
				storageProvider.ContainsPartition(stateMachinePartition),
				$"Parent remained persisted; invoked partition present: {storageProvider.ContainsPartition(invokedPartition)}.");
			Assert.IsFalse(storageProvider.ContainsPartition(invokedPartition));
			await host.Stop();
		}
	}

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
