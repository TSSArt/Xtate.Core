namespace Xtate.Builder
{
	public interface IForEachBuilder
	{
		IForEach Build();

		void SetArray(IValueExpression array);
		void SetItem(ILocationExpression item);
		void SetIndex(ILocationExpression index);
		void AddAction(IExecutableEntity action);
	}
}