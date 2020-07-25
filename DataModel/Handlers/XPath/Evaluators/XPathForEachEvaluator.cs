using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.XPath
{
	internal class XPathForEachEvaluator : DefaultForEachEvaluator
	{
		public XPathForEachEvaluator(in ForEachEntity forEach) : base(forEach) { }

		public override async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			var engine = executionContext.Engine();

			engine.EnterScope();

			try
			{
				ItemEvaluator.DeclareLocalVariable(executionContext);
				IndexEvaluator?.DeclareLocalVariable(executionContext);

				await base.Execute(executionContext, token).ConfigureAwait(false);
			}
			finally
			{
				engine.LeaveScope();
			}
		}
	}
}