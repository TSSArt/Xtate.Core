using System.Collections.Immutable;

namespace Xtate
{
	public interface IState : IStateEntity
	{
		IIdentifier?                 Id            { get; }
		IInitial?                    Initial       { get; }
		ImmutableArray<IStateEntity> States        { get; }
		ImmutableArray<IHistory>     HistoryStates { get; }
		ImmutableArray<ITransition>  Transitions   { get; }
		IDataModel?                  DataModel     { get; }
		ImmutableArray<IOnEntry>     OnEntry       { get; }
		ImmutableArray<IOnExit>      OnExit        { get; }
		ImmutableArray<IInvoke>      Invoke        { get; }
	}
}