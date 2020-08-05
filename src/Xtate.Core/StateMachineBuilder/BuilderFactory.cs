#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using Xtate.Annotations;

namespace Xtate.Builder
{
	[PublicAPI]
	public class BuilderFactory : IBuilderFactory
	{
		public static readonly IBuilderFactory Instance = new BuilderFactory(DefaultErrorProcessor.Instance);

		private readonly IErrorProcessor _errorProcessor;

		public BuilderFactory(IErrorProcessor errorProcessor) => _errorProcessor = errorProcessor ?? throw new ArgumentNullException(nameof(errorProcessor));

	#region Interface IBuilderFactory

		public virtual IStateMachineBuilder CreateStateMachineBuilder(object? ancestor) => new StateMachineBuilder(_errorProcessor, ancestor);
		public virtual IStateBuilder        CreateStateBuilder(object? ancestor)        => new StateBuilder(_errorProcessor, ancestor);
		public virtual IParallelBuilder     CreateParallelBuilder(object? ancestor)     => new ParallelBuilder(_errorProcessor, ancestor);
		public virtual IHistoryBuilder      CreateHistoryBuilder(object? ancestor)      => new HistoryBuilder(_errorProcessor, ancestor);
		public virtual IInitialBuilder      CreateInitialBuilder(object? ancestor)      => new InitialBuilder(_errorProcessor, ancestor);
		public virtual IFinalBuilder        CreateFinalBuilder(object? ancestor)        => new FinalBuilder(_errorProcessor, ancestor);
		public virtual ITransitionBuilder   CreateTransitionBuilder(object? ancestor)   => new TransitionBuilder(_errorProcessor, ancestor);
		public virtual ILogBuilder          CreateLogBuilder(object? ancestor)          => new LogBuilder(_errorProcessor, ancestor);
		public virtual ISendBuilder         CreateSendBuilder(object? ancestor)         => new SendBuilder(_errorProcessor, ancestor);
		public virtual IParamBuilder        CreateParamBuilder(object? ancestor)        => new ParamBuilder(_errorProcessor, ancestor);
		public virtual IContentBuilder      CreateContentBuilder(object? ancestor)      => new ContentBuilder(_errorProcessor, ancestor);
		public virtual IOnEntryBuilder      CreateOnEntryBuilder(object? ancestor)      => new OnEntryBuilder(_errorProcessor, ancestor);
		public virtual IOnExitBuilder       CreateOnExitBuilder(object? ancestor)       => new OnExitBuilder(_errorProcessor, ancestor);
		public virtual IInvokeBuilder       CreateInvokeBuilder(object? ancestor)       => new InvokeBuilder(_errorProcessor, ancestor);
		public virtual IFinalizeBuilder     CreateFinalizeBuilder(object? ancestor)     => new FinalizeBuilder(_errorProcessor, ancestor);
		public virtual IScriptBuilder       CreateScriptBuilder(object? ancestor)       => new ScriptBuilder(_errorProcessor, ancestor);
		public virtual ICustomActionBuilder CreateCustomActionBuilder(object? ancestor) => new CustomActionBuilder(_errorProcessor, ancestor);
		public virtual IDataModelBuilder    CreateDataModelBuilder(object? ancestor)    => new DataModelBuilder(_errorProcessor, ancestor);
		public virtual IDataBuilder         CreateDataBuilder(object? ancestor)         => new DataBuilder(_errorProcessor, ancestor);
		public virtual IDoneDataBuilder     CreateDoneDataBuilder(object? ancestor)     => new DoneDataBuilder(_errorProcessor, ancestor);
		public virtual IAssignBuilder       CreateAssignBuilder(object? ancestor)       => new AssignBuilder(_errorProcessor, ancestor);
		public virtual IRaiseBuilder        CreateRaiseBuilder(object? ancestor)        => new RaiseBuilder(_errorProcessor, ancestor);
		public virtual ICancelBuilder       CreateCancelBuilder(object? ancestor)       => new CancelBuilder(_errorProcessor, ancestor);
		public virtual IForEachBuilder      CreateForEachBuilder(object? ancestor)      => new ForEachBuilder(_errorProcessor, ancestor);
		public virtual IIfBuilder           CreateIfBuilder(object? ancestor)           => new IfBuilder(_errorProcessor, ancestor);
		public virtual IElseBuilder         CreateElseBuilder(object? ancestor)         => new ElseBuilder(_errorProcessor, ancestor);
		public virtual IElseIfBuilder       CreateElseIfBuilder(object? ancestor)       => new ElseIfBuilder(_errorProcessor, ancestor);

	#endregion
	}
}