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

using Xtate.Interpreter.DependencyInjection;
using Xtate.Interpreter.Model;
using Xtate.IoC;
using Xtate.StateMachine;

namespace Xtate.Persistence.DependencyInjection;

[InstantiatedByIoC]
public class PersistenceInterpreterModelBuilderModule : Module<InterpreterModelBuilderModule>
{
	protected override void AddServices()
	{
		Services.AddImplementationSync<PersistedDoneDataNode, DocumentIdNode, IDoneData>().For<DoneDataNode>();
		Services.AddImplementationSync<PersistedInitialNode, DocumentIdNode, IInitial>().For<InitialNode>();
		Services.AddImplementationSync<PersistedTransitionNode, DocumentIdNode, ITransition>().For<TransitionNode>();
		Services.AddImplementationSync<PersistedStateNode, DocumentIdNode, IState>().For<StateNode>();
		Services.AddImplementationSync<PersistedParallelNode, DocumentIdNode, IParallel>().For<ParallelNode>();
		Services.AddImplementationSync<PersistedCompoundNode, DocumentIdNode, IState>().For<CompoundNode>();
		Services.AddImplementationSync<PersistedStateMachineNode, DocumentIdNode, IStateMachine>().For<StateMachineNode>();
		Services.AddImplementationSync<PersistedFinalNode, DocumentIdNode, IFinal>().For<FinalNode>();
		Services.AddImplementationSync<PersistedHistoryNode, DocumentIdNode, IHistory>().For<HistoryNode>();
		Services.AddImplementationSync<PersistedDataModelNode, DocumentIdNode, IDataModel>().For<DataModelNode>();
		Services.AddImplementationSync<PersistedOnEntryNode, DocumentIdNode, IOnEntry>().For<OnEntryNode>();
		Services.AddImplementationSync<PersistedOnExitNode, DocumentIdNode, IOnExit>().For<OnExitNode>();
		Services.AddImplementationSync<PersistedDataNode, DocumentIdNode, IData>().For<DataNode>();
		Services.AddImplementationSync<PersistedInvokeNode, DocumentIdNode, IInvoke>().For<InvokeNode>();
		Services.AddImplementationSync<PersistedCancelNode, DocumentIdNode, ICancel>().For<CancelNode>();
		Services.AddImplementationSync<PersistedAssignNode, DocumentIdNode, IAssign>().For<AssignNode>();
		Services.AddImplementationSync<PersistedForEachNode, DocumentIdNode, IForEach>().For<ForEachNode>();
		Services.AddImplementationSync<PersistedIfNode, DocumentIdNode, IIf>().For<IfNode>();
		Services.AddImplementationSync<PersistedElseIfNode, DocumentIdNode, IElseIf>().For<ElseIfNode>();
		Services.AddImplementationSync<PersistedElseNode, DocumentIdNode, IElse>().For<ElseNode>();
		Services.AddImplementationSync<PersistedLogNode, DocumentIdNode, ILog>().For<LogNode>();
		Services.AddImplementationSync<PersistedRaiseNode, DocumentIdNode, IRaise>().For<RaiseNode>();
		Services.AddImplementationSync<PersistedSendNode, DocumentIdNode, ISend>().For<SendNode>();
		Services.AddImplementationSync<PersistedScriptNode, DocumentIdNode, IScript>().For<ScriptNode>();
		Services.AddImplementationSync<PersistedRuntimeExecNode, DocumentIdNode, IExecutableEntity>().For<RuntimeExecNode>();
		Services.AddImplementationSync<PersistedCustomActionNode, DocumentIdNode, ICustomAction>().For<CustomActionNode>();
		Services.AddImplementationSync<PersistedParamNode, DocumentIdNode, IParam>().For<ParamNode>();
		Services.AddImplementationSync<PersistedScriptExpressionNode, IScriptExpression>().For<ScriptExpressionNode>();
		Services.AddImplementationSync<PersistedExternalScriptExpressionNode, IExternalScriptExpression>().For<ExternalScriptExpressionNode>();
		Services.AddImplementationSync<PersistedExternalDataExpressionNode, IExternalDataExpression>().For<ExternalDataExpressionNode>();
		Services.AddImplementationSync<PersistedValueExpressionNode, IValueExpression>().For<ValueExpressionNode>();
		Services.AddImplementationSync<PersistedLocationExpressionNode, ILocationExpression>().For<LocationExpressionNode>();
		Services.AddImplementationSync<PersistedConditionExpressionNode, IConditionExpression>().For<ConditionExpressionNode>();
		Services.AddImplementationSync<PersistedContentNode, IContent>().For<ContentNode>();
		Services.AddImplementationSync<PersistedFinalizeNode, IFinalize>().For<FinalizeNode>();
		Services.AddImplementationSync<PersistedIdentifierNode, IIdentifier>().For<IdentifierNode>();
		Services.AddImplementationSync<PersistedEventNode, IOutgoingEvent>().For<EventNode>();
		Services.AddImplementationSync<PersistedEventDescriptorNode, IEventDescriptor>().For<EventDescriptorNode>();
	}
}