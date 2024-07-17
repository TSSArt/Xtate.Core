namespace Xtate.Core;

public class OutgoingEventEntityParser : EntityParserBase<IOutgoingEvent>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(IOutgoingEvent evt)
	{
		Infra.Requires(evt);

		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventName", EventName.ToName(evt.NameParts));
		}

		if (evt.SendId is { } sendId)
		{
			yield return new LoggingParameter(name: @"SendId", sendId);
		}

		if (evt.Type is { } type)
		{
			yield return new LoggingParameter(name: @"Type", type);
		}

		if (evt.Target is { } target)
		{
			yield return new LoggingParameter(name: @"Target", target);
		}

		if (evt.DelayMs is var delayMs and > 0)
		{
			yield return new LoggingParameter(name: @"DelayMs", delayMs);
		}
	}
}