﻿// Copyright © 2019-2024 Sergii Artemenko
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

public class StateMachineInterpreterModule : Module<DataModelHandlersModule, InterpreterModelBuilderModule, LoggingModule>
{
	protected override void AddServices()
	{
		Services.AddSharedImplementation<InterpreterLogEnricher<IStateMachineInterpreter>>(SharedWithin.Scope).For<ILogEnricher<IStateMachineInterpreter>>();
		Services.AddSharedImplementation<InterpreterLogEnricher<ILog>>(SharedWithin.Scope).For<ILogEnricher<ILog>>();
		Services.AddSharedImplementation<InterpreterLogEnricher<IInvoke>>(SharedWithin.Scope).For<ILogEnricher<IInvoke>>();
		Services.AddSharedImplementation<InterpreterLogEnricher<IEventController>>(SharedWithin.Scope).For<ILogEnricher<IEventController>>();

		Services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Scope).For<IAssemblyTypeInfo>();

		Services.AddImplementation<InterpreterXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<DataModelXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<ArgsXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<ConfigurationXDataModelProperty>().For<IXDataModelProperty>();
		Services.AddImplementation<HostXDataModelProperty>().For<IXDataModelProperty>();

		Services.AddSharedImplementation<StateMachineSessionId>(SharedWithin.Scope).For<IStateMachineSessionId>();
		Services.AddImplementation<InStateController>().For<IInStateController>();
		Services.AddImplementation<DataModelController>().For<IDataModelController>();
		Services.AddImplementation<EventController>().For<IEventController>();

		Services.AddSharedImplementation<DataModelValueEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<ExceptionEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<StateEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<TransitionEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<EventEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<OutgoingEventEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<InvokeDataEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<InvokeIdEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<SendIdEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		Services.AddSharedImplementation<InterpreterStateParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();

		Services.AddSharedFactory<InterpreterModelGetter>(SharedWithin.Scope).For<IInterpreterModel>();
		Services.AddSharedImplementation<EventQueue>(SharedWithin.Scope).For<IEventQueueReader>().For<IEventQueueWriter>();
		Services.AddSharedImplementation<StateMachineContext>(SharedWithin.Scope).For<IStateMachineContext>();
		Services.AddSharedImplementation<StateMachineInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
	}
}