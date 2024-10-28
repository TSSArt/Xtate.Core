namespace Xtate.DataModel;

public interface IDataModelTypeToUriConverter
{
	Uri GetUri(string dataModelType);
}