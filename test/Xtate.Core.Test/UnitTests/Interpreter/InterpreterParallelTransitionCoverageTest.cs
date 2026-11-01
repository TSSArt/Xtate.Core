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


using System.Threading;
using Xtate.Class;
using Xtate.DataTypes;
using Xtate.IoC;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;

namespace Xtate.Core.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterParallelTransitionCoverageTest
{
	[TestMethod]
	public async Task DrainingALargeInternalQueueIsProgressRatherThanLiveLock()
	{
		var events = string.Concat(Enumerable.Repeat("<raise event='unused'/>", 80));
		var source = "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'><state id='work'><onentry>" +
			events + "<raise event='finish'/></onentry><transition event='finish' target='done'/></state>" +
			"<final id='done'><donedata><content>complete</content></donedata></final></scxml>";
		Assert.AreEqual("complete", (await Execute(source)).AsString());
	}
	[TestMethod]
	public async Task LateBindingInitializesStateDataBeforeStateExecution()
	{
		const string source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" binding="late">
			  <state id="working"><datamodel><data id="local" expr="'initialized'"/></datamodel><transition target="done"/></state>
			  <final id="done"><donedata><content expr="$local"/></donedata></final>
			</scxml>
			""";
		Assert.AreEqual("initialized", (await Execute(source)).AsString());
	}

	[TestMethod]
	public async Task NestedParallelCompletionRequiresEveryRegionToReachFinalState()
	{
		const string source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			  <parallel id="outer">
			    <state id="left"><final id="leftFinal"/></state>
			    <parallel id="right">
			      <state id="top"><final id="topFinal"/></state>
			      <state id="bottom"><final id="bottomFinal"/></state>
			    </parallel>
			    <transition event="done.state.outer" target="done"/>
			  </parallel>
			  <final id="done"><donedata><content>complete</content></donedata></final>
			</scxml>
			""";
		Assert.AreEqual("complete", (await Execute(source)).AsString());
	}

	[TestMethod]
	public async Task ThreeLevelNestedParallelCompletionPropagatesToTheOutermostParallel()
	{
		const string source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			  <parallel id="outer">
			    <parallel id="middle">
			      <parallel id="inner">
			        <state id="a"><final id="aFinal"/></state>
			        <state id="b"><final id="bFinal"/></state>
			      </parallel>
			    </parallel>
			    <transition event="done.state.outer" target="done"/>
			  </parallel>
			  <final id="done"><donedata><content>complete</content></donedata></final>
			</scxml>
			""";
		Assert.AreEqual("complete", (await Execute(source)).AsString());
	}

	[TestMethod]
	public async Task OuterParallelIsNotCompletedWhileAnotherRegionIsUnfinished()
	{
		const string source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			  <parallel id="outer">
			    <state id="pending"><state id="idle"/></state>
			    <parallel id="right">
			      <state id="top"><final id="topFinal"/></state>
			      <state id="bottom"><final id="bottomFinal"/></state>
			    </parallel>
			    <transition event="done.state.right"><raise event="later"/></transition>
			    <transition event="done.state.outer" target="wrong"/>
			    <transition event="later" target="correct"/>
			  </parallel>
			  <final id="wrong"><donedata><content>wrong</content></donedata></final>
			  <final id="correct"><donedata><content>correct</content></donedata></final>
			</scxml>
			""";
		Assert.AreEqual("correct", (await Execute(source)).AsString());
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task DescendantTransitionPreemptsConflictingAncestorRegardlessOfRegionOrder(bool descendantFirst)
	{
		const string descendant = """
			<state id="region"><state id="before"><transition event="go" target="after"/></state>
			<state id="after"><onentry><raise event="finish"/></onentry></state></state>
			""";
		const string other = "<state id='other'><state id='idle'/></state>";
		var regions = descendantFirst ? descendant + other : other + descendant;
		var source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			<parallel id="parallel"><onentry><raise event="go"/></onentry>
			""" + regions + """
			<transition event="go" target="wrong"/><transition event="finish" target="correct"/>
			</parallel><final id="wrong"><donedata><content>wrong</content></donedata></final>
			<final id="correct"><donedata><content>correct</content></donedata></final></scxml>
			""";
		Assert.AreEqual("correct", (await Execute(source)).AsString());
	}

	[TestMethod]
	public async Task TargetingOneParallelDescendantEntersOtherRegionsUsingTheirInitialStates()
	{
		const string source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
			<state id="start"><transition target="selected"/></state>
			<parallel id="parallel">
			  <state id="left"><state id="unused"/><state id="selected"><transition event="other.entered" target="correct"/></state></state>
			  <state id="right"><state id="other"><onentry><raise event="other.entered"/></onentry></state></state>
			</parallel>
			<final id="correct"><donedata><content>correct</content></donedata></final>
			</scxml>
			""";
		Assert.AreEqual("correct", (await Execute(source)).AsString());
	}

	[TestMethod]
	public async Task OuterParallelCompletesWhenDirectRegionFinishesAfterNestedRegions()
	{
		const string source = """
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			  <parallel id="outer">
			    <parallel id="nested"><state id="top"><final id="topFinal"/></state><state id="bottom"><final id="bottomFinal"/></state></parallel>
			    <state id="last"><final id="lastFinal"/></state>
			    <transition event="done.state.outer" target="done"/>
			  </parallel>
			  <final id="done"><donedata><content>complete</content></donedata></final>
			</scxml>
			""";
		Assert.AreEqual("complete", (await Execute(source)).AsString());
	}

	private static async Task<DataModelValue> Execute(string source)
	{
		await using var container = Container.Create<StateMachineProcessorModule>();
		var manager = await container.GetRequiredService<IStateMachineScopeManager>();
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		return await manager.Execute(new ScxmlStringStateMachine(source), SecurityContextType.NewStateMachine).AsTask().WaitAsync(timeout.Token);
	}
}
