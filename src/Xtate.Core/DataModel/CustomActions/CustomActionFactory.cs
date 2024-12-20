﻿// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Xtate.CustomAction;

public class CustomActionFactory
{
	public required ServiceSyncList<IActionProvider> ActionProviders { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public IAction GetAction(ICustomAction customAction)
	{
		Infra.Requires(customAction);

		var ns = customAction.XmlNamespace;
		var name = customAction.XmlName;
		var xml = customAction.Xml;

		Infra.NotNull(ns);
		Infra.NotNull(name);
		Infra.NotNull(xml);

		var actionProviders = ActionProviders.GetEnumerator();

		while (actionProviders.MoveNext())
		{
			if (actionProviders.Current.TryGetActivator(ns, name) is not { } activator)
			{
				continue;
			}

			while (actionProviders.MoveNext())
			{
				if (actionProviders.Current.TryGetActivator(ns, name) is not null)
				{
					Infra.Fail(Res.Format(Resources.Exception_MoreThanOneCustomActionProviderRegisteredForProcessingCustomActionNode, ns, name));
				}
			}

			return activator.Activate(xml);
		}

		throw Infra.Fail<Exception>(Res.Format(Resources.Exception_ThereIsNoAnyCustomActionProviderRegisteredForProcessingCustomActionNode, ns, name));
	}
}