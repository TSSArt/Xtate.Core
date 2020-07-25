namespace Xtate.Builder
{
	public interface IElseIfBuilder
	{
		IElseIf Build();

		void SetCondition(IConditionExpression condition);
	}
}