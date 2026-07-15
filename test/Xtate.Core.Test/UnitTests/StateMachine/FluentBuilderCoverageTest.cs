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

using Xtate.DataTypes;
using Xtate.StateMachine;
using Xtate.StateMachine.Builder;
using Xtate.StateMachineFluentBuilder;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class FluentBuilderCoverageTest
{
	[TestMethod]
	public void StateMachineBuilderCoversInitialOverloadsNestedBuildersAndBuild()
	{
		var builder = new Mock<IStateMachineBuilder>();
		var builtMachine = Mock.Of<IStateMachine>();
		builder.Setup(static b => b.Build()).Returns(builtMachine);
		var initialValues = new List<ImmutableArray<IIdentifier>>();
		builder.Setup(static b => b.SetInitial(It.IsAny<ImmutableArray<IIdentifier>>()))
			   .Callback((ImmutableArray<IIdentifier> values) => initialValues.Add(values));
		var stateBuilder = new Mock<IStateBuilder>();
		stateBuilder.Setup(static b => b.Build()).Returns(Mock.Of<IState>());
		var parallelBuilder = new Mock<IParallelBuilder>();
		parallelBuilder.Setup(static b => b.Build()).Returns(Mock.Of<IParallel>());
		var finalBuilder = new Mock<IFinalBuilder>();
		finalBuilder.Setup(static b => b.Build()).Returns(Mock.Of<IFinal>());
		var fluent = new StateMachineFluentBuilder.StateMachineFluentBuilder
					 {
						 Builder = builder.Object,
						 StateFluentBuilderFactory = (outer, built) => CreateStateBuilder(outer, built, stateBuilder.Object),
						 ParallelFluentBuilderFactory = (outer, built) => CreateParallelBuilder(outer, built, parallelBuilder.Object),
						 FinalFluentBuilderFactory = (outer, built) => CreateFinalBuilder(outer, built, finalBuilder.Object)
					 };
		var id = Identifier.FromString("id");

		Assert.AreSame(fluent, fluent.SetInitial("one", "two"));
		Assert.AreSame(fluent, fluent.SetInitial(id));
		Assert.AreSame(fluent, fluent.SetInitial(ImmutableArray.Create("three")));
		Assert.AreSame(fluent, fluent.SetInitial(ImmutableArray.Create<IIdentifier>(id)));
		Assert.HasCount(expected: 4, initialValues);
		CollectionAssert.AreEqual(new[] { "one", "two" }, initialValues[0].Select(static value => value.Value).ToArray());

		Assert.AreSame(fluent, fluent.BeginState().EndState());
		Assert.AreSame(fluent, fluent.BeginState("state").EndState());
		Assert.AreSame(fluent, fluent.BeginState(id).EndState());
		Assert.AreSame(fluent, fluent.BeginParallel().EndParallel());
		Assert.AreSame(fluent, fluent.BeginParallel("parallel").EndParallel());
		Assert.AreSame(fluent, fluent.BeginParallel(id).EndParallel());
		Assert.AreSame(fluent, fluent.BeginFinal().EndFinal());
		Assert.AreSame(fluent, fluent.BeginFinal("final").EndFinal());
		Assert.AreSame(fluent, fluent.BeginFinal(id).EndFinal());
		stateBuilder.Verify(static b => b.SetId(It.IsAny<IIdentifier>()), Times.Exactly(2));
		parallelBuilder.Verify(static b => b.SetId(It.IsAny<IIdentifier>()), Times.Exactly(2));
		finalBuilder.Verify(static b => b.SetId(It.IsAny<IIdentifier>()), Times.Exactly(2));

		Assert.AreSame(builtMachine, fluent.Build());
		builder.Verify(static b => b.SetDataModelType("runtime"), Times.Once);
		builder.Verify(static b => b.AddState(It.IsAny<IState>()), Times.Exactly(3));
		builder.Verify(static b => b.AddParallel(It.IsAny<IParallel>()), Times.Exactly(3));
		builder.Verify(static b => b.AddFinal(It.IsAny<IFinal>()), Times.Exactly(3));
	}

	[TestMethod]
	public void FinalBuilderCoversIdentityDoneDataActionOverloadsAndEnd()
	{
		var builder = new Mock<IFinalBuilder>();
		var builtFinal = Mock.Of<IFinal>();
		builder.Setup(static b => b.Build()).Returns(builtFinal);
		var contentBuilder = new Mock<IContentBuilder>();
		var content = Mock.Of<IContent>();
		contentBuilder.Setup(static b => b.Build()).Returns(content);
		var doneDataBuilder = new Mock<IDoneDataBuilder>();
		doneDataBuilder.Setup(static b => b.Build()).Returns(Mock.Of<IDoneData>());
		var outer = new object();
		IFinal? captured = null;
		var fluent = new FinalFluentBuilder<object>
					 {
						 Builder = builder.Object,
						 BuiltAction = value => captured = value,
						 OuterBuilder = outer,
						 ContentBuilderFactory = () => contentBuilder.Object,
						 DoneDataBuilderFactory = () => doneDataBuilder.Object
					 };
		var id = Identifier.FromString("final");

		Assert.AreSame(fluent, fluent.SetId("first"));
		Assert.AreSame(fluent, fluent.SetId(id));
		Assert.AreSame(fluent, fluent.SetDoneData(new DataModelValue("constant")));
		Assert.AreSame(fluent, fluent.SetDoneData([ExcludeFromCodeCoverage] static () => new DataModelValue("sync")));
		Assert.AreSame(fluent, fluent.SetDoneData([ExcludeFromCodeCoverage] static () => new ValueTask<DataModelValue>(new DataModelValue("async"))));
		Assert.AreSame(fluent, fluent.AddOnEntry([ExcludeFromCodeCoverage] static () => { }));
		Assert.AreSame(fluent, fluent.AddOnEntry([ExcludeFromCodeCoverage] static () => ValueTask.CompletedTask));
		Assert.AreSame(fluent, fluent.AddOnExit([ExcludeFromCodeCoverage] static () => { }));
		Assert.AreSame(fluent, fluent.AddOnExit([ExcludeFromCodeCoverage] static () => ValueTask.CompletedTask));
		Assert.AreSame(outer, fluent.EndFinal());
		Assert.AreSame(builtFinal, captured);
		builder.Verify(static b => b.SetId(It.IsAny<IIdentifier>()), Times.Exactly(2));
		contentBuilder.Verify(static b => b.SetExpression(It.IsAny<IValueExpression>()), Times.Exactly(3));
		doneDataBuilder.Verify(b => b.SetContent(content), Times.Exactly(3));
		builder.Verify(static b => b.SetDoneData(It.IsAny<IDoneData>()), Times.Exactly(3));
		builder.Verify(static b => b.AddOnEntry(It.IsAny<IOnEntry>()), Times.Exactly(2));
		builder.Verify(static b => b.AddOnExit(It.IsAny<IOnExit>()), Times.Exactly(2));
	}

	[TestMethod]
	public void InitialBuilderCoversTransitionFactoryTargetOverloadsAndEnd()
	{
		var builder = new Mock<IInitialBuilder>();
		var builtInitial = Mock.Of<IInitial>();
		builder.Setup(static b => b.Build()).Returns(builtInitial);
		var transitionBuilder = new Mock<ITransitionBuilder>();
		transitionBuilder.Setup(static b => b.Build()).Returns(Mock.Of<ITransition>());
		var outer = new object();
		IInitial? captured = null;
		var fluent = new InitialFluentBuilder<object>
					 {
						 Builder = builder.Object,
						 BuiltAction = value => captured = value,
						 OuterBuilder = outer,
						 TransitionFluentBuilderFactory = (initial, built) => CreateTransitionBuilder(initial, built, transitionBuilder.Object)
					 };

		Assert.AreSame(fluent, fluent.BeginTransition().EndTransition());
		Assert.AreSame(fluent, fluent.AddTransition("target"));
		Assert.AreSame(fluent, fluent.AddTransition(Identifier.FromString("target-id")));
		Assert.AreSame(outer, fluent.EndInitial());
		Assert.AreSame(builtInitial, captured);
		builder.Verify(static b => b.SetTransition(It.IsAny<ITransition>()), Times.Exactly(3));
		transitionBuilder.Verify(static b => b.SetTarget(It.IsAny<ImmutableArray<IIdentifier>>()), Times.Exactly(2));
	}

	[TestMethod]
	public void HistoryBuilderCoversIdentityTypeTransitionsAndEnd()
	{
		var builder = new Mock<IHistoryBuilder>();
		var builtHistory = Mock.Of<IHistory>();
		builder.Setup(static b => b.Build()).Returns(builtHistory);
		var transitionBuilder = new Mock<ITransitionBuilder>();
		transitionBuilder.Setup(static b => b.Build()).Returns(Mock.Of<ITransition>());
		var outer = new object();
		IHistory? captured = null;
		var fluent = new HistoryFluentBuilder<object>
					 {
						 Builder = builder.Object,
						 BuiltAction = value => captured = value,
						 OuterBuilder = outer,
						 TransitionFluentBuilderFactory = (history, built) => CreateTransitionBuilder(history, built, transitionBuilder.Object)
					 };

		Assert.AreSame(fluent, fluent.SetId("history"));
		Assert.AreSame(fluent, fluent.SetId(Identifier.FromString("history-id")));
		Assert.AreSame(fluent, fluent.SetType(HistoryType.Shallow));
		Assert.AreSame(fluent, fluent.BeginTransition().EndTransition());
		Assert.AreSame(fluent, fluent.AddTransition("target"));
		Assert.AreSame(fluent, fluent.AddTransition(Identifier.FromString("target-id")));
		Assert.AreSame(outer, fluent.EndHistory());
		Assert.AreSame(builtHistory, captured);
		builder.Verify(static b => b.SetId(It.IsAny<IIdentifier>()), Times.Exactly(2));
		builder.Verify(static b => b.SetType(HistoryType.Shallow), Times.Once);
		builder.Verify(static b => b.SetTransition(It.IsAny<ITransition>()), Times.Exactly(3));
	}

	private static StateFluentBuilder<StateMachineFluentBuilder.StateMachineFluentBuilder> CreateStateBuilder(
		StateMachineFluentBuilder.StateMachineFluentBuilder outer, Action<IState> built, IStateBuilder builder) =>
		new()
		{
			Builder = builder,
			BuiltAction = built,
			OuterBuilder = outer,
			InitialFluentBuilderFactory = null!,
			StateFluentBuilderFactory = null!,
			ParallelFluentBuilderFactory = null!,
			FinalFluentBuilderFactory = null!,
			HistoryFluentBuilderFactory = null!,
			TransitionFluentBuilderFactory = null!
		};

	private static ParallelFluentBuilder<StateMachineFluentBuilder.StateMachineFluentBuilder> CreateParallelBuilder(
		StateMachineFluentBuilder.StateMachineFluentBuilder outer, Action<IParallel> built, IParallelBuilder builder) =>
		new()
		{
			Builder = builder,
			BuiltAction = built,
			OuterBuilder = outer,
			StateFluentBuilderFactory = null!,
			ParallelFluentBuilderFactory = null!,
			HistoryFluentBuilderFactory = null!,
			TransitionFluentBuilderFactory = null!
		};

	private static FinalFluentBuilder<StateMachineFluentBuilder.StateMachineFluentBuilder> CreateFinalBuilder(
		StateMachineFluentBuilder.StateMachineFluentBuilder outer, Action<IFinal> built, IFinalBuilder builder) =>
		new()
		{
			Builder = builder,
			BuiltAction = built,
			OuterBuilder = outer,
			ContentBuilderFactory = null!,
			DoneDataBuilderFactory = null!
		};

	private static TransitionFluentBuilder<TOuter> CreateTransitionBuilder<TOuter>(
		TOuter outer, Action<ITransition> built, ITransitionBuilder builder) where TOuter : notnull =>
		new() { Builder = builder, BuiltAction = built, OuterBuilder = outer };
}
