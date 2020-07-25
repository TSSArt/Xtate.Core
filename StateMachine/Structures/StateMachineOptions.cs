namespace Xtate
{
	public struct StateMachineOptions : IStateMachineOptions
	{
	#region Interface IStateMachineOptions

		public string? Name { get; set; }

		public PersistenceLevel? PersistenceLevel { get; set; }

		public bool? SynchronousEventProcessing { get; set; }

		public int? ExternalQueueSize { get; set; }

		public UnhandledErrorBehaviour? UnhandledErrorBehaviour { get; set; }

	#endregion
	}
}