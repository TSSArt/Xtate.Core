namespace Xtate.Persistence;

public interface IEntityMap
{
	bool TryGetEntityByDocumentId(int id, [MaybeNullWhen(false)] out IEntity entity);
}