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

using Xtate.Interpreter;
using Xtate.Persistence;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class StorageManagerCoverageTest
{
	[TestMethod]
	public async Task DefaultStorageFactoryUsesTheUnpartitionedRequestedName()
	{
		var storage = Mock.Of<ITransactionalStorage>();
		var provider = new Mock<IStorageProvider>();
		provider.Setup(static item => item.GetTransactionalStorage(partition: null, "custom")).Returns(new ValueTask<ITransactionalStorage>(storage));
		var factory = new DefaultTransactionalStorage { StorageProvider = provider.Object };

		Assert.AreSame(storage, await factory.Factory("custom"));
		provider.Verify(static item => item.GetTransactionalStorage(partition: null, "custom"), Times.Once);
	}

	[TestMethod]
	public async Task StateMachineStorageManagerRoutesEveryStorageTypeAndRemoval()
	{
		var storage = Mock.Of<ITransactionalStorage>();
		var provider = new Mock<IStorageProvider>();
		provider.Setup(static item => item.GetTransactionalStorage(It.IsAny<string?>(), It.IsAny<string>())).Returns(new ValueTask<ITransactionalStorage>(storage));
		var sessionId = SessionId.FromString("storage-session");
		var manager = new StorageManager
					  {
						  StorageProvider = provider.Object,
						  StateMachineSessionId = Mock.Of<IStateMachineSessionId>(item => item.SessionId == sessionId)
					  };

		Assert.AreSame(storage, await manager.Factory(StorageType.StateMachineDefinition));
		Assert.AreSame(storage, await manager.Factory(StorageType.StateMachineContext));
		Assert.AreSame(storage, await manager.Factory(StorageType.HostContext));
		Assert.ThrowsExactly<InvalidOperationException>(() => manager.Factory((StorageType)int.MaxValue));
		await manager.RemoveStorage(sessionId);

		provider.Verify(item => item.GetTransactionalStorage("sm-" + sessionId, "smd"), Times.Once);
		provider.Verify(item => item.GetTransactionalStorage("sm-" + sessionId, "ctx"), Times.Once);
		provider.Verify(static item => item.GetTransactionalStorage("host", "ctx"), Times.Once);
		provider.Verify(item => item.RemoveAllTransactionalStorage("sm-" + sessionId), Times.Once);
	}
}
