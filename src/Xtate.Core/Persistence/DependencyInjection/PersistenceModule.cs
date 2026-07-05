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

using System.IO;
using Xtate.DataModel;
using Xtate.DataModel.DependencyInjection;
using Xtate.Interpreter;
using Xtate.Interpreter.DependencyInjection;
using Xtate.Interpreter.Services;
using Xtate.IoC;
using Xtate.IoC.DependencyInjection;
using Xtate.IoC.TransformArgs.DependencyInjection;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Persistence.DependencyInjection;

[InstantiatedByIoC]
public class PersistenceModule : Module<StateMachineInterpreterModule, PersistenceInterpreterModelBuilderModule, DataModelHandlersModule, IoCModule>
{
	protected override void AddServices()
	{
		Services.AddImplementationSync<InMemoryStorageNew, bool>().For<InMemoryStorage>().For<IStorage>();
		Services.AddImplementationSync<InMemoryStorageBaseline, ReadOnlyMemory<byte>>().For<InMemoryStorage>().For<IStorage>();
		Services.AddImplementation<StreamStorageNoRollback, Stream>().For<ITransactionalStorage>();
		Services.AddImplementation<StreamStorageWithRollback, Stream, int>().For<ITransactionalStorage>();

		Services.AddSharedImplementation<SuspendEventDispatcher>(SharedWithin.Container).For<SuspendEventDispatcher>().For<ISuspendEventDispatcher>();

		Services.AddType<InterpreterModelBuilder, IStateMachine, IDataModelHandler>();

		Services.AddFactory<PersistedInterpreterModelGetter>().For<IInterpreterModel>(SharedWithin.Scope);

		Services.AddFactory<DefaultTransactionalStorage>().For<ITransactionalStorage, string>();

		Services.AddForwarding(Forward<IStorage, string>.To<ITransactionalStorage>());
		
		Services.ForService<ITransactionalStorage, string>().UseArgValue(@"smd").IfAncestor<PersistedInterpreterModelGetter>();
		Services.ForService<ITransactionalStorage, string>().UseArgValue(@"ctx").IfAncestor<StateMachinePersistedContext>();

		Services.AddSharedImplementation<StateMachinePersistingInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
		Services.AddSharedImplementation<StateMachinePersistedContext>(SharedWithin.Scope).For<IStateMachinePersistenceContext>().For<IStateMachineContext>();

		Services.AddSharedTypeSync<SharedMemoryStreams<Any>>(SharedWithin.Container);
		Services.AddImplementation<InMemoryStorageProvider>().For<IStorageProvider>();
	}

	[InstantiatedByIoC]
	private class StreamStorageNoRollback(Stream stream) : StreamStorage(stream);

	[InstantiatedByIoC]
	private class StreamStorageWithRollback(Stream stream, int rollbackLevel) : StreamStorage(stream, rollbackLevel: rollbackLevel);

	[InstantiatedByIoC]
	private class InMemoryStorageNew(bool writeOnly) : InMemoryStorage(writeOnly);

	[InstantiatedByIoC]
	private class InMemoryStorageBaseline(ReadOnlyMemory<byte> baseline) : InMemoryStorage(baseline.Span);
}