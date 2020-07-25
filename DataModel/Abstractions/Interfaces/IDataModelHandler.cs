using System.Collections.Immutable;

namespace Xtate.DataModel
{
	public interface IDataModelHandler
	{
		bool CaseInsensitive { get; }

		void Process(ref IExecutableEntity executableEntity);

		void Process(ref IDataModel dataModel);

		void Process(ref IDoneData doneData);

		void Process(ref IInvoke invoke);

		void ExecutionContextCreated(IExecutionContext executionContext, out ImmutableDictionary<string, string> dataModelVars);
	}
}