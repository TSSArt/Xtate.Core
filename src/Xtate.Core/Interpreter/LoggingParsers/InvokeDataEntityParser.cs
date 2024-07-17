namespace Xtate.Core;

public class InvokeDataEntityParser : EntityParserBase<InvokeData>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		if (invokeData.InvokeId is { } invokeId)
		{
			yield return new LoggingParameter(name: @"InvokeId", invokeId);
		}

		yield return new LoggingParameter(name: @"InvokeType", invokeData.Type);

		if (invokeData.Source is { } source)
		{
			yield return new LoggingParameter(name: @"InvokeSource", source);
		}
	}
}