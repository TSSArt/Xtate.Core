namespace Xtate.Builder
{
	public interface IContentBuilder
	{
		IContent Build();

		void SetExpression(IValueExpression expression);
		void SetBody(IContentBody contentBody);
	}
}