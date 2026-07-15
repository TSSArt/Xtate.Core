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

using Xtate.Interpreter;
using Xtate.DataModel;
using Xtate.Persistence;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class StateMachineReaderCoverageTest
{
	private static readonly string[] NullableRestoreMethods =
	[
		"RestoreStateMachine", "RestoreDataModel", "RestoreInitial", "RestoreTransition", "RestoreAssign", "RestoreCancel",
		"RestoreCompound", "RestoreCondition", "RestoreConditionExpression", "RestoreContent", "RestoreData", "RestoreDoneData",
		"RestoreElseIf", "RestoreElse", "RestoreEventDescriptor", "RestoreLog", "RestoreRaise", "RestoreScript", "RestoreCustomAction",
		"RestoreSend", "RestoreExternalDataExpression", "RestoreExternalScriptExpression", "RestoreFinalize", "RestoreFinal", "RestoreForEach",
		"RestoreHistory", "RestoreIdentifier", "RestoreIf", "RestoreInvoke", "RestoreLocationExpression", "RestoreOnEntry", "RestoreOnExit",
		"RestoreParallel", "RestoreParam", "RestoreScriptExpression", "RestoreState", "RestoreValueExpression"
	];

	[TestMethod]
	public void NullableRestoreMethodsReturnNullWhenTheirPersistedNodeIsAbsent()
	{
		var reader = new StateMachineReader();
		var empty = new Bucket(new InMemoryStorage(writeOnly: false));

		foreach (var methodName in NullableRestoreMethods)
		{
			Assert.IsNull(Invoke(reader, methodName, empty), methodName);
		}
	}

	[TestMethod]
	public void BuildAndDiscriminatedRestoresRejectMissingMismatchedAndUnknownTypes()
	{
		var reader = new StateMachineReader();
		var empty = new Bucket(new InMemoryStorage(writeOnly: false));
		Assert.ThrowsExactly<PersistenceException>([ExcludeFromCodeCoverage] () => reader.Build(empty));

		var mismatch = new Bucket(new InMemoryStorage(writeOnly: false));
		mismatch.Add(Key.TypeInfo, TypeInfo.StateNode);
		Assert.ThrowsExactly<PersistenceException>([ExcludeFromCodeCoverage] () => reader.Build(mismatch));

		var unknown = new Bucket(new InMemoryStorage(writeOnly: false));
		unknown.Add(Key.TypeInfo, TypeInfo.StateMachineNode);
		AssertInvocationPersistenceException(reader, "RestoreCondition", unknown);
		AssertInvocationPersistenceException(reader, "RestoreExecutableEntity", unknown);
		AssertInvocationPersistenceException(reader, "RestoreStateEntity", unknown);
	}

	[TestMethod]
	public void ForwardExecutableEntityRequiresMapEntryAndExecutableType()
	{
		var reader = new StateMachineReader();
		var bucket = new Bucket(new InMemoryStorage(writeOnly: false));
		bucket.Add(Key.DocumentId, 17);
		AssertInvocationPersistenceException(reader, "ForwardExecEntity", bucket);

		var map = new StubEntityMap(found: false, entity: null);
		SetEntityMap(reader, map);
		AssertInvocationPersistenceException(reader, "ForwardExecEntity", bucket);

		map.Found = true;
		map.Entity = Mock.Of<IEntity>();
		AssertInvocationPersistenceException(reader, "ForwardExecEntity", bucket);

		var executable = Mock.Of<IExecutableEntity>();
		map.Entity = executable;
		Assert.AreSame(executable, Invoke(reader, "ForwardExecEntity", bucket));
	}

	[TestMethod]
	public void ExternalScriptExpressionRestoresEmbeddedContentAndAncestorMetadata()
	{
		var reader = new StateMachineReader();
		var bucket = new Bucket(new InMemoryStorage(writeOnly: false));
		bucket.Add(Key.TypeInfo, TypeInfo.ExternalScriptExpressionNode);
		bucket.Add(Key.DocumentId, 19);
		bucket.Add(Key.Uri, new Uri("https://example.test/script.js"));
		bucket.Add(Key.Content, "embedded script");

		var restored = Invoke(reader, "RestoreExternalScriptExpression", bucket);

		Assert.IsInstanceOfType<IExternalScriptExpression>(restored);
		Assert.IsInstanceOfType<IExternalScriptProvider>(restored);
		Assert.AreEqual(new Uri("https://example.test/script.js"), ((IExternalScriptExpression) restored).Uri);
		Assert.AreEqual("embedded script", ((IExternalScriptProvider) restored).Content);
	}

	[TestMethod]
	public void SendAndInvokeRestoreTheirPersistedScalarValues()
	{
		var reader = new StateMachineReader();
		var sendBucket = new Bucket(new InMemoryStorage(writeOnly: false));
		sendBucket.Add(Key.TypeInfo, TypeInfo.SendNode);
		sendBucket.Add(Key.DocumentId, 23);
		sendBucket.Add(Key.Id, "send-id");
		sendBucket.Add(Key.Type, new FullUri("https://example.test/send"));
		sendBucket.Add(Key.Event, "event.name");
		sendBucket.Add(Key.Target, new FullUri("https://example.test/target"));
		sendBucket.Add(Key.DelayMs, 125);
		sendBucket.Add(Key.NameList, 0);
		sendBucket.Add(Key.Parameters, 0);
		AddContent(sendBucket.Nested(Key.Content), "send content");

		var send = Assert.IsInstanceOfType<ISend>(Invoke(reader, "RestoreSend", sendBucket));
		Assert.AreEqual("send-id", send.Id);
		Assert.AreEqual(new FullUri("https://example.test/send"), send.Type);
		Assert.AreEqual("event.name", send.EventName);
		Assert.AreEqual(new FullUri("https://example.test/target"), send.Target);
		Assert.AreEqual(125, send.DelayMs);
		Assert.IsTrue(send.NameList.IsEmpty);
		Assert.IsTrue(send.Parameters.IsEmpty);
		Assert.AreEqual("send content", send.Content?.Body?.Value);

		var invokeBucket = new Bucket(new InMemoryStorage(writeOnly: false));
		invokeBucket.Add(Key.TypeInfo, TypeInfo.InvokeNode);
		invokeBucket.Add(Key.DocumentId, 29);
		invokeBucket.Add(Key.Id, "invoke-id");
		invokeBucket.Add(Key.Type, new FullUri("https://example.test/invoke"));
		invokeBucket.Add(Key.Source, new Uri("https://example.test/source"));
		invokeBucket.Add(Key.AutoForward, true);
		invokeBucket.Add(Key.NameList, 0);
		invokeBucket.Add(Key.Parameters, 0);
		var finalizeBucket = invokeBucket.Nested(Key.Finalize);
		finalizeBucket.Add(Key.TypeInfo, TypeInfo.FinalizeNode);
		finalizeBucket.Add(Key.Parameters, 0);
		AddContent(invokeBucket.Nested(Key.Content), "invoke content");

		var invoke = Assert.IsInstanceOfType<IInvoke>(Invoke(reader, "RestoreInvoke", invokeBucket));
		Assert.AreEqual("invoke-id", invoke.Id);
		Assert.AreEqual(new FullUri("https://example.test/invoke"), invoke.Type);
		Assert.AreEqual(new Uri("https://example.test/source"), invoke.Source);
		Assert.IsTrue(invoke.AutoForward);
		Assert.IsTrue(invoke.NameList.IsEmpty);
		Assert.IsTrue(invoke.Parameters.IsEmpty);
		Assert.IsNotNull(invoke.Finalize);
		Assert.IsTrue(invoke.Finalize.Action.IsEmpty);
		Assert.AreEqual("invoke content", invoke.Content?.Body?.Value);
	}

	[TestMethod]
	public void DoneDataRestoresPresentNodeWithContentAndParameters()
	{
		var bucket = new Bucket(new InMemoryStorage(writeOnly: false));
		bucket.Add(Key.TypeInfo, TypeInfo.DoneDataNode);
		bucket.Add(Key.Parameters, 0);
		AddContent(bucket.Nested(Key.Content), "done content");

		var doneData = Assert.IsInstanceOfType<IDoneData>(Invoke(new StateMachineReader(), "RestoreDoneData", bucket));
		Assert.AreEqual("done content", doneData.Content?.Body?.Value);
		Assert.IsTrue(doneData.Parameters.IsEmpty);
	}

	private static void AddContent(Bucket bucket, string body)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.ContentNode);
		bucket.Add(Key.Body, body);
	}

	private static object? Invoke(StateMachineReader reader, string methodName, Bucket bucket)
	{
		var method = typeof(StateMachineReader).GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

		return method.Invoke(method.IsStatic ? null : reader, [bucket]);
	}

	private static void AssertInvocationPersistenceException(StateMachineReader reader, string methodName, Bucket bucket)
	{
		try
		{
			_ = Invoke(reader, methodName, bucket);
			Assert.Fail($"{methodName} did not throw.");
		}
		catch (System.Reflection.TargetInvocationException exception)
		{
			Assert.IsInstanceOfType<PersistenceException>(exception.InnerException);
		}
	}

	private static void SetEntityMap(StateMachineReader reader, IEntityMap map) =>
		typeof(StateMachineReader).GetField("_forwardEntities", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(reader, map);

	private sealed class StubEntityMap(bool found, IEntity? entity) : IEntityMap
	{
		public bool Found { get; set; } = found;

		public IEntity? Entity { get; set; } = entity;

		public bool TryGetEntityByDocumentId(int id, [NotNullWhen(true)] out IEntity? entityResult)
		{
			entityResult = Entity;

			return Found;
		}
	}
}
