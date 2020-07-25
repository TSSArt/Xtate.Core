namespace Xtate
{
	public interface IInitial : IEntity
	{
		ITransition? Transition { get; }
	}
}