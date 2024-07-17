namespace Xtate.Core;

public class InvokeIdEntityParser : EntityParserBase<InvokeId?>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeId? invokeId)
	{
		if (invokeId is not null)
		{
			yield return new LoggingParameter(name: @"InvokeId", invokeId);
		}
	}
}