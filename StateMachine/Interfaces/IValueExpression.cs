namespace Xtate
{
	public interface IValueExpression : IExecutableEntity
	{
		string? Expression { get; }
	}
}