namespace Xtate
{
	public interface IScript : IExecutableEntity
	{
		IScriptExpression?         Content { get; }
		IExternalScriptExpression? Source  { get; }
	}
}