namespace Xtate
{
	internal static class StateMachineOptionsExtensions
	{
		public static bool IsStateMachinePersistable(this IStateMachineOptions? options) => options == null || options.PersistenceLevel != PersistenceLevel.None;
	}
}