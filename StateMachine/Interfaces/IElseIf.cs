namespace Xtate
{
	public interface IElseIf : IExecutableEntity
	{
		IConditionExpression? Condition { get; }
	}
}