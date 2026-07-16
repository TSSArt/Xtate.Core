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
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.IoC.ServiceArray;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class CustomActionCoverageTest
{
	[TestMethod]
	public async Task ContainerFiltersExpressionsSetsEvaluatorsForwardsMetadataAndExecutesAction()
	{
		var includedValue = new ActionValue { Expression = "value" };
		var excludedValue = new ActionValue { Expression = null };
		var includedLocation = new ActionLocation { Expression = "location" };
		var excludedLocation = new ActionLocation { Expression = null };
		var action = new TestAction([includedValue, excludedValue], [includedLocation, excludedLocation]);
		var source = new CustomActionSource
					 {
						 XmlNamespace = "urn:actions",
						 XmlName = "run",
						 Xml = "<run/>",
						 Locations = default,
						 Values = default
					 };
		var container = new CustomActionContainer(source, _ => action);

		Assert.AreSame(source, ((IAncestorProvider) container).Ancestor);
		Assert.AreEqual(expected: "urn:actions", container.XmlNamespace);
		Assert.AreEqual(expected: "run", container.XmlName);
		Assert.AreEqual(expected: "<run/>", container.Xml);
		Assert.HasCount(expected: 1, container.Values);
		Assert.AreSame(includedValue, container.Values[0]);
		Assert.HasCount(expected: 1, container.Locations);
		Assert.AreSame(includedLocation, container.Locations[0]);

		container.SetEvaluators(container.Values, container.Locations);
		Assert.AreSame(includedValue, includedValue.Evaluator);
		Assert.AreSame(includedLocation, includedLocation.Evaluator);

		await container.Execute();
		Assert.AreEqual(expected: 1, action.ExecuteCount);
	}

	[TestMethod]
	public void FactorySkipsNonmatchingProvidersActivatesMatchAndUsesSyncListBuilder()
	{
		var source = new CustomActionSource
					 {
						 XmlNamespace = "urn:actions",
						 XmlName = "run",
						 Xml = "<run id='1'/>",
						 Locations = default,
						 Values = default
					 };
		var expectedAction = new TestAction([], []);
		var first = new Mock<IActionProvider>();
		first.Setup(static p => p.TryGetActivator("urn:actions", "run")).Returns((IActionActivator?) null);
		var activator = new Mock<IActionActivator>();
		activator.Setup(static a => a.Activate("<run id='1'/>")).Returns(expectedAction);
		var matching = new Mock<IActionProvider>();
		matching.Setup(static p => p.TryGetActivator("urn:actions", "run")).Returns(activator.Object);
		ISyncList<IActionProvider> providers = [first.Object, matching.Object];
		var factory = new CustomActionFactory { ActionProviders = providers };

		var action = factory.GetAction(source);

		Assert.AreSame(expectedAction, action);
		Assert.AreEqual(expected: 2, providers.Count);
		first.Verify(static p => p.TryGetActivator("urn:actions", "run"), Times.Once);
		matching.Verify(static p => p.TryGetActivator("urn:actions", "run"), Times.Once);
		activator.Verify(static a => a.Activate("<run id='1'/>"), Times.Once);
	}

	private sealed class CustomActionSource : ICustomAction
	{
	#region Interface ICustomAction

		public string? XmlNamespace { get; init; }

		public string? XmlName { get; init; }

		public string? Xml { get; init; }

		public ImmutableArray<ILocationExpression> Locations { get; init; }

		public ImmutableArray<IValueExpression> Values { get; init; }

	#endregion
	}

	private sealed class TestAction(IEnumerable<IActionValue> values, IEnumerable<IActionLocation> locations) : IAction
	{
		public int ExecuteCount { get; private set; }

	#region Interface IAction

		public IEnumerable<IActionValue> GetValues() => values;

		public IEnumerable<IActionLocation> GetLocations() => locations;

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class ActionValue : IActionValue, IValueEvaluator
	{
		public IValueEvaluator? Evaluator { get; private set; }

	#region Interface IActionValue

		public void SetEvaluator(IValueEvaluator valueEvaluator) => Evaluator = valueEvaluator;

	#endregion

	#region Interface IValueExpression

		public string? Expression { get; init; }

	#endregion
	}

	private sealed class ActionLocation : IActionLocation, ILocationEvaluator
	{
		public ILocationEvaluator? Evaluator { get; private set; }

	#region Interface IActionLocation

		public void SetEvaluator(ILocationEvaluator locationEvaluator) => Evaluator = locationEvaluator;

	#endregion

	#region Interface ILocationEvaluator

		public ValueTask SetValue(IObject value) => ValueTask.CompletedTask;

		public ValueTask<IObject> GetValue() => new(DataModelValue.Undefined);

		public ValueTask<string> GetName() => new("location");

	#endregion

	#region Interface ILocationExpression

		public string? Expression { get; init; }

	#endregion
	}
}