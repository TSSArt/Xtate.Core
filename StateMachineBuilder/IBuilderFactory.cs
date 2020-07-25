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