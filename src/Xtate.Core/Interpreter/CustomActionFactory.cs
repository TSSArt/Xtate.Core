namespace Xtate.CustomAction;

public class CustomActionFactory
{
	public required IEnumerable<ICustomActionProvider> CustomActionProviders { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public CustomActionBase GetCustomAction(ICustomAction customAction)
	{
		Infra.Requires(customAction);

		var ns = customAction.XmlNamespace;
		var name = customAction.XmlName;
		var xml = customAction.Xml;

		Infra.NotNull(ns);
		Infra.NotNull(name);
		Infra.NotNull(xml);

		using var enumerator = CustomActionProviders.GetEnumerator();

		while (enumerator.MoveNext())
		{
			Infra.NotNull(enumerator.Current);

			if (enumerator.Current.TryGetActivator(ns, name) is not { } activator)
			{
				continue;
			}

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.TryGetActivator(ns, name) is not null)
				{
					throw Infra.Fail<Exception>(Res.Format(Resources.Exception_MoreThenOneCustomActionProviderRegisteredForProcessingCustomActionNode, ns, name));
				}
			}

			return activator.Activate(xml);
		}

		throw Infra.Fail<Exception>(Res.Format(Resources.Exception_ThereIsNoAnyCustomActionProviderRegisteredForProcessingCustomActionNode, ns, name));
	}
}