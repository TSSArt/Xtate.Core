namespace Xtate.Core;

public class DefaultPersistenceOptions : IPersistenceOptions
{
	public PersistenceLevel PersistenceLevel => PersistenceLevel.StableState;
}