namespace Xtate.Builder
{
	public interface IInitialBuilder
	{
		IInitial Build();

		void SetTransition(ITransition transition);
	}
}