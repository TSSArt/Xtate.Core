#region Copyright © 2019-2020 Sergii Artemenko

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

namespace Xtate
{
	internal class StateMachineContext : IStateMachineContext, IExecutionContext
	{
		private static readonly Uri InternalTarget = new Uri(uriString: "_internal", UriKind.Relative);

		private readonly ImmutableDictionary<object, object> _contextRuntimeItems;
		private readonly IDataModelValueProvider             _dataModelValueProvider;
		private readonly IExternalCommunication              _externalCommunication;
		private readonly ILogger                             _logger;
		private readonly ILoggerContext                      _loggerContext;
		private readonly SessionId                           _sessionId;
		private readonly string?                             _stateMachineName;
		private          DataModelObject?                    _dataModel;
		private          KeyList<StateEntityNode>?           _historyValue;
		private          IContextItems?                      _runtimeItems;

		public StateMachineContext(string? stateMachineName, SessionId sessionId, IDataModelValueProvider dataModelValueProvider, ILogger logger, ILoggerContext loggerContext,
								   IExternalCommunication externalCommunication, ImmutableDictionary<object, object> contextRuntimeItems)
		{
			_stateMachineName = stateMachineName;
			_sessionId = sessionId;
			_dataModelValueProvider = dataModelValueProvider;
			_logger = logger;
			_loggerContext = loggerContext;
			_externalCommunication = externalCommunication;
			_contextRuntimeItems = contextRuntimeItems;
		}

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

		public async ValueTask Send(IOutgoingEvent evt, CancellationToken token)
		{
			if (IsInternalEvent(evt) || await _externalCommunication.TrySendEvent(evt, token).ConfigureAwait(false) == SendStatus.ToInternalQueue)
			{
				InternalQueue.Enqueue(new EventObject(EventType.Internal, evt));
			}
		}

		public ValueTask Cancel(SendId sendId, CancellationToken token) => _externalCommunication.CancelEvent(sendId, token);

		public ValueTask Log(string? label, DataModelValue arguments, CancellationToken token) => _logger.ExecuteLog(_loggerContext, label, arguments, token);

		public ValueTask StartInvoke(InvokeData invokeData, CancellationToken token = default) => _externalCommunication.StartInvoke(invokeData, token);

		public ValueTask CancelInvoke(InvokeId invokeId, CancellationToken token) => _externalCommunication.CancelInvoke(invokeId, token);

		public IContextItems RuntimeItems => _runtimeItems ??= new ContextItems(_contextRuntimeItems);

	#endregion

	#region Interface IStateMachineContext

		public DataModelObject DataModel => _dataModel ??= CreateDataModel();

		public OrderedSet<StateEntityNode> Configuration { get; } = new OrderedSet<StateEntityNode>();

		public IExecutionContext ExecutionContext => this;

		public KeyList<StateEntityNode> HistoryValue => _historyValue ??= new KeyList<StateEntityNode>();

		public EntityQueue<IEvent> InternalQueue { get; } = new EntityQueue<IEvent>();

		public OrderedSet<StateEntityNode> StatesToInvoke { get; } = new OrderedSet<StateEntityNode>();

		public virtual IPersistenceContext PersistenceContext => throw new NotSupportedException();

	#endregion

		private static bool IsInternalEvent(IOutgoingEvent evt)
		{
			if (evt.Target != InternalTarget || evt.Type is { })
			{
				return false;
			}

			if (evt.DelayMs != 0)
			{
				throw new ExecutionException(Resources.Exception_Internal_events_can_t_be_delayed);
			}

			return true;
		}

		private DataModelObject CreateDataModel()
		{
			var platformLazy = new LazyValue<StateMachineContext>(GetPlatform, this);
			var ioProcessorsLazy = new LazyValue<StateMachineContext>(GetIoProcessors, this);

			var dataModel = new DataModelObject(isReadOnly: false, _dataModelValueProvider.CaseInsensitive);

			dataModel.AddInternal(key: @"_name", _stateMachineName, DataModelAccess.ReadOnly);
			dataModel.AddInternal(key: @"_sessionid", _sessionId, DataModelAccess.Constant);
			dataModel.AddInternal(key: @"_event", value: default, DataModelAccess.ReadOnly);
			dataModel.AddInternal(key: @"_ioprocessors", ioProcessorsLazy, DataModelAccess.Constant);
			dataModel.AddInternal(key: @"_x", platformLazy, DataModelAccess.Constant);

			return dataModel;

			static DataModelValue GetPlatform(StateMachineContext context)
			{
				var valueProvider = context._dataModelValueProvider;

				var obj = new DataModelObject(isReadOnly: true, context._dataModelValueProvider.CaseInsensitive);

				obj.AddInternal(key: @"interpreter", valueProvider.Interpreter, DataModelAccess.Constant);
				obj.AddInternal(key: @"datamodel", valueProvider.DataModelHandler, DataModelAccess.Constant);
				obj.AddInternal(key: @"configuration", valueProvider.Configuration, DataModelAccess.Constant);
				obj.AddInternal(key: @"host", valueProvider.Host, DataModelAccess.Constant);
				obj.AddInternal(key: @"args", valueProvider.Arguments, DataModelAccess.ReadOnly);

				return obj;
			}

			static DataModelValue GetIoProcessors(StateMachineContext context)
			{
				var ioProcessors = context._externalCommunication.GetIoProcessors();

				if (ioProcessors.IsDefaultOrEmpty)
				{
					return DataModelObject.Empty;
				}

				var obj = new DataModelObject(isReadOnly: false, context._dataModelValueProvider.CaseInsensitive);

				foreach (var ioProcessor in ioProcessors)
				{
					var locationLazy = new LazyValue<IIoProcessor, SessionId>(GetLocation, ioProcessor, context._sessionId);

					var entry = new DataModelObject(isReadOnly: false, context._dataModelValueProvider.CaseInsensitive)
								{
										{ @"location", locationLazy }
								};

					obj.Add(ioProcessor.Id.ToString(), entry);
				}

				obj.MakeDeepConstant();

				return obj;
			}

			static DataModelValue GetLocation(IIoProcessor ioProcessor, SessionId sessionId) => new DataModelValue(ioProcessor.GetTarget(sessionId).ToString());
		}

		private class ContextItems : IContextItems
		{
			private readonly Dictionary<object, object>          _items = new Dictionary<object, object>();
			private readonly ImmutableDictionary<object, object> _permanentItems;

			public ContextItems(ImmutableDictionary<object, object> permanentItems) => _permanentItems = permanentItems;

		#region Interface IContextItems

			public object? this[object key]
			{
				get => _permanentItems.TryGetValue(key, out var value) ? value : _items.TryGetValue(key, out value) ? value : null;
				set
				{
					if (value is { } && !_permanentItems.ContainsKey(key))
					{
						_items[key] = value;
					}
				}
			}

		#endregion
		}
	}
}