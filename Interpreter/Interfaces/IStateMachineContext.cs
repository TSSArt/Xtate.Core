using System;

namespace Xtate
{
	internal interface IStateMachineContext : IAsyncDisposable
	{
		IPersistenceContext         PersistenceContext { get; }
		IExecutionContext           ExecutionContext   { get; }
		EntityQueue<IEvent>         InternalQueue      { get; }
		DataModelObject             DataModel          { get; }
		OrderedSet<StateEntityNode> Configuration      { get; }
		OrderedSet<StateEntityNode> StatesToInvoke     { get; }
		KeyList<StateEntityNode>    HistoryValue       { get; }
	}
}