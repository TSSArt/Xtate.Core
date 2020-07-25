using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultDoneDataEvaluator : IObjectEvaluator, IDoneData, IAncestorProvider
	{
		private readonly IValueEvaluator?             _contentBodyEvaluator;
		private readonly IObjectEvaluator?            _contentExpressionEvaluator;
		private readonly DoneDataEntity               _doneData;
		private readonly ImmutableArray<DefaultParam> _parameterList;

		public DefaultDoneDataEvaluator(in DoneDataEntity doneData)
		{
			_doneData = doneData;

			_contentExpressionEvaluator = doneData.Content?.Expression?.As<IObjectEvaluator>();
			_contentBodyEvaluator = doneData.Content?.Body?.As<IValueEvaluator>();
			_parameterList = doneData.Parameters.AsArrayOf<IParam, DefaultParam>();
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _doneData.Ancestor;

	#endregion

	#region Interface IDoneData

		public IContent? Content => _doneData.Content;

		public ImmutableArray<IParam> Parameters => _doneData.Parameters;

	#endregion

	#region Interface IObjectEvaluator

		public async ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			return await DataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, nameEvaluatorList: default, _parameterList, executionContext, token).ConfigureAwait(false);
		}

	#endregion
	}
}