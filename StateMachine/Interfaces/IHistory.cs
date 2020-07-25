namespace Xtate
{
	public interface IHistory : IEntity
	{
		IIdentifier? Id         { get; }
		HistoryType  Type       { get; }
		ITransition? Transition { get; }
	}
}