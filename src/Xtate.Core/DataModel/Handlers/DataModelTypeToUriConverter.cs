using System.Globalization;

namespace Xtate.DataModel;

public class DataModelTypeToUriConverter(string uriFormat) : IDataModelTypeToUriConverter
{
#region Interface IDataModelTypeToUriConverter

	public virtual Uri GetUri(string dataModelType)
	{
		var uriString = string.Format(CultureInfo.InvariantCulture, uriFormat, dataModelType);

		return new Uri(uriString, UriKind.RelativeOrAbsolute);
	}

#endregion
}