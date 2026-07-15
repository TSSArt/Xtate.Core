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

using System.Collections;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Services;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterUtilityCoverageTest
{
	[TestMethod]
	public void InvokeIdSetTracksUniqueIdsRaisesOnlyRealChangesAndEnumeratesBothWays()
	{
		var first = InvokeId.FromString("first", "unique-one");
		var sameUniqueId = InvokeId.FromString("alias", "unique-one");
		var second = InvokeId.FromString("second", "unique-two");
		var set = new InvokeIdSet();
		var changes = new List<(InvokeIdSet.ChangedAction Action, InvokeId Id)>();
		set.Changed += (action, id) => changes.Add((action, id));

		set.Add(first);
		set.Add(sameUniqueId);
		set.Add(second);
		set.Remove(sameUniqueId);
		set.Remove(first);

		Assert.AreEqual(expected: 1, set.Count);
		Assert.IsFalse(set.Contains(first));
		Assert.IsTrue(set.Contains(second));
		Assert.AreEqual(expected: 3, changes.Count);
		Assert.AreEqual(InvokeIdSet.ChangedAction.Add, changes[0].Action);
		Assert.AreEqual(InvokeIdSet.ChangedAction.Add, changes[1].Action);
		Assert.AreEqual(InvokeIdSet.ChangedAction.Remove, changes[2].Action);
		Assert.AreSame(second.UniqueId, set.Single());
		Assert.AreEqual(expected: 1, ((IEnumerable) set).Cast<object>().Count());
		var interfaceMap = typeof(InvokeIdSet).GetInterfaceMap(typeof(IEnumerable));
		var enumerator = (IEnumerator) interfaceMap.TargetMethods.Single().Invoke(set, parameters: null)!;
		Assert.IsTrue(enumerator.MoveNext());
		Assert.AreSame(second.UniqueId, enumerator.Current);
	}

	[TestMethod]
	public void OutgoingEventVerboseParserHandlesUndefinedHandlerAndFallbackConversions()
	{
		var handler = new Mock<IDataModelHandler>();
		handler.Setup(static h => h.ConvertToText(It.IsAny<DataModelValue>())).Returns("converted-outgoing");
		var parser = new OutgoingEventVerboseEntityParser { DataModelHandler = () => handler.Object };
		var contract = (IEntityParserHandler) parser;

		Assert.AreEqual(Level.Verbose, contract.Level);
		Assert.IsEmpty(contract.EnumerateProperties(Mock.Of<IOutgoingEvent>(static value => value.Data == DataModelValue.Undefined))!);
		var populated = contract.EnumerateProperties(Mock.Of<IOutgoingEvent>(static value => value.Data == new DataModelValue(17D)))!.ToArray();
		Assert.HasCount(expected: 2, populated);
		Assert.AreEqual("Data", populated[0].Name);
		Assert.AreEqual("DataText", populated[1].Name);
		Assert.AreEqual("converted-outgoing", populated[1].Value);

		var fallback = new OutgoingEventVerboseEntityParser { DataModelHandler = static () => null };
		var fallbackValues = ((IEntityParserHandler) fallback)
			.EnumerateProperties(Mock.Of<IOutgoingEvent>(static value => value.Data == new DataModelValue(false)))!.ToArray();
		Assert.AreEqual("False", fallbackValues[1].Value);
	}

	[TestMethod]
	public void InvokeDataVerboseParserEnumeratesRawContentContentAndParametersWithBothConversions()
	{
		var handler = new Mock<IDataModelHandler>();
		handler.Setup(static h => h.ConvertToText(It.IsAny<DataModelValue>())).Returns<DataModelValue>(static value => $"converted:{value}");
		var parser = new InvokeDataVerboseEntityParser { DataModelHandler = () => handler.Object };
		var contract = (IEntityParserHandler) parser;
		var invokeData = new InvokeData(
			InvokeId.FromString("invoke"),
			new FullUri("service"),
			Source: null,
			RawContent: "raw",
			Content: new DataModelValue("content"),
			Parameters: new DataModelValue(17D));

		var properties = contract.EnumerateProperties(invokeData)!.ToArray();

		Assert.AreEqual(Level.Verbose, contract.Level);
		Assert.HasCount(expected: 5, properties);
		CollectionAssert.AreEqual(new[] { "RawContent", "Content", "ContentText", "Parameters", "ParametersText" }, properties.Select(static property => property.Name).ToArray());
		Assert.AreEqual("converted:content", properties[2].Value);
		Assert.AreEqual("converted:17", properties[4].Value);

		var fallback = new InvokeDataVerboseEntityParser { DataModelHandler = static () => null };
		var fallbackProperties = ((IEntityParserHandler) fallback).EnumerateProperties(invokeData with { RawContent = null, Parameters = DataModelValue.Undefined })!.ToArray();
		Assert.HasCount(expected: 2, fallbackProperties);
		Assert.AreEqual("content", fallbackProperties[1].Value);
		Assert.IsEmpty(contract.EnumerateProperties(invokeData with { RawContent = null, Content = DataModelValue.Undefined, Parameters = DataModelValue.Undefined })!);
	}
}
