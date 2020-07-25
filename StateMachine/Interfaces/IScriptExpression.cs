namespace Xtate
{
	public interface IScriptExpression : IExecutableEntity
	{
		string? Expression { get; }
	}
}