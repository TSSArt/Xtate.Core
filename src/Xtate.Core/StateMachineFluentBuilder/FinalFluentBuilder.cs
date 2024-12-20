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

using Xtate.DataModel.Runtime;

namespace Xtate.Builder;

public class FinalFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
{
	public required IFinalBuilder Builder { private get; [UsedImplicitly] init; }

	public required Action<IFinal> BuiltAction { private get; [UsedImplicitly] init; }

	public required TOuterBuilder OuterBuilder { private get; [UsedImplicitly] init; }

	public required Func<IContentBuilder> ContentBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<IDoneDataBuilder> DoneDataBuilderFactory { private get; [UsedImplicitly] init; }

	public TOuterBuilder EndFinal()
	{
		BuiltAction(Builder.Build());

		return OuterBuilder;
	}

	public FinalFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

	public FinalFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
	{
		Infra.Requires(id);

		Builder.SetId(id);

		return this;
	}

	private FinalFluentBuilder<TOuterBuilder> SetDoneData(IValueExpression evaluator)
	{
		var contentBuilder = ContentBuilderFactory();
		contentBuilder.SetExpression(evaluator);

		var doneData = DoneDataBuilderFactory();
		doneData.SetContent(contentBuilder.Build());

		Builder.SetDoneData(doneData.Build());

		return this;
	}

	public FinalFluentBuilder<TOuterBuilder> SetDoneData(DataModelValue value) => SetDoneData(RuntimeValue.GetValue(value));

	public FinalFluentBuilder<TOuterBuilder> SetDoneData(Func<DataModelValue> evaluator) => SetDoneData(RuntimeValue.GetValue(evaluator));

	public FinalFluentBuilder<TOuterBuilder> SetDoneData(Func<ValueTask<DataModelValue>> evaluator) => SetDoneData(RuntimeValue.GetValue(evaluator));

	private FinalFluentBuilder<TOuterBuilder> AddOnEntry(IExecutableEntity action)
	{
		Builder.AddOnEntry(new OnEntryEntity { Action = [action] });

		return this;
	}

	public FinalFluentBuilder<TOuterBuilder> AddOnEntry(Action action) => AddOnEntry(RuntimeAction.GetAction(action));

	public FinalFluentBuilder<TOuterBuilder> AddOnEntry(Func<ValueTask> action) => AddOnEntry(RuntimeAction.GetAction(action));

	private FinalFluentBuilder<TOuterBuilder> AddOnExit(IExecutableEntity action)
	{
		Builder.AddOnExit(new OnExitEntity { Action = [action] });

		return this;
	}

	public FinalFluentBuilder<TOuterBuilder> AddOnExit(Action action) => AddOnExit(RuntimeAction.GetAction(action));

	public FinalFluentBuilder<TOuterBuilder> AddOnExit(Func<ValueTask> action) => AddOnExit(RuntimeAction.GetAction(action));
}