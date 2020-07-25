using System;

namespace Xtate
{
	public struct ExternalScriptExpression : IExternalScriptExpression, IVisitorEntity<ExternalScriptExpression, IExternalScriptExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; set; }

	#endregion

	#region Interface IVisitorEntity<ExternalScriptExpression,IExternalScriptExpression>

		void IVisitorEntity<ExternalScriptExpression, IExternalScriptExpression>.Init(IExternalScriptExpression source)
		{
			Ancestor = source;
			Uri = source.Uri;
		}

		bool IVisitorEntity<ExternalScriptExpression, IExternalScriptExpression>.RefEquals(ref ExternalScriptExpression other) => ReferenceEquals(Uri, other.Uri);

	#endregion
	}
}