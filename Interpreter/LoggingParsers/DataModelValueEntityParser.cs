namespace Xtate.Core;

public class DataModelValueEntityParser : EntityParserBase<DataModelValue>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(DataModelValue value)
	{
		if (!value.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Parameter", value);
		}
	}
}