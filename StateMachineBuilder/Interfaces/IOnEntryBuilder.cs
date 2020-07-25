namespace Xtate.Builder
{
	public interface IOnEntryBuilder
	{
		IOnEntry Build();

		void AddAction(IExecutableEntity action);
	}
}