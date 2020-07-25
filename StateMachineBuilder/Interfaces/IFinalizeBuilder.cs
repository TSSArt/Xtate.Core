namespace Xtate.Builder
{
	public interface IFinalizeBuilder
	{
		IFinalize Build();

		void AddAction(IExecutableEntity action);
	}
}