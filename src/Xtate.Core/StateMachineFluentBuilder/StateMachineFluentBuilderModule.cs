// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Builder;

public class StateMachineFluentBuilderModule : Module
{
	protected override void AddModules()
	{
		AddModule<StateMachineBuilderModule>();
	}

	protected override void AddServices()
	{
		Services.AddTypeSync<StateMachineFluentBuilder>();
		Services.AddTypeSync<StateFluentBuilder<OuterBuilderStub>, OuterBuilderStub, Action<IState>>();
		Services.AddTypeSync<ParallelFluentBuilder<OuterBuilderStub>, OuterBuilderStub, Action<IParallel>>();
		Services.AddTypeSync<FinalFluentBuilder<OuterBuilderStub>, OuterBuilderStub, Action<IFinal>>();
		Services.AddTypeSync<InitialFluentBuilder<OuterBuilderStub>, OuterBuilderStub, Action<IInitial>>();
		Services.AddTypeSync<HistoryFluentBuilder<OuterBuilderStub>, OuterBuilderStub, Action<IHistory>>();
		Services.AddTypeSync<TransitionFluentBuilder<OuterBuilderStub>, OuterBuilderStub, Action<ITransition>>();
	}

	[UsedImplicitly]
	private class OuterBuilderStub : IStub
	{
	#region Interface IStub

		public bool IsMatch(Type type) => true;

	#endregion
	}
}