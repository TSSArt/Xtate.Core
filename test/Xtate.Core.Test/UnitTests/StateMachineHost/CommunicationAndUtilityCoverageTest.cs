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
using System.Threading;
using System.Xml;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.ExternalServices;
using Xtate.Interpreter;
using Xtate.IoC.ServiceArray.Services;
using Xtate.IoC.Tools;
using Xtate.Scxml.Internal;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class CommunicationAndUtilityCoverageTest
{
	[TestMethod]
	public async Task ExternalCommunicationRoutesInternalScheduledSentCancelAndInvalidTypePaths()
	{
		using var disposingToken = new DisposingToken();
		var router = new Mock<IEventRouter>();
		var scheduler = new Mock<IEventScheduler>();
		var internalEvent = CreateOutgoingEvent(target: "internal", delayMs: 0);
		var immediateEvent = CreateOutgoingEvent(target: "external", delayMs: 0);
		var delayedEvent = CreateOutgoingEvent(target: "external", delayMs: 5);
		var routerEvent = new RouterEvent(InvokeId.FromString("sender"), originType: null, origin: null, immediateEvent);

		router.Setup(static r => r.CanHandle(new FullUri("processor"))).Returns(true);
		router.Setup(static r => r.IsInternalTarget(new FullUri("internal"))).Returns(true);
		router.Setup(static r => r.GetRouterEvent(It.IsAny<IOutgoingEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(routerEvent);

		var communication = new ExternalCommunication
							{
								EventRouters = [router.Object],
								EventScheduler = scheduler.Object,
								DisposeToken = new DisposeToken(disposingToken.Token)
							};

		Assert.AreEqual(SendStatus.ToInternalQueue, await communication.TrySend(internalEvent));
		Assert.AreEqual(SendStatus.Sent, await communication.TrySend(immediateEvent));
		Assert.AreEqual(SendStatus.Scheduled, await communication.TrySend(delayedEvent));

		await communication.Cancel(SendId.FromString("send-0000002a")!);

		router.Verify(static r => r.Dispatch(It.IsAny<IRouterEvent>(), It.IsAny<CancellationToken>()), Times.Once);
		scheduler.Verify(static s => s.ScheduleEvent(It.IsAny<IRouterEvent>(), It.IsAny<CancellationToken>()), Times.Once);
		scheduler.Verify(static s => s.CancelEvent(It.IsAny<SendId>(), It.IsAny<CancellationToken>()), Times.Once);

		var invalidEvent = CreateOutgoingEvent(type: "unknown", target: "external", delayMs: 0);
		Assert.ThrowsExactly<ProcessorException>([ExcludeFromCodeCoverage]() => communication.TrySend(invalidEvent).AsTask().GetAwaiter().GetResult());
	}

	[TestMethod]
	public void RouterEventCopiesOutgoingEventAndRoutingFields()
	{
		var sender = InvokeId.FromString("sender");
		var outgoingEvent = CreateOutgoingEvent(type: "processor", target: "target", delayMs: 123);
		var routerEvent = new RouterEvent(sender, new FullUri("originType"), new FullUri("origin"), outgoingEvent);

		Assert.AreSame(sender, routerEvent.SenderServiceId);
		Assert.AreSame(sender, routerEvent.InvokeId);
		Assert.AreEqual(EventType.External, routerEvent.Type);
		Assert.AreEqual(outgoingEvent.DelayMs, routerEvent.DelayMs);
		Assert.AreEqual(outgoingEvent.Type, routerEvent.TargetType);
		Assert.AreEqual(outgoingEvent.Target, routerEvent.Target);
		Assert.AreEqual(outgoingEvent.Name, routerEvent.Name);
		Assert.AreEqual(outgoingEvent.SendId, routerEvent.SendId);
		Assert.AreEqual(expected: "payload", routerEvent.Data.AsString());
		Assert.AreEqual(new FullUri("originType"), routerEvent.OriginType);
		Assert.AreEqual(new FullUri("origin"), routerEvent.Origin);
	}

	[TestMethod]
	public async Task DefaultScriptEvaluatorUsesContentFirstAndThenSourceEvaluator()
	{
		var contentEvaluator = new ScriptPart("content");
		var sourceEvaluator = new ScriptPart("source");
		var contentScript = new ScriptSource { Content = contentEvaluator, Source = sourceEvaluator };
		var sourceOnlyScript = new ScriptSource { Source = sourceEvaluator };

		var contentDefaultEvaluator = new DefaultScriptEvaluator(contentScript);
		var sourceDefaultEvaluator = new DefaultScriptEvaluator(sourceOnlyScript);

		Assert.AreSame(contentScript, ((IAncestorProvider) contentDefaultEvaluator).Ancestor);
		Assert.AreSame(contentEvaluator, contentDefaultEvaluator.Content);
		Assert.AreSame(sourceEvaluator, contentDefaultEvaluator.Source);

		await contentDefaultEvaluator.Execute();
		await sourceDefaultEvaluator.Execute();

		Assert.AreEqual(expected: 1, contentEvaluator.ExecuteCount);
		Assert.AreEqual(expected: 1, sourceEvaluator.ExecuteCount);
	}

	[TestMethod]
	public async Task ExternalServiceBaseCopiesInputsExecutesLazilyAndDispatchesEvents()
	{
		using var disposingToken = new DisposingToken();
		var service = new TestExternalService
					  {
						  ExternalServiceSourceBase = new TestExternalServiceSource(new Uri("urn:test"), rawContent: "raw", new DataModelValue("content")),
						  ExternalServiceParametersBase = new TestExternalServiceParameters(new DataModelValue("parameters")),
						  TaskMonitorBase = new PassThroughTaskMonitor(),
						  DisposeTokenBase = new DisposeToken(disposingToken.Token)
					  };

		var first = await ((IExternalService) service).GetResult();
		var second = await ((IExternalService) service).GetResult();
		await ((IEventDispatcher) service).Dispatch(new IncomingEvent { Name = (EventName) "event" }, CancellationToken.None);

		Assert.AreEqual(expected: "urn:test:raw:content:parameters", first.AsString());
		Assert.AreEqual(first, second);
		Assert.AreEqual(expected: 1, service.ExecuteCount);
		Assert.AreEqual(expected: "event", service.LastDispatchedEventName);
	}

	[TestMethod]
	public void TextContentReaderExposesSingleTextNodeAndEmptyXmlSurface()
	{
		using var reader = new TextContentReader(new Uri("urn:text"), content: "body");

		Assert.AreEqual(ReadState.Initial, reader.ReadState);
		Assert.AreEqual(expected: "urn:text", reader.BaseURI);
		Assert.AreEqual(expected: 0, reader.AttributeCount);
		Assert.AreEqual(XmlNodeType.None, reader.NodeType);
		Assert.IsFalse(reader.HasValue);
		Assert.AreEqual(string.Empty, reader.Value);
		Assert.IsTrue(reader.Read());
		Assert.AreEqual(ReadState.Interactive, reader.ReadState);
		Assert.AreEqual(XmlNodeType.Text, reader.NodeType);
		Assert.AreEqual(expected: 1, reader.Depth);
		Assert.IsTrue(reader.HasValue);
		Assert.AreEqual(expected: "body", reader.Value);
		Assert.AreEqual(expected: "body", reader.ReadString());
		Assert.AreEqual(expected: "body", reader.ReadInnerXml());
		Assert.AreEqual(expected: "body", reader.ReadOuterXml());
		Assert.IsNull(reader.GetAttribute("missing"));
		Assert.IsNull(reader.GetAttribute(name: "missing", namespaceURI: null));
		Assert.IsNull(reader.LookupNamespace("prefix"));
		Assert.IsFalse(reader.MoveToAttribute("missing"));
		Assert.IsFalse(reader.MoveToAttribute(name: "missing", namespaceURI: null));
		Assert.IsFalse(reader.MoveToElement());
		Assert.IsFalse(reader.MoveToFirstAttribute());
		Assert.IsFalse(reader.MoveToNextAttribute());
		Assert.IsFalse(reader.ReadAttributeValue());
		Assert.IsFalse(reader.Read());
		Assert.IsTrue(reader.EOF);
		Assert.AreEqual(string.Empty, reader.Value);
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => _ = reader.NameTable);
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => reader.GetAttribute(0));

		reader.Close();

		Assert.AreEqual(ReadState.Closed, reader.ReadState);
	}

	[TestMethod]
	public void ServiceReadOnlyListExposesReadOnlyListCollectionAndSpanSemantics()
	{
		var list = new ServiceSyncList<string?>(["one", null, "three"]);
		IList<string?> genericList = list;
		ICollection<string?> genericCollection = list;
		IList nonGenericList = list;
		ICollection nonGenericCollection = list;

		Assert.AreEqual(expected: 3, list.Count);
		Assert.AreEqual(expected: "one", list[0]);
		Assert.IsTrue(genericCollection.IsReadOnly);
		Assert.IsTrue(nonGenericCollection.IsSynchronized);
		Assert.IsTrue(nonGenericList.IsReadOnly);
		Assert.IsTrue(nonGenericList.IsFixedSize);
		Assert.IsTrue(list.Contains("three"));
		Assert.IsFalse(list.Contains("missing"));
		Assert.AreEqual(expected: 1, list.IndexOf(null));
		Assert.AreEqual(expected: 2, nonGenericList.IndexOf("three"));
		Assert.AreEqual(expected: 1, nonGenericList.IndexOf(null));
		Assert.AreEqual(expected: -1, nonGenericList.IndexOf(42));
		Assert.IsTrue(nonGenericList.Contains(null));
		Assert.AreEqual(expected: "one", nonGenericList[0]);
		CollectionAssert.AreEqual(new[] { "one", null, "three" }, list.ToArray());

		var genericCopy = new string?[3];
		var nonGenericCopy = new object?[3];
		list.CopyTo(genericCopy, index: 0);
		nonGenericCollection.CopyTo(nonGenericCopy, index: 0);

		CollectionAssert.AreEqual(new[] { "one", null, "three" }, genericCopy);
		CollectionAssert.AreEqual(new object?[] { "one", null, "three" }, nonGenericCopy);
		Assert.AreEqual(expected: "one", list.AsSpan()[0]);
		Assert.AreEqual(expected: 3, list.AsMemory().Length);

		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => _ = nonGenericCollection.SyncRoot);
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => genericCollection.Add("new"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => genericCollection.Clear());
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => genericCollection.Remove("one"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => genericList[0] = "new");
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => genericList.Insert(index: 0, item: "new"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => genericList.RemoveAt(0));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => nonGenericList[0] = "new");
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => nonGenericList.Add("new"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => nonGenericList.Clear());
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => nonGenericList.Insert(index: 0, value: "new"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => nonGenericList.Remove("one"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => nonGenericList.RemoveAt(0));
	}

	private static IOutgoingEvent CreateOutgoingEvent(string type = "processor", string target = "target", int delayMs = 0) =>
		new OutgoingEventSource
		{
			SendId = SendId.FromString("send-0000002a"),
			Name = (EventName) "event",
			Type = new FullUri(type),
			Target = new FullUri(target),
			DelayMs = delayMs,
			Data = new DataModelValue("payload")
		};

	private sealed class OutgoingEventSource : IOutgoingEvent
	{
	#region Interface IOutgoingEvent

		public SendId? SendId { get; init; }

		public EventName Name { get; init; }

		public FullUri? Target { get; init; }

		public FullUri? Type { get; init; }

		public int DelayMs { get; init; }

		public DataModelValue Data { get; init; }

	#endregion
	}

	private sealed class ScriptSource : IScript
	{
	#region Interface IScript

		public IScriptExpression? Content { get; init; }

		public IExternalScriptExpression? Source { get; init; }

	#endregion
	}

	private sealed class ScriptPart(string value) : IScriptExpression, IExternalScriptExpression, IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

	#region Interface IExecEvaluator

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri => new("urn:" + value);

	#endregion

	#region Interface IScriptExpression

		public string? Expression => value;

	#endregion
	}

	private sealed class TestExternalService : ExternalServiceBase
	{
		public int ExecuteCount { get; private set; }

		public string? LastDispatchedEventName { get; private set; }

		protected override ValueTask<DataModelValue> Execute()
		{
			ExecuteCount ++;

			return new ValueTask<DataModelValue>(new DataModelValue($"{Source}:{RawContent}:{Content.AsString()}:{Parameters.AsString()}"));
		}

		protected override ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token)
		{
			LastDispatchedEventName = incomingEvent.Name.ToString();

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestExternalServiceSource(Uri source, string rawContent, DataModelValue content) : IExternalServiceSource
	{
	#region Interface IExternalServiceSource

		public Uri? Source { get; } = source;

		public string? RawContent { get; } = rawContent;

		public DataModelValue Content { get; } = content;

	#endregion
	}

	private sealed class TestExternalServiceParameters(DataModelValue parameters) : IExternalServiceParameters
	{
	#region Interface IExternalServiceParameters

		public DataModelValue Parameters { get; } = parameters;

	#endregion
	}

	private sealed class PassThroughTaskMonitor : ITaskMonitor
	{
	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) { }

		public void Forget(ValueTask valueTask) { }

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }

	#endregion
	}
}