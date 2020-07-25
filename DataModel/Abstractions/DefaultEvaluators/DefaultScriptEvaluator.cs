using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultScriptEvaluator : IScript, IExecEvaluator, IAncestorProvider
	{
		private readonly ScriptEntity _script;

		public DefaultScriptEvaluator(in ScriptEntity script)
		{
			_script = script;

			Infrastructure.Assert(script.Content != null || script.Source != null);

			ContentEvaluator = script.Content?.As<IExecEvaluator>();
			SourceEvaluator = script.Source?.As<IExecEvaluator>();
		}

		public IExecEvaluator? ContentEvaluator { get; }
		public IExecEvaluator? SourceEvaluator  { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _script.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var evaluator = ContentEvaluator ?? SourceEvaluator;
			return evaluator!.Execute(executionContext, token);
		}

	#endregion

	#region Interface IScript

		public IScriptExpression? Content => _script.Content;

		public IExternalScriptExpression? Source => _script.Source;

	#endregion
	}
}