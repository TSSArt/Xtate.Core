#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate.Core
{
	internal class StateMachineContext : IStateMachineContext, IExecutionContext
	{
		private static readonly Uri InternalTarget = new(uriString: @"_internal", UriKind.Relative);

		private readonly Parameters                _parameters;
		private          DataModelList?            _dataModel;
		private          KeyList<StateEntityNode>? _historyValue;
		private          IContextItems?            _runtimeItems;

		public StateMachineContext(Parameters parameters) => _parameters = parameters;

	#region Interface IAsyncDisposable

		public virtual ValueTask DisposeAsync() => default;

	#endregion

	#region Interface IExecutionContext

		public bool InState(IIdentifier id)
		{
			foreach (var state in Configuration)
			{
				if (IdentifierEqualityComparer.Instance.Equals(id, state.Id))
				{
					return true;
				}
			}

			return false;
		}

		public async ValueTask Send(IOutgoingEvent outgoingEvent, CancellationToken token)
		{
			if (_parameters.Logger is { } logger)
			{
				await logger.TraceSendEvent(_parameters.LoggerContext, outgoingEvent, token).ConfigureAwait(false);
			}

			if (IsInternalEvent(outgoingEvent))
			{
				InternalQueue.Enqueue(new EventObject(outgoingEvent) { Type = EventType.Internal });

				return;
			}

			if (_parameters.ExternalCommunication is { } externalCommunication)
			{
				if (await externalCommunication.TrySendEvent(outgoingEvent, token).ConfigureAwait(false) == SendStatus.ToInternalQueue)
				{
					InternalQueue.Enqueue(new EventObject(outgoingEvent) { Type = EventType.Internal });
				}
			}
		}

		public async ValueTask Cancel(SendId sendId, CancellationToken token)
		{
			if (_parameters.Logger is { } logger)
			{
				await logger.TraceCancelEvent(_parameters.LoggerContext, sendId, token).ConfigureAwait(false);
			}

			if (_parameters.ExternalCommunication is { } externalCommunication)
			{
				await externalCommunication.CancelEvent(sendId, token).ConfigureAwait(false);
			}
		}

		public async ValueTask StartInvoke(InvokeData invokeData, CancellationToken token = default)
		{
			if (_parameters.Logger is { } logger)
			{
				await logger.TraceStartInvoke(_parameters.LoggerContext, invokeData, token).ConfigureAwait(false);
			}

			if (_parameters.ExternalCommunication is { } externalCommunication)
			{
				await externalCommunication.StartInvoke(invokeData, token).ConfigureAwait(false);
			}
		}

		public async ValueTask CancelInvoke(InvokeId invokeId, CancellationToken token)
		{
			if (_parameters.Logger is { } logger)
			{
				await logger.TraceCancelInvoke(_parameters.LoggerContext, invokeId, token).ConfigureAwait(false);
			}

			if (_parameters.ExternalCommunication is { } externalCommunication)
			{
				await externalCommunication.CancelInvoke(invokeId, token).ConfigureAwait(false);
			}
		}

		public IContextItems RuntimeItems => _runtimeItems ??= new ContextItems(_parameters.ContextRuntimeItems);

		public ISecurityContext SecurityContext => _parameters.SecurityContext ?? Core.SecurityContext.NoAccess;

	#endregion

	#region Interface ILogEvent

		public ValueTask Log(LogLevel logLevel,
							 string? message,
							 DataModelValue arguments,
							 Exception? exception,
							 CancellationToken token) =>
			_parameters.Logger?.ExecuteLog(_parameters.LoggerContext, logLevel, message, arguments, exception, token) ?? default;

	#endregion

	#region Interface IStateMachineContext

		public DataModelList DataModel => _dataModel ??= CreateDataModel();

		public OrderedSet<StateEntityNode> Configuration { get; } = new();

		public IExecutionContext ExecutionContext => this;

		public KeyList<StateEntityNode> HistoryValue => _historyValue ??= new KeyList<StateEntityNode>();

		public EntityQueue<IEvent> InternalQueue { get; } = new();

		public OrderedSet<StateEntityNode> StatesToInvoke { get; } = new();

		public ServiceIdSet ActiveInvokes { get; } = new();

		public virtual IPersistenceContext PersistenceContext => throw new NotSupportedException();

	#endregion

		private static bool IsInternalEvent(IOutgoingEvent outgoingEvent)
		{
			if (outgoingEvent.Target != InternalTarget || outgoingEvent.Type is not null)
			{
				return false;
			}

			if (outgoingEvent.DelayMs != 0)
			{
				throw new ExecutionException(Resources.Exception_InternalEventsCantBeDelayed);
			}

			return true;
		}

		private DataModelList CreateDataModel()
		{
			var platformLazy = new LazyValue<Parameters>(GetPlatform, _parameters);
			var ioProcessorsLazy = new LazyValue<Parameters>(GetIoProcessors, _parameters);

			var dataModel = new DataModelList(_parameters.DataModelCaseInsensitive);

			dataModel.AddInternal(key: @"_name", _parameters.StateMachineName, DataModelAccess.ReadOnly);
			dataModel.AddInternal(key: @"_sessionid", _parameters.SessionId, DataModelAccess.Constant);
			dataModel.AddInternal(key: @"_event", value: default, DataModelAccess.ReadOnly);
			dataModel.AddInternal(key: @"_ioprocessors", ioProcessorsLazy, DataModelAccess.Constant);
			dataModel.AddInternal(key: @"_x", platformLazy, DataModelAccess.Constant);

			return dataModel;

			static DataModelValue GetPlatform(Parameters arguments)
			{
				var list = new DataModelList(DataModelAccess.ReadOnly, arguments.DataModelCaseInsensitive);

				list.AddInternal(key: @"interpreter", arguments.DataModelInterpreter, DataModelAccess.Constant);
				list.AddInternal(key: @"datamodel", arguments.DataModelHandlerData, DataModelAccess.Constant);
				list.AddInternal(key: @"configuration", arguments.DataModelConfiguration, DataModelAccess.Constant);
				list.AddInternal(key: @"host", arguments.DataModelHost, DataModelAccess.Constant);
				list.AddInternal(key: @"args", arguments.DataModelArguments, DataModelAccess.ReadOnly);

				return list;
			}

			static DataModelValue GetIoProcessors(Parameters arguments)
			{
				if (arguments.ExternalCommunication is not { } externalCommunication)
				{
					return default;
				}

				var ioProcessors = externalCommunication.GetIoProcessors();

				if (ioProcessors.IsDefaultOrEmpty)
				{
					return DataModelList.Empty;
				}

				var list = new DataModelList(arguments.DataModelCaseInsensitive);

				foreach (var ioProcessor in ioProcessors)
				{
					var locationLazy = new LazyValue<IIoProcessor, SessionId>(GetLocation, ioProcessor, arguments.SessionId);

					var entry = new DataModelList(arguments.DataModelCaseInsensitive)
								{
									{ @"location", locationLazy }
								};

					list.Add(ioProcessor.Id.ToString(), entry);
				}

				list.MakeDeepConstant();

				return list;
			}

			static DataModelValue GetLocation(IIoProcessor ioProcessor, SessionId sessionId) => new(ioProcessor.GetTarget(sessionId)?.ToString());
		}

		[PublicAPI]
		internal record Parameters
		{
			public Parameters(SessionId sessionId) => SessionId = sessionId;

			public SessionId                            SessionId                { get; init; }
			public string?                              StateMachineName         { get; init; }
			public DataModelValue                       DataModelArguments       { get; init; }
			public DataModelValue                       DataModelInterpreter     { get; init; }
			public DataModelValue                       DataModelConfiguration   { get; init; }
			public DataModelValue                       DataModelHost            { get; init; }
			public DataModelValue                       DataModelHandlerData     { get; init; }
			public bool                                 DataModelCaseInsensitive { get; init; }
			public ILogger?                             Logger                   { get; init; }
			public IInterpreterLoggerContext?           LoggerContext            { get; init; }
			public IExternalCommunication?              ExternalCommunication    { get; init; }
			public ISecurityContext?                    SecurityContext          { get; init; }
			public ImmutableDictionary<object, object>? ContextRuntimeItems      { get; init; }
		}

		private class ContextItems : IContextItems
		{
			private readonly Dictionary<object, object>           _items = new();
			private readonly ImmutableDictionary<object, object>? _permanentItems;

			public ContextItems(ImmutableDictionary<object, object>? permanentItems) => _permanentItems = permanentItems;

		#region Interface IContextItems

			public object? this[object key]
			{
				get => _permanentItems is not null && _permanentItems.TryGetValue(key, out var value) ? value : _items.TryGetValue(key, out value) ? value : null;
				set
				{
					if (value is not null && (_permanentItems is null || !_permanentItems.ContainsKey(key)))
					{
						_items[key] = value;
					}
				}
			}

		#endregion
		}
	}
}