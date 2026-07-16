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
using Xtate.Interpreter;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class IoProcessorBaseCoverageTest
{
	[TestMethod]
	public async Task DefaultProcessorCoversSessionInvokeMatchingAndRouterForwarding()
	{
		var id = new FullUri("urn:processor");
		var alias = new FullUri("urn:alias");
		var sessionId = SessionId.FromString("session");
		var invokeId = InvokeId.FromString("invoke");
		var sessionProcessor = new TestIoProcessor(id, alias)
							   {
								   InvokeIdBase = null,
								   SessionIdBase = Mock.Of<IStateMachineSessionId>(source => source.SessionId == sessionId)
							   };
		var invokeProcessor = new TestIoProcessor(id, ioProcessorAlias: null)
							  {
								  InvokeIdBase = Mock.Of<IExternalServiceInvokeId>(source => source.InvokeId == invokeId),
								  SessionIdBase = Mock.Of<IStateMachineSessionId>(source => source.SessionId == sessionId)
							  };
		IIoProcessor sessionContract = sessionProcessor;
		IIoProcessor invokeContract = invokeProcessor;
		IEventRouter router = sessionProcessor;

		Assert.AreEqual(id, sessionContract.Id);
		Assert.IsNull(sessionContract.Target);
		Assert.IsNull(sessionContract.Target);
		Assert.IsNull(invokeContract.Target);
		Assert.IsTrue(router.CanHandle(id));
		Assert.IsTrue(router.CanHandle(alias));
		Assert.IsFalse(router.CanHandle(new FullUri("urn:other")));
		Assert.IsFalse(router.CanHandle(type: null));
		Assert.IsFalse(router.IsInternalTarget(new FullUri("urn:target")));

		var outgoingEvent = Mock.Of<IOutgoingEvent>(source => source.Name == (EventName) "event");
		var routerEvent = await router.GetRouterEvent(outgoingEvent, CancellationToken.None);
		Assert.AreEqual(sessionId, routerEvent.SenderServiceId);
		Assert.AreEqual(id, routerEvent.OriginType);
		Assert.IsNull(routerEvent.Target);
		Assert.AreEqual(expected: "event", routerEvent.Name.ToString());

		await router.Dispatch(routerEvent, CancellationToken.None);
		Assert.AreSame(routerEvent, sessionProcessor.DispatchedEvent);
	}

	private sealed class TestIoProcessor(FullUri ioProcessorId, FullUri? ioProcessorAlias) : IoProcessorBase(ioProcessorId, ioProcessorAlias)
	{
		public IRouterEvent? DispatchedEvent { get; private set; }

		protected override ValueTask Dispatch(IRouterEvent routerEvent, CancellationToken token)
		{
			DispatchedEvent = routerEvent;

			return ValueTask.CompletedTask;
		}
	}
}