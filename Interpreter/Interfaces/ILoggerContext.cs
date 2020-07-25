namespace Xtate
{
	public interface ILoggerContext
	{
		public SessionId? SessionId { get; }

		public string? StateMachineName { get; }
	}
}