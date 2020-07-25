namespace Xtate.Builder
{
	public interface IHistoryBuilder
	{
		IHistory Build();

		void SetId(IIdentifier id);
		void SetType(HistoryType type);
		void SetTransition(ITransition transition);
	}
}