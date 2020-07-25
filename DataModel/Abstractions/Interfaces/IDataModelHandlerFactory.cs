namespace Xtate.DataModel
{
	public interface IDataModelHandlerFactory
	{
		bool CanHandle(string dataModelType);

		IDataModelHandler CreateHandler(IErrorProcessor errorProcessor);
	}
}