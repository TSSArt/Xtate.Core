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

using System.Collections.Specialized;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using Xtate.DataTypes;
using Xtate.DataTypes.Internal;
using Xtate.ExternalServices.HttpClient.Services;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.Persistence.Internal;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Core.Test.UnitTests;

[TestClass]
public class ReportedPublicApiUsageCoverageTest
{
	[TestMethod]
	public void PersistenceSchemaMembersRemainAddressable()
	{
		Key[] keys =
		[
			Key.FireOn,
			Key.OptionPersistenceLevel,
			Key.OptionSynchronousEventProcessing,
			Key.OptionExternalQueueSize,
			Key.ParentSessionId,
			Key.SecurityContextType,
			Key.SecurityContextPermissions,
			Key.Sender,
			Key.InvokeUniqueId,
			Key.TargetServiceId,
			Key.UnhandledErrorBehaviour,
			Key.UriId
		];
		TypeInfo[] typeInfo = [TypeInfo.InvokedService, TypeInfo.ScheduledEvent, TypeInfo.StateMachine];

		Assert.HasCount(expected: 12, keys);
		Assert.HasCount(expected: 3, typeInfo);
		GC.KeepAlive(Const.XtateScxmlNs);
	}

	[TestMethod]
	public void PublicConversionAndValidationMembersRemainUsable()
	{
		var parsed = DataModelDateTime.Parse("2024-01-02T03:04:05Z");
		var parsedWithProvider = DataModelDateTime.Parse(value: "2024-01-02T03:04:05Z", CultureInfo.InvariantCulture);
		var parsedWithStyle = DataModelDateTime.Parse(value: " 2024-01-02T03:04:05Z ", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
		var exact = DataModelDateTime.ParseExact(value: "2024-01-02", format: "yyyy-MM-dd", CultureInfo.InvariantCulture);
		var exactWithStyle = DataModelDateTime.ParseExact(value: " 2024-01-02 ", format: "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
		var exactMany = DataModelDateTime.ParseExact(value: "2024-01-02", ["yyyy-MM-dd"], CultureInfo.InvariantCulture);
		var exactManyWithStyle = DataModelDateTime.ParseExact(value: " 2024-01-02 ", ["yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
		var parameter = new LoggingParameter(name: "value", value: 15, format: "X2");
		var error = new ErrorItem(typeof(ReportedPublicApiUsageCoverageTest), message: "message", exception: null);

		Assert.AreEqual(parsed, parsedWithProvider);
		Assert.AreEqual(parsed, parsedWithStyle);
		Assert.AreEqual(exact, exactMany);
		Assert.AreEqual(exact, exactWithStyle);
		Assert.AreEqual(exact, exactManyWithStyle);
		Assert.AreEqual(expected: "0F", parameter.ValueToString(CultureInfo.InvariantCulture));
		Assert.AreEqual(ErrorSeverity.Error, error.Severity);
		var severityValues = (ErrorSeverity[])Enum.GetValues(typeof(ErrorSeverity));
		CollectionAssert.Contains(severityValues, ErrorSeverity.Warning);
		CollectionAssert.Contains(severityValues, ErrorSeverity.Info);
		Infra.NotNull(error, message: "The error item must be present.");

		var dictionary = new ExtDictionary<string, int>();
		Assert.IsTrue(dictionary.TryAdd(key: "key", value: 1));
		Assert.IsTrue(dictionary.TryUpdate(key: "key", newValue: 2, comparisonValue: 1));
		Assert.AreEqual(expected: 2, dictionary["key"]);
	}

	[TestMethod]
	public void SessionIdProtectedConstructorsSupportDerivedIdentifiers()
	{
		var generated = TestSessionId.New();
		var supplied = TestSessionId.FromString("derived-session");

		Assert.IsFalse(SessionId.IsNullOrEmpty(generated));
		Assert.AreEqual(expected: "derived-session", supplied.ToString());
	}

	[TestMethod]
	public void MutableEventContractSupportsPostConstructionPopulation()
	{
		var entity = new EventEntity("event");
		PopulateEventEntity(ref entity);

		Assert.AreEqual(expected: "raw", entity.RawData);
		Assert.AreEqual(expected: "payload", entity.Data.AsString());
		Assert.AreEqual(expected: 25, entity.DelayMs);
		Assert.AreEqual(expected: "updated", entity.Name.ToString());
		Assert.AreEqual(expected: "send", entity.SendId!.ToString());
		Assert.AreEqual(expected: "https://example.test/target", entity.Target!.ToString());
		Assert.AreEqual(expected: "https://example.test/type", entity.Type!.ToString());
	}

	private static void PopulateEventEntity(ref EventEntity entity)
	{
		entity.RawData = "raw";
		entity.Data = new DataModelValue("payload");
		entity.DelayMs = 25;
		entity.Name = EventName.FromString("updated");
		entity.SendId = SendId.FromString("send");
		entity.Target = new FullUri("https://example.test/target");
		entity.Type = new FullUri("https://example.test/type");
	}

	[TestMethod]
	public void RouterEventPublicInitializersRemainAvailableToConsumers()
	{
		var sender = SessionId.FromString("sender");
		var ioProcessorData = new DataModelList { ["key"] = "value" };
		var routerEvent = new TestRouterEvent
						  {
							  DelayMs = 10,
							  SenderServiceId = sender,
							  IoProcessorData = ioProcessorData,
							  TargetType = new FullUri("https://example.test/type"),
							  Target = new FullUri("https://example.test/target")
						  };

		Assert.AreEqual(expected: 10, routerEvent.DelayMs);
		Assert.AreSame(sender, routerEvent.SenderServiceId);
		Assert.AreSame(ioProcessorData, routerEvent.IoProcessorData);
		Assert.AreEqual(expected: "https://example.test/type", routerEvent.TargetType!.ToString());
		Assert.AreEqual(expected: "https://example.test/target", routerEvent.Target!.ToString());
	}

	[TestMethod]
	public async Task InterfaceImplementationsConsumeContractContextParameters()
	{
		var logProvider = new CapturingLogProvider();
		var source = typeof(ReportedPublicApiUsageCoverageTest);
		Assert.IsTrue(logProvider.IsEnabled(source, Level.Info));
		await logProvider.Write(source, Level.Info, eventId: 1, message: "message");
		Assert.AreSame(source, logProvider.Source);

		var mimeHandler = new CapturingMimeTypeHandler();
		using var request = new HttpRequestMessage(HttpMethod.Post, requestUri: "https://example.test/");
		using var response = new HttpResponseMessage();
		var contentType = new ContentType("text/plain");
		var parameters = new DataModelList { ["name"] = "value" };
		var value = new DataModelValue("payload");
		mimeHandler.PrepareRequest(request, contentType, parameters, value);
		using var content = mimeHandler.TryCreateHttpContent(request, contentType, parameters, value);
		Assert.AreEqual(expected: "value", (await mimeHandler.TryParseResponseAsync(response, parameters, CancellationToken.None))!.Value.AsString());

		IExternalEntityGetter entityGetter = new CapturingExternalEntityGetter();

		// ReSharper disable once MethodHasAsyncOverload -- both contract variants must remain externally usable.
		Assert.AreEqual(typeof(string), entityGetter.GetEntity(new Uri("https://example.test/"), headers: null, typeof(string)));
		Assert.AreEqual(typeof(int), await entityGetter.GetEntityAsync(new Uri("https://example.test/"), headers: null, typeof(int)));

		using var cancellation = new CancellationTokenSource();
		IEventScheduler scheduler = new CapturingScheduler();
		await scheduler.ScheduleEvent(Mock.Of<IRouterEvent>(), cancellation.Token);
		await scheduler.CancelEvent(SendId.FromString("send"), cancellation.Token);
		IExternalServiceScopeManager scopeManager = new CapturingScopeManager();
		await scopeManager.Cancel(InvokeId.FromString("invoke"), cancellation.Token);
		Assert.AreEqual(cancellation.Token, ((CapturingScheduler)scheduler).Token);
		Assert.AreEqual(cancellation.Token, ((CapturingScopeManager)scopeManager).Token);

		var expression = Expression.Constant(new DataModelList());
		var metaObject = new ParameterUsingMetaObject(expression, new DataModelList());
		Assert.AreSame(expression, metaObject.UseCastParameter(expression));
	}

	private sealed class TestRouterEvent : RouterEvent;

	private sealed class TestSessionId : SessionId
	{
		private TestSessionId() { }

		private TestSessionId(string value) : base(value) { }

		public static new TestSessionId New() => new();

		public static new TestSessionId FromString(string value) => new(value);
	}

	private sealed class CapturingLogProvider : ILogProvider
	{
		public Type? Source { get; private set; }

	#region Interface ILogProvider

		public bool IsEnabled(Type source, Level level)
		{
			Source = source;

			return level == Level.Info;
		}

		public ValueTask Write(Type source,
							   Level level,
							   int eventId,
							   string? message,
							   IEnumerable<LoggingParameter>? parameters = null)
		{
			Source = source;
			_ = level;
			_ = eventId;
			_ = message;
			_ = parameters;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class CapturingMimeTypeHandler : HttpClientMimeTypeHandler
	{
		public override void PrepareRequest(HttpRequestMessage request,
											ContentType? contentType,
											DataModelList parameters,
											DataModelValue value)
		{
			request.Content = new StringContent($"{contentType?.MediaType}:{parameters.Count}:{value.AsString()}");
		}

		public override HttpContent TryCreateHttpContent(HttpRequestMessage request,
														 ContentType? contentType,
														 DataModelList parameters,
														 DataModelValue value) =>
			request.Content ?? new StringContent($"{contentType?.MediaType}:{parameters.Count}:{value.AsString()}");

		public override ValueTask<DataModelValue?> TryParseResponseAsync(HttpResponseMessage response,
																		 DataModelList parameters,
																		 CancellationToken token)
		{
			_ = response.StatusCode;
			token.ThrowIfCancellationRequested();

			return new ValueTask<DataModelValue?>(parameters["name"]);
		}
	}

	private sealed class CapturingExternalEntityGetter : IExternalEntityGetter
	{
	#region Interface IExternalEntityGetter

		public bool SupportsType(Uri absoluteUri, Type? type) => absoluteUri.IsAbsoluteUri && type is not null;

		public object GetEntity(Uri uri, NameValueCollection? headers, Type? ofObjectToReturn)
		{
			_ = uri;
			_ = headers;

			return ofObjectToReturn!;
		}

		public ValueTask<object> GetEntityAsync(Uri uri, NameValueCollection? headers, Type? ofObjectToReturn) => new(GetEntity(uri, headers, ofObjectToReturn));

	#endregion
	}

	private sealed class CapturingScheduler : IEventScheduler
	{
		public CancellationToken Token { get; private set; }

	#region Interface IEventScheduler

		public ValueTask ScheduleEvent(IRouterEvent routerEvent, CancellationToken token)
		{
			_ = routerEvent;
			Token = token;

			return ValueTask.CompletedTask;
		}

		public ValueTask CancelEvent(SendId sendId, CancellationToken token)
		{
			_ = sendId;
			Token = token;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class CapturingScopeManager : IExternalServiceScopeManager
	{
		public CancellationToken Token { get; private set; }

	#region Interface IExternalServiceScopeManager

		public ValueTask<ExternalServiceResult> Start(ExternalServiceClass externalServiceClass, CancellationToken token)
		{
			_ = externalServiceClass;
			Token = token;

			return ValueTask.FromResult(new ExternalServiceResult(Task.CompletedTask));
		}

		public ValueTask Cancel(InvokeId invokeId, CancellationToken token)
		{
			_ = invokeId;
			Token = token;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class ParameterUsingMetaObject(Expression expression, object value) : MetaObjectBase(expression, value)
	{
		public Expression UseCastParameter(Expression expressionToCast) => CastToList(expressionToCast);

		protected override BindingRestrictions SameTypeRestriction() => BindingRestrictions.Empty;

		protected override Expression CastToList(Expression expressionToCast) => expressionToCast;
	}
}
