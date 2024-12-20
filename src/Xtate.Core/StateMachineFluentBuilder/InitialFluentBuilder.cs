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

namespace Xtate.Builder;

public class InitialFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
{
	public required IInitialBuilder Builder { private get; [UsedImplicitly] init; }

	public required Action<IInitial> BuiltAction { private get; [UsedImplicitly] init; }

	public required TOuterBuilder OuterBuilder { private get; [UsedImplicitly] init; }

	public required Func<InitialFluentBuilder<TOuterBuilder>, Action<ITransition>, TransitionFluentBuilder<InitialFluentBuilder<TOuterBuilder>>> TransitionFluentBuilderFactory
	{
		private get;
		[UsedImplicitly] init;
	}

	public TOuterBuilder EndInitial()
	{
		BuiltAction(Builder.Build());

		return OuterBuilder;
	}

	public TransitionFluentBuilder<InitialFluentBuilder<TOuterBuilder>> BeginTransition() => TransitionFluentBuilderFactory(this, Builder.SetTransition);

	public InitialFluentBuilder<TOuterBuilder> AddTransition(string target) => AddTransition((Identifier) target);

	public InitialFluentBuilder<TOuterBuilder> AddTransition(IIdentifier target) => BeginTransition().SetTarget(target).EndTransition();
}