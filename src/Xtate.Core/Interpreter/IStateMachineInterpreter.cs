using System.Threading.Tasks;

namespace Xtate.Core;

public interface IStateMachineInterpreter
{
	ValueTask<DataModelValue> RunAsync();
}