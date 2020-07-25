namespace Xtate.Builder
{
	public interface IParallelBuilder
	{
		IParallel Build();

		void SetId(IIdentifier id);
		void AddState(IState state);
		void AddParallel(IParallel parallel);
		void AddHistory(IHistory history);
		void AddTransition(ITransition transition);
		void AddOnEntry(IOnEntry onEntry);
		void AddOnExit(IOnExit onExit);
		void AddInvoke(IInvoke invoke);
		void SetDataModel(IDataModel dataModel);
	}
}