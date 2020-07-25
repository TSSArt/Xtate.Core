using System;

namespace Xtate
{
	public struct ExternalDataExpression : IExternalDataExpression, IVisitorEntity<ExternalDataExpression, IExternalDataExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IExternalDataExpression

		public Uri? Uri { get; set; }

	#endregion

	#region Interface IVisitorEntity<ExternalDataExpression,IExternalDataExpression>

		void IVisitorEntity<ExternalDataExpression, IExternalDataExpression>.Init(IExternalDataExpression source)
		{
			Ancestor = source;
			Uri = source.Uri;
		}

		bool IVisitorEntity<ExternalDataExpression, IExternalDataExpression>.RefEquals(ref ExternalDataExpression other) => ReferenceEquals(Uri, other.Uri);

	#endregion
	}
}