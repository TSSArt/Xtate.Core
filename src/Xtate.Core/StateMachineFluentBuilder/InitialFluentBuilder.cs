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
using System.Diagnostics.CodeAnalysis;

namespace Xtate.Builder
{
	[PublicAPI]
	public class InitialFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		private readonly IInitialBuilder  _builder;
		private readonly Action<IInitial> _builtAction;
		private readonly IBuilderFactory  _factory;
		private readonly TOuterBuilder    _outerBuilder;

		public InitialFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<IInitial> builtAction)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateInitialBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		[return: NotNull]
		public TOuterBuilder EndInitial()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public TransitionFluentBuilder<InitialFluentBuilder<TOuterBuilder>> BeginTransition() => new(_factory, this, _builder.SetTransition);

		public InitialFluentBuilder<TOuterBuilder> AddTransition(string target) => AddTransition((Identifier) target);

		public InitialFluentBuilder<TOuterBuilder> AddTransition(IIdentifier target) => BeginTransition().SetTarget(target).EndTransition();
	}
}