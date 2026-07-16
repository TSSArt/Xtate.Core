// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.IoC.AncestorTracker.Internal;

namespace Xtate.IoC.AncestorTracker.Services;

[InstantiatedByIoC]
public class AncestorTracker : IAncestorTracker, IServiceProviderActions, IServiceProviderDataActions
{
	private readonly AsyncLocal<Node?> _node = new();

#region Interface IAncestorTracker

	public bool IsAncestorTypeEquals(int level, Type type)
	{
		Infra.RequiresNonNegative(level);

		var node = _node.Value;

		for (; node != null && level > 0; node = node.PrevNode, level --) { }

		return node?.AncestorType == type;
	}

#endregion

#region Interface IServiceProviderActions

	IServiceProviderDataActions? IServiceProviderActions.RegisterServices(int _) => null;

	IServiceProviderDataActions? IServiceProviderActions.Event(ActionsEventType type, ref ActionsContext context)
	{
		switch (type)
		{
			case ActionsEventType.FactoryCalling:
				context.UserDataObject = _node.Value = new Node(context.ServiceType, _node.Value);

				break;

			case ActionsEventType.FactoryCalled:
				return this;

			case ActionsEventType.FactoryCallRunning:
			case ActionsEventType.FactoryCallError:
				RestoreNode(context.UserDataObject);

				break;
		}

		return null;
	}

#endregion

#region Interface IServiceProviderDataActions

	[ExcludeFromCodeCoverage]
	void IServiceProviderDataActions.RegisterService(ServiceEntry serviceEntry) { }

	void IServiceProviderDataActions.Event<T, TArg>(ActionsEventType type, ref DataActionsContext<T, TArg> context)
	{
		Infra.Assert(type is ActionsEventType.FactoryCalled);

		if (RestoreNode(context.UserDataObject).AncestorConsumer is IAncestorConsumer<T> ancestorConsumer)
		{
			ancestorConsumer.SetValue(context.Instance);
		}
	}

#endregion

	private Node RestoreNode(object? userDataObject)
	{
		var node = (Node) userDataObject!;
		_node.Value = node.PrevNode;

		return node;
	}

	public void CaptureAncestor<T>(IAncestorConsumer<T> ancestorConsumer)
	{
		for (var node = _node.Value; node != null; node = node.PrevNode)
		{
			if (node.AncestorType == typeof(T))
			{
				while (true)
				{
					var preVal = (IAncestorConsumer<T>?) node.AncestorConsumer;
					var newVal = preVal is null ? ancestorConsumer : new CombinedAncestorConsumer<T>(preVal, ancestorConsumer);

					if (Interlocked.CompareExchange(ref node.AncestorConsumer, newVal, preVal) == preVal)
					{
						return;
					}
				}
			}
		}
	}
}