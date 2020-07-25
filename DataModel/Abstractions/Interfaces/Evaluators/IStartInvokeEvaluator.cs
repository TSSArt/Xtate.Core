using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public interface IStartInvokeEvaluator
	{
		ValueTask<InvokeId> Start(IIdentifier stateId, IExecutionContext executionContext, CancellationToken token);
	}
}