namespace Xtate.Core;

public class EventEntityParser : EntityParserBase<IEvent>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(IEvent evt)
	{
		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventName", EventName.ToName(evt.NameParts));
		}

		yield return new LoggingParameter(name: @"EventType", evt.Type);

		if (evt.Origin is { } origin)
		{
			yield return new LoggingParameter(name: @"Origin", origin);
		}

		if (evt.OriginType is { } originType)
		{
			yield return new LoggingParameter(name: @"OriginType", originType);
		}

		if (evt.SendId is { } sendId)
		{
			yield return new LoggingParameter(name: @"SendId", sendId);
		}

		if (evt.InvokeId is { } invokeId)
		{
			yield return new LoggingParameter(name: @"InvokeId", invokeId);
		}
	}
}