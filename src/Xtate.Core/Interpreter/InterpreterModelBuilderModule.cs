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

public class InterpreterModelBuilderModule : Module<ErrorProcessorModule, DataModelHandlersModule, ResourceLoadersModule>
{
	protected override void AddServices()
	{
		Services.AddTypeSync<InitialNode.Empty, DocumentIdNode, TransitionNode>();
		Services.AddTypeSync<TransitionNode.Empty, DocumentIdNode, ImmutableArray<StateEntityNode>>();
		Services.AddTypeSync<DoneDataNode, DocumentIdNode, IDoneData>();
		Services.AddTypeSync<InitialNode, DocumentIdNode, IInitial>();
		Services.AddTypeSync<TransitionNode, DocumentIdNode, ITransition>();
		Services.AddTypeSync<StateNode, DocumentIdNode, IState>();
		Services.AddTypeSync<ParallelNode, DocumentIdNode, IParallel>();
		Services.AddTypeSync<CompoundNode, DocumentIdNode, IState>();
		Services.AddTypeSync<StateMachineNode, DocumentIdNode, IStateMachine>();
		Services.AddTypeSync<FinalNode, DocumentIdNode, IFinal>();
		Services.AddTypeSync<HistoryNode, DocumentIdNode, IHistory>();
		Services.AddTypeSync<DataModelNode, DocumentIdNode, IDataModel>();
		Services.AddTypeSync<OnEntryNode, DocumentIdNode, IOnEntry>();
		Services.AddTypeSync<OnExitNode, DocumentIdNode, IOnExit>();
		Services.AddTypeSync<DataNode, DocumentIdNode, IData>();
		Services.AddTypeSync<InvokeNode, DocumentIdNode, IInvoke>();
		Services.AddTypeSync<CancelNode, DocumentIdNode, ICancel>();
		Services.AddTypeSync<AssignNode, DocumentIdNode, IAssign>();
		Services.AddTypeSync<ForEachNode, DocumentIdNode, IForEach>();
		Services.AddTypeSync<IfNode, DocumentIdNode, IIf>();
		Services.AddTypeSync<ElseIfNode, DocumentIdNode, IElseIf>();
		Services.AddTypeSync<ElseNode, DocumentIdNode, IElse>();
		Services.AddTypeSync<LogNode, DocumentIdNode, ILog>();
		Services.AddTypeSync<RaiseNode, DocumentIdNode, IRaise>();
		Services.AddTypeSync<SendNode, DocumentIdNode, ISend>();
		Services.AddTypeSync<ScriptNode, DocumentIdNode, IScript>();
		Services.AddTypeSync<RuntimeExecNode, DocumentIdNode, IExecutableEntity>();
		Services.AddTypeSync<CustomActionNode, DocumentIdNode, ICustomAction>();
		Services.AddTypeSync<ParamNode, DocumentIdNode, IParam>();
		Services.AddTypeSync<ScriptExpressionNode, IScriptExpression>();
		Services.AddTypeSync<ExternalScriptExpressionNode, IExternalScriptExpression>();
		Services.AddTypeSync<ExternalDataExpressionNode, IExternalDataExpression>();
		Services.AddTypeSync<ValueExpressionNode, IValueExpression>();
		Services.AddTypeSync<LocationExpressionNode, ILocationExpression>();
		Services.AddTypeSync<ConditionExpressionNode, IConditionExpression>();
		Services.AddTypeSync<ContentNode, IContent>();
		Services.AddTypeSync<FinalizeNode, IFinalize>();
		Services.AddTypeSync<IdentifierNode, IIdentifier>();
		Services.AddTypeSync<EventNode, IOutgoingEvent>();
		Services.AddTypeSync<EventDescriptorNode, IEventDescriptor>();

		Services.AddImplementation<NoStateMachineLocation>().For<IStateMachineLocation>(Option.IfNotRegistered);
		Services.AddType<DataConverter>(Option.IfNotRegistered);
		Services.AddType<InterpreterModelBuilder>();
	}
}