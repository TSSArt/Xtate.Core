namespace Xtate.Core;

public class ExceptionEntityParser : EntityParserBase<Exception>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(Exception exception)
	{
		yield return new LoggingParameter(name: @"Exception", exception);
	}
}