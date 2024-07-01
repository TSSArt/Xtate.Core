<<<<<<< Updated upstream
﻿using Xtate.Core;
using Xtate.IoC;

namespace Xtate.Builder
{
	public static class StateMachineExtensions
	{
		public static void RegisterStateMachineBuilder(this IServiceCollection services)
		{
			if (services.IsRegistered<IStateMachineBuilder>())
			{
				return;
			}

			services.RegisterErrorProcessor();

			services.AddImplementationSync<StateMachineBuilder>().For<IStateMachineBuilder>();
			services.AddImplementationSync<StateBuilder>().For<IStateBuilder>();
			services.AddImplementationSync<ParallelBuilder>().For<IParallelBuilder>();
			services.AddImplementationSync<HistoryBuilder>().For<IHistoryBuilder>();
			services.AddImplementationSync<InitialBuilder>().For<IInitialBuilder>();
			services.AddImplementationSync<FinalBuilder>().For<IFinalBuilder>();
			services.AddImplementationSync<TransitionBuilder>().For<ITransitionBuilder>();
			services.AddImplementationSync<LogBuilder>().For<ILogBuilder>();
			services.AddImplementationSync<SendBuilder>().For<ISendBuilder>();
			services.AddImplementationSync<ParamBuilder>().For<IParamBuilder>();
			services.AddImplementationSync<ContentBuilder>().For<IContentBuilder>();
			services.AddImplementationSync<OnEntryBuilder>().For<IOnEntryBuilder>();
			services.AddImplementationSync<OnExitBuilder>().For<IOnExitBuilder>();
			services.AddImplementationSync<InvokeBuilder>().For<IInvokeBuilder>();
			services.AddImplementationSync<FinalizeBuilder>().For<IFinalizeBuilder>();
			services.AddImplementationSync<ScriptBuilder>().For<IScriptBuilder>();
			services.AddImplementationSync<DataModelBuilder>().For<IDataModelBuilder>();
			services.AddImplementationSync<DataBuilder>().For<IDataBuilder>();
			services.AddImplementationSync<DoneDataBuilder>().For<IDoneDataBuilder>();
			services.AddImplementationSync<ForEachBuilder>().For<IForEachBuilder>();
			services.AddImplementationSync<IfBuilder>().For<IIfBuilder>();
			services.AddImplementationSync<ElseBuilder>().For<IElseBuilder>();
			services.AddImplementationSync<ElseIfBuilder>().For<IElseIfBuilder>();
			services.AddImplementationSync<RaiseBuilder>().For<IRaiseBuilder>();
			services.AddImplementationSync<AssignBuilder>().For<IAssignBuilder>();
			services.AddImplementationSync<CancelBuilder>().For<ICancelBuilder>();
			services.AddImplementationSync<CustomActionBuilder>().For<ICustomActionBuilder>();

			services.AddImplementationSync<StateMachineBuilder, object?>().For<IStateMachineBuilder>();
			services.AddImplementationSync<StateBuilder, object?>().For<IStateBuilder>();
			services.AddImplementationSync<ParallelBuilder, object?>().For<IParallelBuilder>();
			services.AddImplementationSync<HistoryBuilder, object?>().For<IHistoryBuilder>();
			services.AddImplementationSync<InitialBuilder, object?>().For<IInitialBuilder>();
			services.AddImplementationSync<FinalBuilder, object?>().For<IFinalBuilder>();
			services.AddImplementationSync<TransitionBuilder, object?>().For<ITransitionBuilder>();
			services.AddImplementationSync<LogBuilder, object?>().For<ILogBuilder>();
			services.AddImplementationSync<SendBuilder, object?>().For<ISendBuilder>();
			services.AddImplementationSync<ParamBuilder, object?>().For<IParamBuilder>();
			services.AddImplementationSync<ContentBuilder, object?>().For<IContentBuilder>();
			services.AddImplementationSync<OnEntryBuilder, object?>().For<IOnEntryBuilder>();
			services.AddImplementationSync<OnExitBuilder, object?>().For<IOnExitBuilder>();
			services.AddImplementationSync<InvokeBuilder, object?>().For<IInvokeBuilder>();
			services.AddImplementationSync<FinalizeBuilder, object?>().For<IFinalizeBuilder>();
			services.AddImplementationSync<ScriptBuilder, object?>().For<IScriptBuilder>();
			services.AddImplementationSync<DataModelBuilder, object?>().For<IDataModelBuilder>();
			services.AddImplementationSync<DataBuilder, object?>().For<IDataBuilder>();
			services.AddImplementationSync<DoneDataBuilder, object?>().For<IDoneDataBuilder>();
			services.AddImplementationSync<ForEachBuilder, object?>().For<IForEachBuilder>();
			services.AddImplementationSync<IfBuilder, object?>().For<IIfBuilder>();
			services.AddImplementationSync<ElseBuilder, object?>().For<IElseBuilder>();
			services.AddImplementationSync<ElseIfBuilder, object?>().For<IElseIfBuilder>();
			services.AddImplementationSync<RaiseBuilder, object?>().For<IRaiseBuilder>();
			services.AddImplementationSync<AssignBuilder, object?>().For<IAssignBuilder>();
			services.AddImplementationSync<CancelBuilder, object?>().For<ICancelBuilder>();
			services.AddImplementationSync<CustomActionBuilder, object?>().For<ICustomActionBuilder>();
		}
	}
}
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using Xtate.IoC;

namespace Xtate.Builder;

public static class StateMachineExtensions
{
	public static void RegisterStateMachineBuilder(this IServiceCollection services)
	{
		if (services.IsRegistered<IStateMachineBuilder>())
		{
			return;
		}

		services.RegisterErrorProcessor();

		services.AddImplementationSync<StateMachineBuilder>().For<IStateMachineBuilder>();
		services.AddImplementationSync<StateBuilder>().For<IStateBuilder>();
		services.AddImplementationSync<ParallelBuilder>().For<IParallelBuilder>();
		services.AddImplementationSync<HistoryBuilder>().For<IHistoryBuilder>();
		services.AddImplementationSync<InitialBuilder>().For<IInitialBuilder>();
		services.AddImplementationSync<FinalBuilder>().For<IFinalBuilder>();
		services.AddImplementationSync<TransitionBuilder>().For<ITransitionBuilder>();
		services.AddImplementationSync<LogBuilder>().For<ILogBuilder>();
		services.AddImplementationSync<SendBuilder>().For<ISendBuilder>();
		services.AddImplementationSync<ParamBuilder>().For<IParamBuilder>();
		services.AddImplementationSync<ContentBuilder>().For<IContentBuilder>();
		services.AddImplementationSync<OnEntryBuilder>().For<IOnEntryBuilder>();
		services.AddImplementationSync<OnExitBuilder>().For<IOnExitBuilder>();
		services.AddImplementationSync<InvokeBuilder>().For<IInvokeBuilder>();
		services.AddImplementationSync<FinalizeBuilder>().For<IFinalizeBuilder>();
		services.AddImplementationSync<ScriptBuilder>().For<IScriptBuilder>();
		services.AddImplementationSync<DataModelBuilder>().For<IDataModelBuilder>();
		services.AddImplementationSync<DataBuilder>().For<IDataBuilder>();
		services.AddImplementationSync<DoneDataBuilder>().For<IDoneDataBuilder>();
		services.AddImplementationSync<ForEachBuilder>().For<IForEachBuilder>();
		services.AddImplementationSync<IfBuilder>().For<IIfBuilder>();
		services.AddImplementationSync<ElseBuilder>().For<IElseBuilder>();
		services.AddImplementationSync<ElseIfBuilder>().For<IElseIfBuilder>();
		services.AddImplementationSync<RaiseBuilder>().For<IRaiseBuilder>();
		services.AddImplementationSync<AssignBuilder>().For<IAssignBuilder>();
		services.AddImplementationSync<CancelBuilder>().For<ICancelBuilder>();
		services.AddImplementationSync<CustomActionBuilder>().For<ICustomActionBuilder>();

		services.AddImplementationSync<StateMachineBuilder, object?>().For<IStateMachineBuilder>();
		services.AddImplementationSync<StateBuilder, object?>().For<IStateBuilder>();
		services.AddImplementationSync<ParallelBuilder, object?>().For<IParallelBuilder>();
		services.AddImplementationSync<HistoryBuilder, object?>().For<IHistoryBuilder>();
		services.AddImplementationSync<InitialBuilder, object?>().For<IInitialBuilder>();
		services.AddImplementationSync<FinalBuilder, object?>().For<IFinalBuilder>();
		services.AddImplementationSync<TransitionBuilder, object?>().For<ITransitionBuilder>();
		services.AddImplementationSync<LogBuilder, object?>().For<ILogBuilder>();
		services.AddImplementationSync<SendBuilder, object?>().For<ISendBuilder>();
		services.AddImplementationSync<ParamBuilder, object?>().For<IParamBuilder>();
		services.AddImplementationSync<ContentBuilder, object?>().For<IContentBuilder>();
		services.AddImplementationSync<OnEntryBuilder, object?>().For<IOnEntryBuilder>();
		services.AddImplementationSync<OnExitBuilder, object?>().For<IOnExitBuilder>();
		services.AddImplementationSync<InvokeBuilder, object?>().For<IInvokeBuilder>();
		services.AddImplementationSync<FinalizeBuilder, object?>().For<IFinalizeBuilder>();
		services.AddImplementationSync<ScriptBuilder, object?>().For<IScriptBuilder>();
		services.AddImplementationSync<DataModelBuilder, object?>().For<IDataModelBuilder>();
		services.AddImplementationSync<DataBuilder, object?>().For<IDataBuilder>();
		services.AddImplementationSync<DoneDataBuilder, object?>().For<IDoneDataBuilder>();
		services.AddImplementationSync<ForEachBuilder, object?>().For<IForEachBuilder>();
		services.AddImplementationSync<IfBuilder, object?>().For<IIfBuilder>();
		services.AddImplementationSync<ElseBuilder, object?>().For<IElseBuilder>();
		services.AddImplementationSync<ElseIfBuilder, object?>().For<IElseIfBuilder>();
		services.AddImplementationSync<RaiseBuilder, object?>().For<IRaiseBuilder>();
		services.AddImplementationSync<AssignBuilder, object?>().For<IAssignBuilder>();
		services.AddImplementationSync<CancelBuilder, object?>().For<ICancelBuilder>();
		services.AddImplementationSync<CustomActionBuilder, object?>().For<ICustomActionBuilder>();
	}
}
>>>>>>> Stashed changes
