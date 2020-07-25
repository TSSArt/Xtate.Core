namespace Xtate.Builder
{
	public interface IAssignBuilder
	{
		IAssign Build();

		void SetLocation(ILocationExpression location);
		void SetExpression(IValueExpression expression);
		void SetInlineContent(IInlineContent inlineContent);
		void SetType(string type);
		void SetAttribute(string attribute);
	}
}