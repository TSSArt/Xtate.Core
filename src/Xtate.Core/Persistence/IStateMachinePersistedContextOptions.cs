namespace Xtate.Persistence;

public interface IStateMachinePersistedContextOptions
{
	ImmutableDictionary<int, IEntity> EntityMap { get; }
}