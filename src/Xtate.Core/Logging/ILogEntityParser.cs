namespace Xtate.Core;

public interface ILogEntityParser<in TEntity>
{
	IEnumerable<(string Name, object Value)> EnumerateProperties(TEntity entity);
}