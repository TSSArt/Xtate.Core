using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultCustomActionEvaluator : ICustomAction, IExecEvaluator, IAncestorProvider
	{
		private readonly CustomActionEntity            _customAction;
		private readonly ICustomActionDispatcher _customActionDispatcher;

		public DefaultCustomActionEvaluator(in CustomActionEntity customAction)
		{
			_customAction = customAction;

			var customActionDispatcher = customAction.Ancestor?.As<ICustomActionDispatcher>();

			Infrastructure.Assert(customActionDispatcher != null, Resources.Assertion_CustomActionDoesNotConfigured);

			var locations = customAction.Locations.AsArrayOf<ILocationExpression, ILocationEvaluator>(true);
			var values = customAction.Values.AsArrayOf<IValueExpression, IObjectEvaluator>(true);

			customActionDispatcher.SetEvaluators(locations, values);

			_customActionDispatcher = customActionDispatcher;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _customAction.Ancestor;

	#endregion

	#region Interface ICustomAction

		public string? Xml => _customAction.Xml;

		public ImmutableArray<ILocationExpression> Locations => _customAction.Locations;

		public ImmutableArray<IValueExpression> Values => _customAction.Values;

	#endregion

	#region Interface IExecEvaluator

		public virtual ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			return _customActionDispatcher.Execute(executionContext, token);
		}

	#endregion
	}
}