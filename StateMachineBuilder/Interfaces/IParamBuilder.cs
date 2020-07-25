namespace Xtate.Builder
{
	public interface IParamBuilder
	{
		IParam Build();

		void SetName(string name);
		void SetExpression(IValueExpression expression);
		void SetLocation(ILocationExpression location);
	}
}