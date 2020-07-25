namespace Xtate
{
	public interface IParam : IEntity
	{
		string?              Name       { get; }
		IValueExpression?    Expression { get; }
		ILocationExpression? Location   { get; }
	}
}