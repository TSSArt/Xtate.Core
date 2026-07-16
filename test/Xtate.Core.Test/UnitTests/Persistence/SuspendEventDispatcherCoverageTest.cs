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

using Xtate.Persistence;
using Xtate.Persistence.Services;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class SuspendEventDispatcherCoverageTest
{
	[TestMethod]
	public void SuspendRaisesSubscribedHandlersSupportsRemovalAndOptionallySetsFlag()
	{
		var dispatcher = new SuspendEventDispatcher();
		var contract = (ISuspendEventDispatcher) dispatcher;
		var calls = 0;

		void Handler() => calls ++;

		contract.OnSuspend += Handler;
		dispatcher.Suspend(setSuspendRequestedFlag: false);
		Assert.AreEqual(expected: 1, calls);
		Assert.IsFalse(contract.IsSuspendRequested);

		dispatcher.Suspend(setSuspendRequestedFlag: true);
		Assert.AreEqual(expected: 2, calls);
		Assert.IsTrue(contract.IsSuspendRequested);

		contract.OnSuspend -= Handler;
		dispatcher.Suspend(setSuspendRequestedFlag: false);
		Assert.AreEqual(expected: 2, calls);
	}
}