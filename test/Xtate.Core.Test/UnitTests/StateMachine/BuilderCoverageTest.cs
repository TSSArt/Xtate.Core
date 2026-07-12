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

using Xtate.Ancestor;
using Xtate.StateMachine;
using Xtate.StateMachine.Builder.Services;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class BuilderCoverageTest
{
	[TestMethod]
	public void AssignBuilderBuildsConfiguredAndDefaultEntities()
	{
		var ancestor = new object();
		var location = Mock.Of<ILocationExpression>();
		var expression = Mock.Of<IValueExpression>();
		var inlineContent = Mock.Of<IInlineContent>();
		var builder = new AssignBuilder { Ancestor = ancestor };

		builder.SetLocation(location);
		builder.SetExpression(expression);
		builder.SetInlineContent(inlineContent);
		builder.SetType("replacechildren");
		builder.SetAttribute("attribute");
		var assign = builder.Build();

		Assert.AreSame(ancestor, ((IAncestorProvider) assign).Ancestor);
		Assert.AreSame(location, assign.Location);
		Assert.AreSame(expression, assign.Expression);
		Assert.AreSame(inlineContent, assign.InlineContent);
		Assert.AreEqual(expected: "replacechildren", assign.Type);
		Assert.AreEqual(expected: "attribute", assign.Attribute);

		var empty = new AssignBuilder().Build();
		Assert.IsNull(empty.Location);
		Assert.IsNull(empty.Expression);
		Assert.IsNull(empty.InlineContent);
		Assert.IsNull(empty.Type);
		Assert.IsNull(empty.Attribute);
	}

	[TestMethod]
	public void AssignBuilderRejectsNullConfigurationValues()
	{
		var builder = new AssignBuilder();

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetLocation(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetExpression(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetInlineContent(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetType(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetAttribute(null!));
	}

	[TestMethod]
	public void FinalizeBuilderBuildsActionsAndDefaultCollection()
	{
		var ancestor = new object();
		var action = Mock.Of<IExecutableEntity>();
		var builder = new FinalizeBuilder { Ancestor = ancestor };

		builder.AddAction(action);
		var finalize = builder.Build();

		Assert.AreSame(ancestor, ((IAncestorProvider) finalize).Ancestor);
		Assert.AreSame(action, finalize.Action.Single());
		Assert.IsTrue(new FinalizeBuilder().Build().Action.IsDefault);
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddAction(null!));
	}

	[TestMethod]
	public void ForEachBuilderBuildsConfiguredAndDefaultEntities()
	{
		var ancestor = new object();
		var array = Mock.Of<IValueExpression>();
		var item = Mock.Of<ILocationExpression>();
		var index = Mock.Of<ILocationExpression>();
		var action = Mock.Of<IExecutableEntity>();
		var builder = new ForEachBuilder { Ancestor = ancestor };

		builder.SetArray(array);
		builder.SetItem(item);
		builder.SetIndex(index);
		builder.AddAction(action);
		var forEach = builder.Build();

		Assert.AreSame(ancestor, ((IAncestorProvider) forEach).Ancestor);
		Assert.AreSame(array, forEach.Array);
		Assert.AreSame(item, forEach.Item);
		Assert.AreSame(index, forEach.Index);
		Assert.AreSame(action, forEach.Action.Single());
		Assert.IsTrue(new ForEachBuilder().Build().Action.IsDefault);
	}

	[TestMethod]
	public void ForEachBuilderRejectsNullConfigurationValues()
	{
		var builder = new ForEachBuilder();

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetArray(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetItem(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetIndex(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddAction(null!));
	}

	[TestMethod]
	public void IfBuilderBuildsConfiguredAndDefaultEntities()
	{
		var ancestor = new object();
		var condition = Mock.Of<IConditionExpression>();
		var action = Mock.Of<IExecutableEntity>();
		var builder = new IfBuilder { Ancestor = ancestor };

		builder.SetCondition(condition);
		builder.AddAction(action);
		var @if = builder.Build();

		Assert.AreSame(ancestor, ((IAncestorProvider) @if).Ancestor);
		Assert.AreSame(condition, @if.Condition);
		Assert.AreSame(action, @if.Action.Single());
		Assert.IsTrue(new IfBuilder().Build().Action.IsDefault);
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetCondition(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddAction(null!));
	}

	[TestMethod]
	public void ParallelBuilderBuildsDefaultAndPopulatedCollections()
	{
		var ancestor = new object();
		var id = Mock.Of<IIdentifier>();
		var state = Mock.Of<IState>();
		var parallelChild = Mock.Of<IParallel>();
		var history = Mock.Of<IHistory>();
		var transition = Mock.Of<ITransition>();
		var onEntry = Mock.Of<IOnEntry>();
		var onExit = Mock.Of<IOnExit>();
		var invoke = Mock.Of<IInvoke>();
		var dataModel = Mock.Of<IDataModel>();
		var builder = new ParallelBuilder { Ancestor = ancestor };

		builder.SetId(id);
		builder.AddState(state);
		builder.AddParallel(parallelChild);
		builder.AddHistory(history);
		builder.AddTransition(transition);
		builder.AddOnEntry(onEntry);
		builder.AddOnExit(onExit);
		builder.AddInvoke(invoke);
		builder.SetDataModel(dataModel);
		var parallel = builder.Build();

		Assert.AreSame(ancestor, ((IAncestorProvider) parallel).Ancestor);
		Assert.AreSame(id, parallel.Id);
		Assert.AreSequenceEqual(new IStateEntity[] { state, parallelChild }, parallel.States);
		Assert.AreSame(history, parallel.HistoryStates.Single());
		Assert.AreSame(transition, parallel.Transitions.Single());
		Assert.AreSame(onEntry, parallel.OnEntry.Single());
		Assert.AreSame(onExit, parallel.OnExit.Single());
		Assert.AreSame(invoke, parallel.Invoke.Single());
		Assert.AreSame(dataModel, parallel.DataModel);

		var empty = new ParallelBuilder().Build();
		Assert.IsTrue(empty.States.IsDefault);
		Assert.IsTrue(empty.HistoryStates.IsDefault);
		Assert.IsTrue(empty.Transitions.IsDefault);
		Assert.IsTrue(empty.OnEntry.IsDefault);
		Assert.IsTrue(empty.OnExit.IsDefault);
		Assert.IsTrue(empty.Invoke.IsDefault);
	}

	[TestMethod]
	public void ParallelBuilderRejectsNullConfigurationValues()
	{
		var builder = new ParallelBuilder();

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetId(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddState(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddParallel(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddHistory(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddTransition(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddOnEntry(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddOnExit(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.AddInvoke(null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => builder.SetDataModel(null!));
	}
}