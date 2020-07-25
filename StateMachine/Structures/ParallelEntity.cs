using System;
using System.Collections.Immutable;

namespace Xtate
{
	public struct ParallelEntity : IParallel, IVisitorEntity<ParallelEntity, IParallel>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IParallel

		public IIdentifier?                 Id            { get; set; }
		public ImmutableArray<IStateEntity> States        { get; set; }
		public ImmutableArray<IHistory>     HistoryStates { get; set; }
		public ImmutableArray<ITransition>  Transitions   { get; set; }
		public IDataModel?                  DataModel     { get; set; }
		public ImmutableArray<IOnEntry>     OnEntry       { get; set; }
		public ImmutableArray<IOnExit>      OnExit        { get; set; }
		public ImmutableArray<IInvoke>      Invoke        { get; set; }

	#endregion

	#region Interface IVisitorEntity<ParallelEntity,IParallel>

		void IVisitorEntity<ParallelEntity, IParallel>.Init(IParallel source)
		{
			Ancestor = source;
			Id = source.Id;
			Invoke = source.Invoke;
			States = source.States;
			HistoryStates = source.HistoryStates;
			DataModel = source.DataModel;
			OnExit = source.OnExit;
			OnEntry = source.OnEntry;
			Transitions = source.Transitions;
		}

		bool IVisitorEntity<ParallelEntity, IParallel>.RefEquals(ref ParallelEntity other) =>
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(DataModel, other.DataModel) &&
				Invoke == other.Invoke &&
				States == other.States &&
				HistoryStates == other.HistoryStates &&
				OnExit == other.OnExit &&
				OnEntry == other.OnEntry &&
				Transitions == other.Transitions;

	#endregion
	}
}