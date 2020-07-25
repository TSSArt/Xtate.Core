using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultAssignEvaluator : IAssign, IExecEvaluator, IAncestorProvider
	{
		private readonly AssignEntity _assign;

		public DefaultAssignEvaluator(in AssignEntity assign)
		{
			_assign = assign;

			Infrastructure.Assert(assign.Location != null);

			LocationEvaluator = assign.Location.As<ILocationEvaluator>();
			ExpressionEvaluator = assign.Expression?.As<IObjectEvaluator>();
			InlineContentEvaluator = assign.InlineContent?.As<IObjectEvaluator>();
		}

		public ILocationEvaluator LocationEvaluator      { get; }
		public IObjectEvaluator?  ExpressionEvaluator    { get; }
		public IObjectEvaluator?  InlineContentEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _assign.Ancestor;

	#endregion

	#region Interface IAssign

		public ILocationExpression Location      => _assign.Location!;
		public IValueExpression?   Expression    => _assign.Expression;
		public IInlineContent?     InlineContent => _assign.InlineContent;
		public string?             Type          => _assign.Type;
		public string?             Attribute     => _assign.Attribute;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var value = await EvaluateRightValue(executionContext, token).ConfigureAwait(false);

			await LocationEvaluator.SetValue(value, executionContext, token).ConfigureAwait(false);
		}

	#endregion

		protected virtual ValueTask<IObject> EvaluateRightValue(IExecutionContext executionContext, CancellationToken token)
		{
			if (ExpressionEvaluator != null)
			{
				return ExpressionEvaluator.EvaluateObject(executionContext, token);
			}

			if (InlineContentEvaluator != null)
			{
				return InlineContentEvaluator.EvaluateObject(executionContext, token);
			}

			return new ValueTask<IObject>(DefaultObject.Null);
		}
	}
}