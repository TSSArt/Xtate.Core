using Xtate.DataModel;

namespace Xtate.Core;

public class InvokeDataVerboseEntityParser<TSource>() : EntityParserBase<TSource, InvokeData>(Level.Verbose)
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		if (invokeData.RawContent is { } rawContent)
		{
			yield return new LoggingParameter(name: @"RawContent", rawContent);
		}

		if (!invokeData.Content.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Content", invokeData.Content.ToObject()!);

			yield return new LoggingParameter(name: @"ContentText", DataModelHandler.ConvertToText(invokeData.Content));
		}

		if (!invokeData.Parameters.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Parameters", invokeData.Parameters.ToObject()!);

			yield return new LoggingParameter(name: @"ParametersText", DataModelHandler.ConvertToText(invokeData.Parameters));
		}
	}
}