namespace Xtate
{
	public interface IAssign : IExecutableEntity
	{
		ILocationExpression? Location      { get; }
		IValueExpression?    Expression    { get; }
		IInlineContent?      InlineContent { get; }
		string?              Type          { get; }
		string?              Attribute     { get; }
	}
}