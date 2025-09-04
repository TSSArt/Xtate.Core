// Copyright © 2019-2025 Sergii Artemenko
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

public class HistoryFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
{
    public required IHistoryBuilder Builder { private get; [UsedImplicitly] init; }

    public required Action<IHistory> BuiltAction { private get; [UsedImplicitly] init; }

    public required TOuterBuilder OuterBuilder { private get; [UsedImplicitly] init; }

    public required Func<HistoryFluentBuilder<TOuterBuilder>, Action<ITransition>, TransitionFluentBuilder<HistoryFluentBuilder<TOuterBuilder>>> TransitionFluentBuilderFactory
    {
        private get;
        [UsedImplicitly] init;
    }

    public TOuterBuilder EndHistory()
    {
        BuiltAction(Builder.Build());

        return OuterBuilder;
    }

    public HistoryFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier)id);

    public HistoryFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
    {
        Infra.Requires(id);

        Builder.SetId(id);

        return this;
    }

    public HistoryFluentBuilder<TOuterBuilder> SetType(HistoryType type)
    {
        Infra.RequiresValidEnum(type);

        Builder.SetType(type);

        return this;
    }

    public TransitionFluentBuilder<HistoryFluentBuilder<TOuterBuilder>> BeginTransition() => TransitionFluentBuilderFactory(this, Builder.SetTransition);

    public HistoryFluentBuilder<TOuterBuilder> AddTransition(string target) => AddTransition((Identifier)target);

    public HistoryFluentBuilder<TOuterBuilder> AddTransition(IIdentifier target) => BeginTransition().SetTarget(target).EndTransition();
}