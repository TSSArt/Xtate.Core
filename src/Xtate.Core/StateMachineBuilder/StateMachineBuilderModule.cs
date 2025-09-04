// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.IoC;

namespace Xtate.Builder;

public class StateMachineBuilderModule : Module<ErrorProcessorModule>
{
    protected override void AddServices()
    {
        Services.AddImplementationSync<StateMachineBuilder>().For<IStateMachineBuilder>();
        Services.AddImplementationSync<StateBuilder>().For<IStateBuilder>();
        Services.AddImplementationSync<ParallelBuilder>().For<IParallelBuilder>();
        Services.AddImplementationSync<HistoryBuilder>().For<IHistoryBuilder>();
        Services.AddImplementationSync<InitialBuilder>().For<IInitialBuilder>();
        Services.AddImplementationSync<FinalBuilder>().For<IFinalBuilder>();
        Services.AddImplementationSync<TransitionBuilder>().For<ITransitionBuilder>();
        Services.AddImplementationSync<LogBuilder>().For<ILogBuilder>();
        Services.AddImplementationSync<SendBuilder>().For<ISendBuilder>();
        Services.AddImplementationSync<ParamBuilder>().For<IParamBuilder>();
        Services.AddImplementationSync<ContentBuilder>().For<IContentBuilder>();
        Services.AddImplementationSync<OnEntryBuilder>().For<IOnEntryBuilder>();
        Services.AddImplementationSync<OnExitBuilder>().For<IOnExitBuilder>();
        Services.AddImplementationSync<InvokeBuilder>().For<IInvokeBuilder>();
        Services.AddImplementationSync<FinalizeBuilder>().For<IFinalizeBuilder>();
        Services.AddImplementationSync<ScriptBuilder>().For<IScriptBuilder>();
        Services.AddImplementationSync<DataModelBuilder>().For<IDataModelBuilder>();
        Services.AddImplementationSync<DataBuilder>().For<IDataBuilder>();
        Services.AddImplementationSync<DoneDataBuilder>().For<IDoneDataBuilder>();
        Services.AddImplementationSync<ForEachBuilder>().For<IForEachBuilder>();
        Services.AddImplementationSync<IfBuilder>().For<IIfBuilder>();
        Services.AddImplementationSync<ElseBuilder>().For<IElseBuilder>();
        Services.AddImplementationSync<ElseIfBuilder>().For<IElseIfBuilder>();
        Services.AddImplementationSync<RaiseBuilder>().For<IRaiseBuilder>();
        Services.AddImplementationSync<AssignBuilder>().For<IAssignBuilder>();
        Services.AddImplementationSync<CancelBuilder>().For<ICancelBuilder>();
        Services.AddImplementationSync<CustomActionBuilder>().For<ICustomActionBuilder>();

        Services.AddImplementationSync<StateMachineBuilder, object?>().For<IStateMachineBuilder>();
        Services.AddImplementationSync<StateBuilder, object?>().For<IStateBuilder>();
        Services.AddImplementationSync<ParallelBuilder, object?>().For<IParallelBuilder>();
        Services.AddImplementationSync<HistoryBuilder, object?>().For<IHistoryBuilder>();
        Services.AddImplementationSync<InitialBuilder, object?>().For<IInitialBuilder>();
        Services.AddImplementationSync<FinalBuilder, object?>().For<IFinalBuilder>();
        Services.AddImplementationSync<TransitionBuilder, object?>().For<ITransitionBuilder>();
        Services.AddImplementationSync<LogBuilder, object?>().For<ILogBuilder>();
        Services.AddImplementationSync<SendBuilder, object?>().For<ISendBuilder>();
        Services.AddImplementationSync<ParamBuilder, object?>().For<IParamBuilder>();
        Services.AddImplementationSync<ContentBuilder, object?>().For<IContentBuilder>();
        Services.AddImplementationSync<OnEntryBuilder, object?>().For<IOnEntryBuilder>();
        Services.AddImplementationSync<OnExitBuilder, object?>().For<IOnExitBuilder>();
        Services.AddImplementationSync<InvokeBuilder, object?>().For<IInvokeBuilder>();
        Services.AddImplementationSync<FinalizeBuilder, object?>().For<IFinalizeBuilder>();
        Services.AddImplementationSync<ScriptBuilder, object?>().For<IScriptBuilder>();
        Services.AddImplementationSync<DataModelBuilder, object?>().For<IDataModelBuilder>();
        Services.AddImplementationSync<DataBuilder, object?>().For<IDataBuilder>();
        Services.AddImplementationSync<DoneDataBuilder, object?>().For<IDoneDataBuilder>();
        Services.AddImplementationSync<ForEachBuilder, object?>().For<IForEachBuilder>();
        Services.AddImplementationSync<IfBuilder, object?>().For<IIfBuilder>();
        Services.AddImplementationSync<ElseBuilder, object?>().For<IElseBuilder>();
        Services.AddImplementationSync<ElseIfBuilder, object?>().For<IElseIfBuilder>();
        Services.AddImplementationSync<RaiseBuilder, object?>().For<IRaiseBuilder>();
        Services.AddImplementationSync<AssignBuilder, object?>().For<IAssignBuilder>();
        Services.AddImplementationSync<CancelBuilder, object?>().For<ICancelBuilder>();
        Services.AddImplementationSync<CustomActionBuilder, object?>().For<ICustomActionBuilder>();
    }
}