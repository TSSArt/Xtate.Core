namespace Xtate.Builder
{
	public interface IFinalBuilder
	{
		IFinal Build();

		void SetId(IIdentifier id);
		void AddOnEntry(IOnEntry onEntry);
		void AddOnExit(IOnExit onExit);
		void SetDoneData(IDoneData doneData);
	}
}