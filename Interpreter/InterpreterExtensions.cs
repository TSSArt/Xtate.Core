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

public static class InterpreterExtensions
{
	public static void RegisterInterpreterModelBuilder(this IServiceCollection services)
	{
		if (services.IsRegistered<InterpreterModelBuilder>())
		{
			return;
		}

		services.RegisterErrorProcessor();
		services.RegisterDataModelHandlers();
		services.RegisterResourceLoaders();

		services.AddTypeSync<EmptyInitialNode, DocumentIdNode, TransitionNode>();
		services.AddTypeSync<EmptyTransitionNode, DocumentIdNode, ImmutableArray<StateEntityNode>>();
		services.AddTypeSync<DoneDataNode, DocumentIdNode, IDoneData>();
		services.AddTypeSync<InitialNode, DocumentIdNode, IInitial>();
		services.AddTypeSync<TransitionNode, DocumentIdNode, ITransition>();
		services.AddTypeSync<StateNode, DocumentIdNode, IState>();
		services.AddTypeSync<ParallelNode, DocumentIdNode, IParallel>();
		services.AddTypeSync<CompoundNode, DocumentIdNode, IState>();
		services.AddTypeSync<StateMachineNode, DocumentIdNode, IStateMachine>();
		services.AddTypeSync<FinalNode, DocumentIdNode, IFinal>();
		services.AddTypeSync<HistoryNode, DocumentIdNode, IHistory>();
		services.AddTypeSync<DataModelNode, DocumentIdNode, IDataModel>();
		services.AddTypeSync<OnEntryNode, DocumentIdNode, IOnEntry>();
		services.AddTypeSync<OnExitNode, DocumentIdNode, IOnExit>();
		services.AddTypeSync<DataNode, DocumentIdNode, IData>();
		services.AddTypeSync<InvokeNode, DocumentIdNode, IInvoke>();
		services.AddTypeSync<CancelNode, DocumentIdNode, ICancel>();
		services.AddTypeSync<AssignNode, DocumentIdNode, IAssign>();
		services.AddTypeSync<ForEachNode, DocumentIdNode, IForEach>();
		services.AddTypeSync<IfNode, DocumentIdNode, IIf>();
		services.AddTypeSync<ElseIfNode, DocumentIdNode, IElseIf>();
		services.AddTypeSync<ElseNode, DocumentIdNode, IElse>();
		services.AddTypeSync<LogNode, DocumentIdNode, ILog>();
		services.AddTypeSync<RaiseNode, DocumentIdNode, IRaise>();
		services.AddTypeSync<SendNode, DocumentIdNode, ISend>();
		services.AddTypeSync<ScriptNode, DocumentIdNode, IScript>();
		services.AddTypeSync<RuntimeExecNode, DocumentIdNode, IExecutableEntity>();
		services.AddTypeSync<CustomActionNode, DocumentIdNode, ICustomAction>();
		services.AddTypeSync<ParamNode, DocumentIdNode, IParam>();
		services.AddTypeSync<ScriptExpressionNode, IScriptExpression>();
		services.AddTypeSync<ExternalScriptExpressionNode, IExternalScriptExpression>();
		services.AddTypeSync<ExternalDataExpressionNode, IExternalDataExpression>();
		services.AddTypeSync<ValueExpressionNode, IValueExpression>();
		services.AddTypeSync<LocationExpressionNode, ILocationExpression>();
		services.AddTypeSync<ConditionExpressionNode, IConditionExpression>();
		services.AddTypeSync<ContentNode, IContent>();
		services.AddTypeSync<FinalizeNode, IFinalize>();
		services.AddTypeSync<IdentifierNode, IIdentifier>();
		services.AddTypeSync<EventNode, IOutgoingEvent>();
		services.AddTypeSync<EventDescriptorNode, IEventDescriptor>();

		services.AddType<DataConverter>();
		services.AddType<InterpreterModelBuilder>();
	}

	public static void RegisterStateMachineInterpreter(this IServiceCollection services)
	{
		if (services.IsRegistered<IStateMachineInterpreter>())
		{
			return;
		}

		services.RegisterDataModelHandlers();
		services.RegisterInterpreterModelBuilder();
		services.RegisterLogging();

		services.AddSharedImplementation<InterpreterLogEnricher<IStateMachineInterpreter>>(SharedWithin.Scope).For<ILogEnricher<IStateMachineInterpreter>>();
		services.AddSharedImplementation<InterpreterLogEnricher<ILog>>(SharedWithin.Scope).For<ILogEnricher<ILog>>();
		services.AddSharedImplementation<InterpreterLogEnricher<IInvoke>>(SharedWithin.Scope).For<ILogEnricher<IInvoke>>();
		services.AddSharedImplementation<InterpreterLogEnricher<IEventController>>(SharedWithin.Scope).For<ILogEnricher<IEventController>>();

		services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Scope).For<IAssemblyTypeInfo>();

		services.AddImplementation<InterpreterXDataModelProperty>().For<IXDataModelProperty>();
		services.AddImplementation<DataModelXDataModelProperty>().For<IXDataModelProperty>();
		services.AddImplementation<ArgsXDataModelProperty>().For<IXDataModelProperty>();
		services.AddImplementation<ConfigurationXDataModelProperty>().For<IXDataModelProperty>();
		services.AddImplementation<HostXDataModelProperty>().For<IXDataModelProperty>();

		services.AddSharedImplementation<StateMachineSessionId>(SharedWithin.Scope).For<IStateMachineSessionId>();
		services.AddImplementation<InStateController>().For<IInStateController>();
		services.AddImplementation<DataModelController>().For<IDataModelController>();
		services.AddImplementation<EventController>().For<IEventController>();

		services.AddSharedImplementation<DataModelValueEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<ExceptionEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<StateEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<TransitionEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<EventEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<OutgoingEventEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<InvokeDataEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<InvokeIdEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<SendIdEntityParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();
		services.AddSharedImplementation<InterpreterStateParser<Any>>(SharedWithin.Scope).For<IEntityParserProvider<Any>>();

		services.AddSharedFactory<InterpreterModelGetter>(SharedWithin.Scope).For<IInterpreterModel>();
		services.AddSharedImplementation<EventQueue>(SharedWithin.Scope).For<IEventQueueReader>().For<IEventQueueWriter>();
		services.AddSharedImplementation<StateMachineContext>(SharedWithin.Scope).For<IStateMachineContext>();
		services.AddSharedImplementation<StateMachineInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
	}
}