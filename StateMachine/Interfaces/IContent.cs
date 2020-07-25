namespace Xtate
{
	public interface IContent : IEntity
	{
		IValueExpression? Expression { get; }
		IContentBody?     Body       { get; }
	}
}