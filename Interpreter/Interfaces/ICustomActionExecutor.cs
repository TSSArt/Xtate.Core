using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.CustomAction
{
	[PublicAPI]
	public interface ICustomActionExecutor
	{
		ValueTask Execute(IExecutionContext executionContext, CancellationToken token);
	}
}