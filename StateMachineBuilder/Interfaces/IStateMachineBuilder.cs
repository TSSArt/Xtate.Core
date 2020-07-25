using System.Collections.Immutable;

namespace Xtate.Builder
{
	public interface IStateMachineBuilder
	{
		IStateMachine Build();

		void SetInitial(ImmutableArray<IIdentifier> initial);
		void AddState(IState state);
		void AddParallel(IParallel parallel);
		void AddFinal(IFinal final);
		void SetDataModel(IDataModel dataModel);
		void SetScript(IScript script);
		void SetName(string name);
		void SetDataModelType(string dataModelType);
		void SetBindingType(BindingType bindingType);
		void SetPersistenceLevel(PersistenceLevel persistenceLevel);
		void SetSynchronousEventProcessing(bool value);
		void SetExternalQueueSize(int size);
		void SetUnhandledErrorBehaviour(UnhandledErrorBehaviour unhandledErrorBehaviour);
	}
}