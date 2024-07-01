namespace Xtate.Core;

public interface IEntityParserHandler
{
	IEnumerable<LoggingParameter> EnumerateProperties<T>(T entity);
}