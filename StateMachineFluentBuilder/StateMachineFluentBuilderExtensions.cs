#region Copyright © 2019-2023 Sergii Artemenko

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

using Xtate.IoC;

namespace Xtate.Builder;

public static class StateMachineFluentBuilderExtensions
{
	public static void RegisterStateMachineFluentBuilder(this IServiceCollection services)
	{
		services.RegisterStateMachineBuilder();

		services.AddTypeSync<StateMachineFluentBuilder>();
		services.AddTypeSync<StateFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IState>>();
		services.AddTypeSync<ParallelFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IParallel>>();
		services.AddTypeSync<FinalFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IFinal>>();
		services.AddTypeSync<InitialFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IInitial>>();
		services.AddTypeSync<HistoryFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IHistory>>();
		services.AddTypeSync<TransitionFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<ITransition>>();
	}

	private class TOuterBuilder : IStub
	{
#region Interface IStub

		public bool IsMatch(Type type) => true;

#endregion
	}
}