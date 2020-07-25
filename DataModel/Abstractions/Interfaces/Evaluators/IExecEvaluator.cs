using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public interface IExecEvaluator
	{
		ValueTask Execute(IExecutionContext executionContext, CancellationToken token);
	}
}