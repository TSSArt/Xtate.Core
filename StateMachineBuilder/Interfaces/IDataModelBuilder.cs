namespace Xtate.Builder
{
	public interface IDataModelBuilder
	{
		IDataModel Build();

		void AddData(IData data);
	}
}