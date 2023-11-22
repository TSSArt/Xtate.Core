using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Xtate.Builder;
using Xtate.IoC;

namespace Xtate.Core.Test.Interpreter
{
	[TestClass]
	public class FluentBuilderTest
	{
		public class TOuterBuilder : IStub
		{
			public bool IsMatch(Type type) => true;
		}

		[TestMethod]
		public async Task BasicTest()
		{
			var services = new ServiceCollection();
			services.RegisterStateMachineFluentBuilder();
			
			var provider = services.BuildProvider();

			var builder = await provider.GetRequiredService<StateMachineFluentBuilder>();
			
			var stateMachine = builder
				.SetExternalQueueSize(4)
				.SetPersistenceLevel(PersistenceLevel.Event)
				.SetSynchronousEventProcessing(true)
				.SetInitial("init")
				.BeginState("init")
					.AddOnEntry(() => {})
					.AddOnExit(() => {})
					.BeginParallel("parallel")
						.BeginState("a1").EndState()
						.BeginState("a2").EndState()
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
									TransitionFluentBuilderFactory = null!,
								};

			// Act

			var endState = fluentBuilder.EndState();

			// Assert
			
			Assert.AreSame(outerBuilder, endState);
			builderMock.Verify(m => m.AddState(It.IsNotNull<IState>()), Times.Once);
		}
	}
}
