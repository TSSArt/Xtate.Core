namespace Xtate
{
	public interface ILog : IExecutableEntity
	{
		string?           Label      { get; }
		IValueExpression? Expression { get; }
	}
}