namespace Xtate.Builder
{
	public interface IRaiseBuilder
	{
		IRaise Build();

		void SetEvent(IOutgoingEvent evt);
	}
}