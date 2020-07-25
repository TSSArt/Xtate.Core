namespace Xtate
{
	public interface IRaise : IExecutableEntity
	{
		IOutgoingEvent? OutgoingEvent { get; }
	}
}