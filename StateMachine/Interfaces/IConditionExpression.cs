namespace Xtate
{
	public interface IConditionExpression : IExecutableEntity
	{
		string? Expression { get; }
	}
}