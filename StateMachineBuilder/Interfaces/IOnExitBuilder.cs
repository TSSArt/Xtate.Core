namespace Xtate.Builder
{
	public interface IOnExitBuilder
	{
		IOnExit Build();

		void AddAction(IExecutableEntity action);
	}
}