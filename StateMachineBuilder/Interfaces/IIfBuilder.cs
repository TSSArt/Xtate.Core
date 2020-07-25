namespace Xtate.Builder
{
	public interface IIfBuilder
	{
		IIf Build();

		void SetCondition(IConditionExpression condition);
		void AddAction(IExecutableEntity action);
	}
}