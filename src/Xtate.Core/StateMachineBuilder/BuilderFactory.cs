#region Copyright © 2019-2021 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/
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
// along with this program.  If not, see <https://www.gnu.org/licenses/.

#endregion

using System;
using System.Threading.Tasks;

namespace Xtate.Builder
{
	public class BuilderFactory : IBuilderFactory
	{
		public required Func<object?, StateMachineBuilder> StateMachineBuilderFactory { private get; init; }
		public required Func<object?, StateBuilder>        StateBuilderFactory        { private get; init; }
		public required Func<object?, ParallelBuilder>     ParallelBuilderFactory     { private get; init; }
		public required Func<object?, HistoryBuilder>      HistoryBuilderFactory      { private get; init; }
		public required Func<object?, InitialBuilder>      InitialBuilderFactory      { private get; init; }
		public required Func<object?, FinalBuilder>        FinalBuilderFactory        { private get; init; }
		public required Func<object?, TransitionBuilder>   TransitionBuilderFactory   { private get; init; }
		public required Func<object?, LogBuilder>          LogBuilderFactory          { private get; init; }
		public required Func<object?, SendBuilder>         SendBuilderFactory         { private get; init; }
		public required Func<object?, ParamBuilder>        ParamBuilderFactory        { private get; init; }
		public required Func<object?, ContentBuilder>      ContentBuilderFactory      { private get; init; }
		public required Func<object?, OnEntryBuilder>      OnEntryBuilderFactory      { private get; init; }
		public required Func<object?, OnExitBuilder>       OnExitBuilderFactory       { private get; init; }
		public required Func<object?, InvokeBuilder>       InvokeBuilderFactory       { private get; init; }
		public required Func<object?, FinalizeBuilder>     FinalizeBuilderFactory     { private get; init; }
		public required Func<object?, ScriptBuilder>       ScriptBuilderFactory       { private get; init; }
		public required Func<object?, CustomActionBuilder> CustomActionBuilderFactory { private get; init; }
		public required Func<object?, DataModelBuilder>    DataModelBuilderFactory    { private get; init; }
		public required Func<object?, DataBuilder>         DataBuilderFactory         { private get; init; }
		public required Func<object?, DoneDataBuilder>     DoneDataBuilderFactory     { private get; init; }
		public required Func<object?, AssignBuilder>       AssignBuilderFactory       { private get; init; }
		public required Func<object?, RaiseBuilder>        RaiseBuilderFactory        { private get; init; }
		public required Func<object?, CancelBuilder>       CancelBuilderFactory       { private get; init; }
		public required Func<object?, ForEachBuilder>      ForEachBuilderFactory      { private get; init; }
		public required Func<object?, IfBuilder>           IfBuilderFactory           { private get; init; }
		public required Func<object?, ElseBuilder>         ElseBuilderFactory         { private get; init; }
		public required Func<object?, ElseIfBuilder>       ElseIfBuilderFactory       { private get; init; }

	#region Interface IBuilderFactory

		public virtual IStateMachineBuilder CreateStateMachineBuilder(object? ancestor) => StateMachineBuilderFactory(ancestor);
		public virtual IStateBuilder        CreateStateBuilder(object? ancestor)        => StateBuilderFactory(ancestor);
		public virtual IParallelBuilder     CreateParallelBuilder(object? ancestor)     => ParallelBuilderFactory(ancestor);
		public virtual IHistoryBuilder      CreateHistoryBuilder(object? ancestor)      => HistoryBuilderFactory(ancestor);
		public virtual IInitialBuilder      CreateInitialBuilder(object? ancestor)      => InitialBuilderFactory(ancestor);
		public virtual IFinalBuilder        CreateFinalBuilder(object? ancestor)        => FinalBuilderFactory(ancestor);
		public virtual ITransitionBuilder   CreateTransitionBuilder(object? ancestor)   => TransitionBuilderFactory(ancestor);
		public virtual ILogBuilder          CreateLogBuilder(object? ancestor)          => LogBuilderFactory(ancestor);
		public virtual ISendBuilder         CreateSendBuilder(object? ancestor)         => SendBuilderFactory(ancestor);
		public virtual IParamBuilder        CreateParamBuilder(object? ancestor)        => ParamBuilderFactory(ancestor);
		public virtual IContentBuilder      CreateContentBuilder(object? ancestor)      => ContentBuilderFactory(ancestor);
		public virtual IOnEntryBuilder      CreateOnEntryBuilder(object? ancestor)      => OnEntryBuilderFactory(ancestor);
		public virtual IOnExitBuilder       CreateOnExitBuilder(object? ancestor)       => OnExitBuilderFactory(ancestor);
		public virtual IInvokeBuilder       CreateInvokeBuilder(object? ancestor)       => InvokeBuilderFactory(ancestor);
		public virtual IFinalizeBuilder     CreateFinalizeBuilder(object? ancestor)     => FinalizeBuilderFactory(ancestor);
		public virtual IScriptBuilder       CreateScriptBuilder(object? ancestor)       => ScriptBuilderFactory(ancestor);
		public virtual ICustomActionBuilder CreateCustomActionBuilder(object? ancestor) => CustomActionBuilderFactory(ancestor);
		public virtual IDataModelBuilder    CreateDataModelBuilder(object? ancestor)    => DataModelBuilderFactory(ancestor);
		public virtual IDataBuilder         CreateDataBuilder(object? ancestor)         => DataBuilderFactory(ancestor);
		public virtual IDoneDataBuilder     CreateDoneDataBuilder(object? ancestor)     => DoneDataBuilderFactory(ancestor);
		public virtual IAssignBuilder       CreateAssignBuilder(object? ancestor)       => AssignBuilderFactory(ancestor);
		public virtual IRaiseBuilder        CreateRaiseBuilder(object? ancestor)        => RaiseBuilderFactory(ancestor);
		public virtual ICancelBuilder       CreateCancelBuilder(object? ancestor)       => CancelBuilderFactory(ancestor);
		public virtual IForEachBuilder      CreateForEachBuilder(object? ancestor)      => ForEachBuilderFactory(ancestor);
		public virtual IIfBuilder           CreateIfBuilder(object? ancestor)           => IfBuilderFactory(ancestor);
		public virtual IElseBuilder         CreateElseBuilder(object? ancestor)         => ElseBuilderFactory(ancestor);
		public virtual IElseIfBuilder       CreateElseIfBuilder(object? ancestor)       => ElseIfBuilderFactory(ancestor);

	#endregion
	}
}