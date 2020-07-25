using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultContentBodyEvaluator : IContentBody, IObjectEvaluator, IStringEvaluator, IAncestorProvider
	{
		private readonly ContentBody    _contentBody;
		private          DataModelValue _parsedValue;
		private          Exception?     _parsingException;

		public DefaultContentBodyEvaluator(in ContentBody contentBody)
		{
			Infrastructure.Assert(contentBody.Value != null);

			_contentBody = contentBody;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _contentBody.Ancestor;

	#endregion

	#region Interface IContentBody

		public string Value => _contentBody.Value!;

	#endregion

	#region Interface IObjectEvaluator

		public virtual ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token)
		{
			if (Value == null)
			{
				return new ValueTask<IObject>(DefaultObject.Null);
			}

			if (_parsingException == null && _parsedValue.IsUndefined())
			{
				_parsedValue = ParseToDataModel(ref _parsingException);
				_parsedValue.MakeDeepConstant();
			}

			if (_parsingException != null)
			{
				Infrastructure.IgnoredException(_parsingException);
			}

			return new ValueTask<IObject>(_parsedValue.CloneAsWritable());
		}

	#endregion

	#region Interface IStringEvaluator

		public virtual ValueTask<string> EvaluateString(IExecutionContext executionContext, CancellationToken token) => new ValueTask<string>(Value);

	#endregion

		protected virtual DataModelValue ParseToDataModel(ref Exception? parseException) => Value;
	}
}