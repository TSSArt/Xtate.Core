// Copyright © 2019-2024 Sergii Artemenko
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

using System.IO;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.Persistence;

namespace Xtate.Core;

public class PersistenceModule : Module<InterpreterModelBuilderModule, StateMachineFactoryModule, DataModelHandlersModule>
{
	protected override void AddServices()
	{
		Services.AddImplementationSync<InMemoryStorageNew, bool>().For<InMemoryStorage>().For<IStorage>();
		Services.AddImplementationSync<InMemoryStorageBaseline, ReadOnlyMemory<byte>>().For<InMemoryStorage>().For<IStorage>();
		Services.AddImplementation<StreamStorageNoRollback, Stream>().For<ITransactionalStorage>();
		Services.AddImplementation<StreamStorageWithRollback, Stream, int>().For<ITransactionalStorage>();

		Services.AddDecorator<PersistedStateMachineService>().For<IStateMachineService>();
		Services.AddFactory<PersistedDataModelHandlerGetter>().For<IDataModelHandler>(Option.DoNotDispose);
		Services.AddImplementation<PersistedStateMachineRunState>().For<IPersistedStateMachineRunState>();

		Services.AddType<InterpreterModelBuilder, IStateMachine, IDataModelHandler>();

		Services.AddSharedFactory<PersistedInterpreterModelGetter>(SharedWithin.Scope).For<IInterpreterModel>();
	}

	[UsedImplicitly]
	private class StreamStorageNoRollback(Stream stream) : StreamStorage(stream);

	[UsedImplicitly]
	private class StreamStorageWithRollback(Stream stream, int rollbackLevel) : StreamStorage(stream, rollbackLevel: rollbackLevel);

	[UsedImplicitly]
	private class InMemoryStorageNew(bool writeOnly) : InMemoryStorage(writeOnly);

	[UsedImplicitly]
	private class InMemoryStorageBaseline(ReadOnlyMemory<byte> baseline) : InMemoryStorage(baseline.Span);
}