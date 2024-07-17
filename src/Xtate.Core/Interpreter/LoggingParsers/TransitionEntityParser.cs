namespace Xtate.Core;

public class TransitionEntityParser : EntityParserBase<ITransition>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(ITransition transition)
	{
		Infra.Requires(transition);

		yield return new LoggingParameter(name: @"TransitionType", transition.Type);

		if (!transition.EventDescriptors.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventDescriptors", EventDescriptor.ToString(transition.EventDescriptors));
		}

		if (!transition.Target.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"Target", Identifier.ToString(transition.Target));
		}
	}
}