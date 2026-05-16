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

using Xtate.IoC;

namespace Xtate.Core;

[InstantiatedByIoC]
[Obsolete]
public class AncestorTracker : IServiceProviderActions, IServiceProviderDataActions
{
	private readonly AsyncLocal<Node?> _node = new();

#region Interface IServiceProviderActions

	public IServiceProviderDataActions? RegisterServices(int _) => null;

	public IServiceProviderDataActions? Event(ActionsEventType type, ref ActionsContext context)
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
	public void RegisterService(ServiceEntry serviceEntry) { }

	public void Event<T, TArg>(ActionsEventType type, ref DataActionsContext<T, TArg> context)
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

	public bool IsAncestorTypeEquals(int level, Type type)
	{
		Infra.RequiresNonNegative(level);

		var node = _node.Value;

		for (; node != null && level > 0; node = node.PrevNode, level --) { }

		return node?.AncestorType == type;
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
					var newVal = Combined(preVal, ancestorConsumer);

					if (Interlocked.CompareExchange(ref node.AncestorConsumer, newVal, preVal) == preVal)
					{
						return;
					}
				}
			}
		}
	}

	private static IAncestorConsumer<T> Combined<T>(IAncestorConsumer<T>? ancestorConsumerRoot, IAncestorConsumer<T> ancestorConsumer) =>
		ancestorConsumerRoot is null ? ancestorConsumer : new CombinedAncestorConsumer<T>(ancestorConsumerRoot, ancestorConsumer);

	private class Node(Type ancestorType, Node? prevNode)
	{
		public readonly Type AncestorType = ancestorType;

		public readonly Node? PrevNode = prevNode;

		public object? AncestorConsumer;
	}

	private class CombinedAncestorConsumer<T>(IAncestorConsumer<T> ancestorConsumer1, IAncestorConsumer<T> ancestorConsumer2) : IAncestorConsumer<T>
	{
	#region Interface IAncestorConsumer<T>

		public void SetValue(T? value)
		{
			ancestorConsumer1.SetValue(value);
			ancestorConsumer2.SetValue(value);
		}

	#endregion
	}
}