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

using Xtate.Builder;
using Xtate.IoC;

namespace Xtate.Core.Test.Interpreter;

[TestClass]
public class FluentBuilderTest
{
	[TestMethod]
	public async Task BasicTest()
	{
		var services = new ServiceCollection();
		services.AddModule<StateMachineFluentBuilderModule>();

		var provider = services.BuildProvider();

		var builder = await provider.GetRequiredService<StateMachineFluentBuilder>();

		builder
			.SetExternalQueueSize(4)
			.SetPersistenceLevel(PersistenceLevel.Event)
			.SetSynchronousEventProcessing(true)
			.SetInitial("init")
			.BeginState("init")
			.AddOnEntry(() => { })
			.AddOnExit(() => { })
			.BeginParallel("parallel")
			.BeginState("a1")
			.EndState()
			.BeginState("a2")
			.EndState()
			.EndParallel()
			.EndState()
			.Build();
	}

	[TestMethod]
	public void StateMachineFluentBuilderTest()
	{
		// Arrange 

		var builderMock = new Mock<IStateMachineBuilder>();
		builderMock.Setup(m => m.Build()).Returns(() => new StateMachineEntity());

		var fluentBuilder = new StateMachineFluentBuilder
							{
								Builder = builderMock.Object,
								StateFluentBuilderFactory = null!,
								ParallelFluentBuilderFactory = null!,
								FinalFluentBuilderFactory = null!
							};

		// Act

		var stateMachine = fluentBuilder.Build();

		// Assert

		Assert.IsNotNull(stateMachine);
	}

	[TestMethod]
	public void StateFluentBuilderTest()
	{
		// Arrange 

		var builderMock = new Mock<IStateBuilder>();
		builderMock.Setup(m => m.Build()).Returns(() => new StateEntity());

		var outerBuilder = new object();

		var fluentBuilder = new StateFluentBuilder<object>
							{
								Builder = builderMock.Object,
								BuiltAction = builderMock.Object.AddState,
								OuterBuilder = outerBuilder,
								InitialFluentBuilderFactory = null!,
								StateFluentBuilderFactory = null!,
								ParallelFluentBuilderFactory = null!,
								FinalFluentBuilderFactory = null!,
								HistoryFluentBuilderFactory = null!,
								TransitionFluentBuilderFactory = null!
							};

		// Act

		var endState = fluentBuilder.EndState();

		// Assert

		Assert.AreSame(outerBuilder, endState);
		builderMock.Verify(m => m.AddState(It.IsNotNull<IState>()), Times.Once);
	}

	public class TOuterBuilder : IStub
	{
	#region Interface IStub

		public bool IsMatch(Type type) => true;

	#endregion
	}
}