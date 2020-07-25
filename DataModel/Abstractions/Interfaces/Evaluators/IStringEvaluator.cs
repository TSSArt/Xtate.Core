using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public interface IStringEvaluator : IValueEvaluator
	{
		ValueTask<string> EvaluateString(IExecutionContext executionContext, CancellationToken token);
	}
}