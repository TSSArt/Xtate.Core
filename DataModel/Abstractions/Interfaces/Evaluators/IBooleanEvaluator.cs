using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public interface IBooleanEvaluator : IValueEvaluator
	{
		ValueTask<bool> EvaluateBoolean(IExecutionContext executionContext, CancellationToken token);
	}
}