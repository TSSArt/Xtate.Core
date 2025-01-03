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

using Xtate.DataModel;
using Xtate.IoC;

namespace Xtate.Core;

public class StateMachineInterpreterModule : Module<DataModelHandlersModule, InterpreterModelBuilderModule, ToolsModule>
{
	protected override void AddServices()
	{
		Services.AddSharedImplementation<DefaultStateMachineSessionId>(SharedWithin.Scope).For<IStateMachineSessionId>(Option.IfNotRegistered);
		Services.AddImplementation<NoStateMachineArguments>().For<IStateMachineArguments>(Option.IfNotRegistered);
		
		Services.AddImplementation<NoUnhandledErrorBehaviour>().For<IUnhandledErrorBehaviour>(Option.IfNotRegistered);
		Services.AddImplementation<NoNotifyStateChanged>().For<INotifyStateChanged>(Option.IfNotRegistered);
		Services.AddImplementation<NoExternalConnections>().For<IExternalCommunication>(Option.IfNotRegistered).For<IExternalServiceManager>(Option.IfNotRegistered);

		Services.AddImplementation<InterpreterInfoLogEnricher<Any>>().For<ILogEnricher<Any>>();
		Services.AddImplementation<InterpreterDebugLogEnricher<Any>>().For<ILogEnricher<Any>>();
		Services.AddImplementation<InterpreterVerboseLogEnricher<Any>>().For<ILogEnricher<Any>>();

		Services.AddImplementation<StateEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<TransitionEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<EventEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<EventVerboseEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<OutgoingEventEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<OutgoingEventVerboseEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<InvokeDataEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<InvokeDataVerboseEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<InvokeIdEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<SendIdEntityParser<Any>>().For<IEntityParserHandler<Any>>();
		Services.AddImplementation<InterpreterStateParser<Any>>().For<IEntityParserHandler<Any>>();

		Services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Scope).For<IAssemblyTypeInfo>();

		Services.AddImplementation<InterpreterXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<DataModelXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<ArgsXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<ConfigurationXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<HostXDataModelProperty>().For<IXDataModelProperty>();

		Services.AddType<StateMachineRuntimeError>(Option.IfNotRegistered);
		Services.AddImplementation<InStateController>().For<IInStateController>();
		Services.AddImplementation<DataModelController>().For<IDataModelController>();
		Services.AddImplementation<InvokeController>().For<IInvokeController>();

		Services.AddSharedFactory<InterpreterModelGetter>(SharedWithin.Scope).For<IInterpreterModel>();
		Services.AddSharedImplementation<EventQueue>(SharedWithin.Scope).For<IEventQueueReader>().For<IEventQueueWriter>().For<IEventDispatcher>();
		Services.AddSharedImplementation<StateMachineContext>(SharedWithin.Scope).For<IStateMachineContext>();
		Services.AddSharedImplementation<StateMachineInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
	}
}