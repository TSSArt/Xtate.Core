namespace Xtate.Core;

public class LogEntityParserService : IEntityParserHandler
{
	public required IEnumerable<IEntityParserProvider> Providers { private get; [UsedImplicitly] init; }

#region Interface IEntityParserHandler

	public IEnumerable<LoggingParameter> EnumerateProperties<T>(T entity)
	{
		foreach (var provider in Providers)
		{
			if (provider.TryGetEntityParserHandler(entity) is { } handler)
			{
				return handler.EnumerateProperties(entity);
			}
		}

		throw new InfrastructureException(Res.Format(Resources.Exception_CantFindEntityParser, typeof(T)));
	}

#endregion
}