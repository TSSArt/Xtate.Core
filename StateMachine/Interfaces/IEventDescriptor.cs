namespace Xtate
{
	public interface IEventDescriptor : IEntity
	{
		string Value { get; }

		bool IsEventMatch(IEvent evt);
	}
}