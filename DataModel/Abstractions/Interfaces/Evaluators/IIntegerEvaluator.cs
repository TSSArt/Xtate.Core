using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public interface IIntegerEvaluator : IValueEvaluator
	{
		ValueTask<int> EvaluateInteger(IExecutionContext executionContext, CancellationToken token);
	}
}