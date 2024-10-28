namespace Xtate.DataModel;

public interface IAssemblyContainerProvider
{
	IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders();
}