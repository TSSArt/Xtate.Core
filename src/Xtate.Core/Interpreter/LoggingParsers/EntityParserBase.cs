namespace Xtate.Core;

public abstract class EntityParserBase<TEntity> : IEntityParserProvider, IEntityParserHandler
{
#region Interface IEntityParserHandler

	IEnumerable<LoggingParameter> IEntityParserHandler.EnumerateProperties<T>(T entity) => EnumerateProperties(ConvertHelper<T, TEntity>.Convert(entity));

#endregion

#region Interface IEntityParserProvider

	public virtual IEntityParserHandler? TryGetEntityParserHandler<T>(T entity) => entity is TEntity ? this : default;

#endregion

	protected abstract IEnumerable<LoggingParameter> EnumerateProperties(TEntity entity);
}