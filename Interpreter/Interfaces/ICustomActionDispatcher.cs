using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.DataModel;

namespace Xtate
{
	internal interface ICustomActionDispatcher
	{
		void SetEvaluators(ImmutableArray<ILocationEvaluator> locations, ImmutableArray<IObjectEvaluator> values);

		ValueTask Execute(IExecutionContext executionContext, CancellationToken token);
	}
}