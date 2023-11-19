using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.IoC;

namespace Xtate.Core;

public class DataModelHandlerGetter
{
	public required IDataModelHandlerService DataModelHandlerService { private get; init; }
	public required IStateMachine?           StateMachine            { private get; init; }

	public virtual async ValueTask<IDataModelHandler?> GetDataModelHandler() =>
		StateMachine is not null ? await DataModelHandlerService.GetDataModelHandler(StateMachine.DataModelType).ConfigureAwait(false) : default;
}