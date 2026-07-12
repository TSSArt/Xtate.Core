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

using Xtate;
using Xtate.Interpreter.Services;
using Xtate.Interpreter.Internal;
using Xtate.IoC;
using Xtate.StateMachine.Internal;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class SmallHelperCoverageTest
{
	[TestMethod]
	public void SpanFormattableStringExtensionsCopyNullEmptySuccessfulAndInsufficientDestinationCases()
	{
		var buffer = new char[2];
		var destination = buffer.AsSpan();
		var charsWritten = 0;
		string? nullString = null;

		Assert.IsTrue(nullString.TryCopyIncremental(ref destination, ref charsWritten));
		Assert.AreEqual(0, charsWritten);
		Assert.AreEqual(2, destination.Length);
		Assert.IsTrue("ab".TryCopyIncremental(ref destination, ref charsWritten));
		CollectionAssert.AreEqual(new[] { 'a', 'b' }, buffer);
		Assert.AreEqual(2, charsWritten);
		Assert.AreEqual(0, destination.Length);
		Assert.IsFalse("c".TryCopyIncremental(ref destination, ref charsWritten));

		Assert.IsTrue(string.Empty.TryCopyTo(stackalloc char[0], out var emptyCharsWritten));
		Assert.AreEqual(0, emptyCharsWritten);
		Assert.IsFalse("abc".TryCopyTo(stackalloc char[2], out var failedCharsWritten));
		Assert.AreEqual(0, failedCharsWritten);
		Assert.IsTrue("xy".TryCopyTo(stackalloc char[2], out var copiedCharsWritten));
		Assert.AreEqual(2, copiedCharsWritten);
	}

	[TestMethod]
	public void SpanFormattableCharExtensionCopiesWhenDestinationHasSpaceOnly()
	{
		var buffer = new char[1];
		var destination = buffer.AsSpan();
		var charsWritten = 0;

		Assert.IsTrue('x'.TryCopyIncremental(ref destination, ref charsWritten));
		Assert.AreEqual('x', buffer[0]);
		Assert.AreEqual(1, charsWritten);
		Assert.AreEqual(0, destination.Length);
		Assert.IsFalse('y'.TryCopyIncremental(ref destination, ref charsWritten));
		Assert.AreEqual(1, charsWritten);
	}

	[TestMethod]
	public void DocumentIdSlotReturnsNegativeUntilPositiveNodeValueIsCached()
	{
		var emptySlot = new DocumentIdSlot(node: null);
		var negativeList = new LinkedList<int>();
		var negativeNode = negativeList.AddLast(-5);
		var negativeSlot = new DocumentIdSlot(negativeNode);
		var positiveList = new LinkedList<int>();
		var positiveNode = positiveList.AddLast(42);
		var positiveSlot = new DocumentIdSlot(positiveNode);

		Assert.AreEqual(-1, emptySlot.CreateValue());
		Assert.AreEqual(-1, emptySlot.CreateValue());
		Assert.AreEqual(-1, negativeSlot.CreateValue());
		Assert.AreEqual(-1, negativeSlot.CreateValue());
		Assert.AreEqual(42, positiveSlot.CreateValue());
		positiveNode.Value = 7;
		Assert.AreEqual(42, positiveSlot.CreateValue());
	}

	[TestMethod]
	public async Task ForwardFactoryPropagatesMissingServiceException()
	{
		var serviceProvider = new Mock<Xtate.IoC.IServiceProvider>();
		serviceProvider.Setup(sp => sp.GetImplementationEntry(It.IsAny<TypeKey>())).Returns((ImplementationEntry?) null);
		var forward = Forward<object, string>.To<string>();

		await Assert.ThrowsExactlyAsync<MissedServiceException>(async () => await forward(serviceProvider.Object, "arg"));
	}

	[TestMethod]
	public void IdGeneratorCreatesDebugFormattedIdsForAllPublicFactories()
	{
		Assert.AreEqual("0000002A", IdGenerator.NewSendId(42));
		Assert.AreEqual("0000002A", IdGenerator.NewSessionId(42));
		Assert.AreEqual("0000002A", IdGenerator.NewInvokeUniqueId(42));
		Assert.AreEqual("0000002A", IdGenerator.NewId(42));
		Assert.AreEqual("state.0000002A", IdGenerator.NewInvokeId("state", 42));
		Assert.AreEqual("state.FFFFFFFF", IdGenerator.NewInvokeId("state", -1));
	}

	[TestMethod]
	public void AssemblyTypeInfoExposesTypeAssemblyAndVersion()
	{
		var typeInfo = new AssemblyTypeInfo(typeof(SmallHelperCoverageTest));

		Assert.AreEqual(typeof(SmallHelperCoverageTest).FullName, typeInfo.FullTypeName);
		Assert.AreEqual(typeof(SmallHelperCoverageTest).Assembly.GetName().Name, typeInfo.AssemblyName);
		Assert.IsNotNull(typeInfo.AssemblyVersion);
	}
}
