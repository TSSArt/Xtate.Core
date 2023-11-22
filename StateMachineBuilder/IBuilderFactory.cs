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

using System.Threading.Tasks;

namespace Xtate.Builder
{
	public interface IBuilderFactory
	{
		IStateMachineBuilder CreateStateMachineBuilder(object? ancestor);
		IStateBuilder        CreateStateBuilder(object? ancestor);
		IParallelBuilder     CreateParallelBuilder(object? ancestor);
		IHistoryBuilder      CreateHistoryBuilder(object? ancestor);
		IInitialBuilder      CreateInitialBuilder(object? ancestor);
		IFinalBuilder        CreateFinalBuilder(object? ancestor);
		ITransitionBuilder   CreateTransitionBuilder(object? ancestor);
		ILogBuilder          CreateLogBuilder(object? ancestor);
		ISendBuilder         CreateSendBuilder(object? ancestor);
		IParamBuilder        CreateParamBuilder(object? ancestor);
		IContentBuilder      CreateContentBuilder(object? ancestor);
		IOnEntryBuilder      CreateOnEntryBuilder(object? ancestor);
		IOnExitBuilder       CreateOnExitBuilder(object? ancestor);
		IInvokeBuilder       CreateInvokeBuilder(object? ancestor);
		IFinalizeBuilder     CreateFinalizeBuilder(object? ancestor);
		IScriptBuilder       CreateScriptBuilder(object? ancestor);
		IDataModelBuilder    CreateDataModelBuilder(object? ancestor);
		IDataBuilder         CreateDataBuilder(object? ancestor);
		IDoneDataBuilder     CreateDoneDataBuilder(object? ancestor);
		IForEachBuilder      CreateForEachBuilder(object? ancestor);
		IIfBuilder           CreateIfBuilder(object? ancestor);
		IElseBuilder         CreateElseBuilder(object? ancestor);
		IElseIfBuilder       CreateElseIfBuilder(object? ancestor);
		IRaiseBuilder        CreateRaiseBuilder(object? ancestor);
		IAssignBuilder       CreateAssignBuilder(object? ancestor);
		ICancelBuilder       CreateCancelBuilder(object? ancestor);
		ICustomActionBuilder CreateCustomActionBuilder(object? ancestor);
	}
}