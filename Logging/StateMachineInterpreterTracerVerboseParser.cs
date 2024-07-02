using Xtate.DataModel;

namespace Xtate.Core;

public class StateMachineInterpreterTracerVerboseParser : StateMachineInterpreterTracerParser
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	public override IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeData invokeData)
	{
		foreach (var property in base.EnumerateProperties(invokeData))
		{
			yield return property;
		}

		if (invokeData.RawContent is { } rawContent)
		{
			yield return (@"RawContent", rawContent);
		}

		if (!invokeData.Content.IsUndefined())
		{
			yield return (@"Content", invokeData.Content.ToObject()!);
			yield return (@"ContentText", DataModelHandler.ConvertToText(invokeData.Content));
		}

		if (!invokeData.Parameters.IsUndefined())
		{
			yield return (@"Parameters", invokeData.Parameters.ToObject()!);
			yield return (@"ParametersText", DataModelHandler.ConvertToText(invokeData.Parameters));
		}
	}

	public override IEnumerable<(string Name, object Value)> EnumerateProperties(IOutgoingEvent outgoingEvent)
	{
		foreach (var property in base.EnumerateProperties(outgoingEvent))
		{
			yield return property;
		}

		if (!outgoingEvent.Data.IsUndefined())
		{
			yield return (@"Data", outgoingEvent.Data.ToObject()!);
			yield return (@"DataText", DataModelHandler.ConvertToText(outgoingEvent.Data));
		}
	}

	public override IEnumerable<(string Name, object Value)> EnumerateProperties(IEvent evt)
	{
		foreach (var property in base.EnumerateProperties(evt))
		{
			yield return property;
		}

		if (!evt.Data.IsUndefined())
		{
			yield return (@"Data", evt.Data.ToObject()!);
			yield return (@"DataText", DataModelHandler.ConvertToText(evt.Data));
		}
	}
}