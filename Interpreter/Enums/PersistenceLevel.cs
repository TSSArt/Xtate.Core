namespace Xtate
{
	public enum PersistenceLevel
	{
		None             = 0,
		StableState      = 1,
		Event            = 2,
		Transition       = 3,
		ExecutableAction = 4
	}
}