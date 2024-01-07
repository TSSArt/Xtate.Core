namespace Xtate.Core;

public interface IEntityParserProvider
{
	IEntityParserHandler? TryGetEntityParserHandler<T>(T entity);
}