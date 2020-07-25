using System;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ExternalScriptExpressionNode : IExternalScriptExpression, IExternalScriptConsumer, IStoreSupport, IAncestorProvider
	{
		private readonly ExternalScriptExpression _externalScriptExpression;
		private          string?                  _content;

		public ExternalScriptExpressionNode(in ExternalScriptExpression externalScriptExpression)
		{
			Infrastructure.Assert(externalScriptExpression.Uri != null);

			_externalScriptExpression = externalScriptExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _externalScriptExpression.Ancestor;

	#endregion

	#region Interface IExternalScriptConsumer

		public void SetContent(string content)
		{
			_content = content;

			if (_externalScriptExpression.Ancestor.Is<IExternalScriptConsumer>(out var externalScript))
			{
				externalScript.SetContent(content);
			}
		}

	#endregion

	#region Interface IExternalScriptExpression

		public Uri Uri => _externalScriptExpression.Uri!;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ExternalScriptExpressionNode);
			bucket.Add(Key.Uri, Uri);
			bucket.Add(Key.Content, _content);
		}

	#endregion
	}
}