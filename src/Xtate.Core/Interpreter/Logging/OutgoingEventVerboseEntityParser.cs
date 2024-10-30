using Xtate.DataModel;

namespace Xtate.Core;

public class OutgoingEventVerboseEntityParser<TSource>() : EntityParserBase<TSource, IOutgoingEvent>(Level.Verbose)
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	protected override IEnumerable<LoggingParameter> EnumerateProperties(IOutgoingEvent evt)
	{
		if (!evt.Data.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Data", evt.Data.ToObject());

			yield return new LoggingParameter(name: @"DataText", DataModelHandler.ConvertToText(evt.Data));
		}
	}
}