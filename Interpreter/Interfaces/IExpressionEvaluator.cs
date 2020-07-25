using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public interface IExpressionEvaluator
	{
		ValueTask<DataModelValue> Evaluate(IExecutionContext executionContext, CancellationToken token);
	}
}