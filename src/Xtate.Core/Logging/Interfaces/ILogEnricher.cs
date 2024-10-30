namespace Xtate.Core;

public interface ILogEnricher<[UsedImplicitly] TSource>
{
	string? Namespace { get; }

	Level Level { get; }

	IEnumerable<LoggingParameter> EnumerateProperties();
}