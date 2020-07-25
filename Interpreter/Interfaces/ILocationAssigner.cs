using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public interface ILocationAssigner
	{
		ValueTask Assign(IExecutionContext executionContext, DataModelValue value, CancellationToken token);
	}
}