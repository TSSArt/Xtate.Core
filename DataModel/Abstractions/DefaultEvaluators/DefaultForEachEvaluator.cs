using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultForEachEvaluator : IForEach, IExecEvaluator, IAncestorProvider
	{
		private readonly ForEachEntity _forEach;

		public DefaultForEachEvaluator(in ForEachEntity forEach)
		{
			_forEach = forEach;

			Infrastructure.Assert(forEach.Array != null);
			Infrastructure.Assert(forEach.Item != null);

			ArrayEvaluator = forEach.Array.As<IArrayEvaluator>();
			ItemEvaluator = forEach.Item.As<ILocationEvaluator>();
			IndexEvaluator = forEach.Index?.As<ILocationEvaluator>();
			ActionEvaluatorList = forEach.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>();
		}

		public IArrayEvaluator                ArrayEvaluator      { get; }
		public ILocationEvaluator             ItemEvaluator       { get; }
		public ILocationEvaluator?            IndexEvaluator      { get; }
		public ImmutableArray<IExecEvaluator> ActionEvaluatorList { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _forEach.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var array = await ArrayEvaluator.EvaluateArray(executionContext, token).ConfigureAwait(false);

			for (var i = 0; i < array.Length; i ++)
			{
				var instance = array[i];

				await ItemEvaluator.SetValue(instance, executionContext, token).ConfigureAwait(false);

				if (IndexEvaluator != null)
				{
					await IndexEvaluator.SetValue(new DefaultObject(i), executionContext, token).ConfigureAwait(false);
				}

				foreach (var execEvaluator in ActionEvaluatorList)
				{
					await execEvaluator.Execute(executionContext, token).ConfigureAwait(false);
				}
			}
		}

	#endregion

	#region Interface IForEach

		public IValueExpression                  Array  => _forEach.Array!;
		public ILocationExpression               Item   => _forEach.Item!;
		public ILocationExpression?              Index  => _forEach.Index;
		public ImmutableArray<IExecutableEntity> Action => _forEach.Action;

	#endregion
	}
}