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

using Xtate.DataModel;
using Xtate.DataModel.DependencyInjection;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Services;
using Xtate.IoC;
using Xtate.IoC.DependencyInjection;
using Xtate.Logging.Provider;

namespace Xtate.Interpreter.DependencyInjection;

public class StateMachineInterpreterModule : Module<InterpreterModelBuilderModule, DataModelHandlersModule, IoCModule>
{
	protected override void AddServices()
	{
		Services.AddSharedImplementation<UniqueStateMachineSessionId>(SharedWithin.Scope).For<IStateMachineSessionId>(Option.IfNotRegistered);
		Services.AddSharedImplementation<NoStateMachineArguments>(SharedWithin.Scope).For<IStateMachineArguments>(Option.IfNotRegistered);

		Services.AddImplementation<NoExternalConnections>().For<IExternalCommunication>(Option.IfNotRegistered).For<IExternalServiceManager>(Option.IfNotRegistered);

		Services.AddImplementation<InterpreterInfoLogEnricher<Any>>().For<ILogEnricher<Any>>();
		Services.AddImplementation<InterpreterDebugLogEnricher<Any>>().For<ILogEnricher<Any>>();
		Services.AddImplementation<InterpreterVerboseLogEnricher<Any>>().For<ILogEnricher<Any>>();

		Services.AddImplementation<ExceptionEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<StateEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<TransitionEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<EventEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<EventVerboseEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<OutgoingEventEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<OutgoingEventVerboseEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<InvokeDataEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<InvokeDataVerboseEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<InvokeIdEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<SendIdEntityParser>().For<IEntityParserHandler>();
		Services.AddImplementation<InterpreterStateParser>().For<IEntityParserHandler>();

		Services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Scope).For<IAssemblyTypeInfo>();

		Services.AddImplementation<DataModelXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<ArgsXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<ConfigurationXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<HostXDataModelProperty>().For<IXDataModelProperty>();

		Services.AddType<StateMachineRuntimeError>(Option.IfNotRegistered);
		
		Services.AddImplementation<InStateController>().For<IInStateController>();
		Services.AddImplementation<DataModelController>().For<IDataModelController>();
		Services.AddImplementation<InvokeController>().For<IInvokeController>();
		Services.AddImplementation<LogController>().For<ILogController>();
		Services.AddImplementation<EventController>().For<IEventController>();

		Services.AddFactory<InterpreterModelGetter>().For<IInterpreterModel>(SharedWithin.Scope);
		Services.AddSharedImplementation<EventQueue>(SharedWithin.Scope).For<IEventReader>().For<IEventDispatcher>();
		Services.AddSharedImplementation<StateMachineContext>(SharedWithin.Scope).For<IStateMachineContext>();
		
		Services.AddSharedImplementation<StateMachineInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
	}
}