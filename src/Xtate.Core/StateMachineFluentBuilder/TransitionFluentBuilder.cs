#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.DataModel.Runtime;

namespace Xtate.Builder
{
	public class TransitionFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		public required ITransitionBuilder  Builder      { private get; init; }
		public required Action<ITransition> BuiltAction  { private get; init; }
		public required TOuterBuilder       OuterBuilder { private get; init; }

		[return: NotNull]
		public TOuterBuilder EndTransition()
		{
			BuiltAction(Builder.Build());

			return OuterBuilder;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetEvent(params string[] eventDescriptors)
		{
			Infra.RequiresNonEmptyCollection(eventDescriptors);

			var eventsDescriptorsBuilder = ImmutableArray.CreateBuilder<IEventDescriptor>(eventDescriptors.Length);

			foreach (var eventDescriptor in eventDescriptors)
			{
				eventsDescriptorsBuilder.Add(EventDescriptor.FromString(eventDescriptor));
			}

			Builder.SetEvent(eventsDescriptorsBuilder.MoveToImmutable());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetEvent(params IEventDescriptor[] eventDescriptors)
		{
			Infra.RequiresNonEmptyCollection(eventDescriptors);

			Builder.SetEvent(eventDescriptors.ToImmutableArray());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetEvent(ImmutableArray<IEventDescriptor> eventDescriptors)
		{
			Infra.RequiresNonEmptyCollection(eventDescriptors);

			Builder.SetEvent(eventDescriptors);

			return this;
		}

		private TransitionFluentBuilder<TOuterBuilder> SetCondition(IConditionExpression conditionExpression)
		{
			Builder.SetCondition(conditionExpression);

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetConditionFunc(Func<bool> predicate) => SetCondition(RuntimePredicate.GetPredicate(predicate));

		public TransitionFluentBuilder<TOuterBuilder> SetConditionFuncAsync(Func<ValueTask<bool>> predicate) => SetCondition(RuntimePredicate.GetPredicate(predicate));

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(params string[] target)
		{
			Infra.RequiresNonEmptyCollection(target);

			var builder = ImmutableArray.CreateBuilder<IIdentifier>(target.Length);

			foreach (var s in target)
			{
				builder.Add((Identifier) s);
			}

			Builder.SetTarget(builder.MoveToImmutable());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(params IIdentifier[] target)
		{
			Infra.RequiresNonEmptyCollection(target);

			Builder.SetTarget(target.ToImmutableArray());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(ImmutableArray<string> target)
		{
			Infra.RequiresNonEmptyCollection(target);

			Builder.SetTarget(ImmutableArray.CreateRange<string, IIdentifier>(target, id => (Identifier) id));

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(ImmutableArray<IIdentifier> target)
		{
			Infra.RequiresNonEmptyCollection(target);

			Builder.SetTarget(target);

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetType(TransitionType type)
		{
			Builder.SetType(type);

			return this;
		}

		private TransitionFluentBuilder<TOuterBuilder> AddOnTransition(IExecutableEntity action)
		{
			Builder.AddAction(action);

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> AddOnTransition(Action action) => AddOnTransition(RuntimeAction.GetAction(action));

		public TransitionFluentBuilder<TOuterBuilder> AddOnTransitionAsync(Func<ValueTask> action) => AddOnTransition(RuntimeAction.GetAction(action));
	}
}