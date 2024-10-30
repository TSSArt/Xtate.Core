using Xtate.DataModel;

namespace Xtate.Core;

public class EventVerboseEntityParser<TSource>() : EntityParserBase<TSource, IEvent>(Level.Verbose)
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	protected override IEnumerable<LoggingParameter> EnumerateProperties(IEvent evt)
	{
		if (!evt.Data.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Data", evt.Data.ToObject());

			yield return new LoggingParameter(name: @"DataText", DataModelHandler.ConvertToText(evt.Data));
		}
	}
}