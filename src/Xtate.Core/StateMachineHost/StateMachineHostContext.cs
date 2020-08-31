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
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Builder;
using Xtate.Scxml;
using Xtate.Service;

namespace Xtate
{
	internal class StateMachineHostContext : IAsyncDisposable
	{
		private const string SessionIdPrefix = "#_scxml_";
		private const string InvokeIdPrefix  = "#_";
		private const string Location        = "location";

		private static readonly XmlReaderSettings DefaultSyncXmlReaderSettings  = new XmlReaderSettings { Async = false, CloseInput = true };
		private static readonly XmlReaderSettings DefaultAsyncXmlReaderSettings = new XmlReaderSettings { Async = true, CloseInput = true };

		private readonly DataModelObject?                                        _configuration;
		private readonly ImmutableDictionary<object, object>?                    _contextRuntimeItems;
		private readonly StateMachineHostOptions                                 _options;
		private readonly ConcurrentDictionary<SessionId, IService>               _parentServiceBySessionId = new ConcurrentDictionary<SessionId, IService>();
		private readonly ConcurrentDictionary<InvokeId, IService?>               _serviceByInvokeId        = new ConcurrentDictionary<InvokeId, IService?>();
		private readonly IStateMachineHost                                       _stateMachineHost;
		private readonly ConcurrentDictionary<SessionId, StateMachineController> _stateMachinesBySessionId = new ConcurrentDictionary<SessionId, StateMachineController>();
		private readonly CancellationTokenSource                                 _stopTokenSource;
		private readonly CancellationTokenSource                                 _suspendTokenSource;

		public StateMachineHostContext(IStateMachineHost stateMachineHost, StateMachineHostOptions options)
		{
			_stateMachineHost = stateMachineHost;
			_options = options;
			_suspendTokenSource = new CancellationTokenSource();
			_stopTokenSource = new CancellationTokenSource();

			if (stateMachineHost is IHost host)
			{
				_contextRuntimeItems = ImmutableDictionary<object, object>.Empty.Add(typeof(IHost), host);
			}

			if (options.Configuration?.Count > 0)
			{
				_configuration = new DataModelObject();

				foreach (var pair in options.Configuration)
				{
					_configuration.Add(pair.Key, pair.Value);
				}

				_configuration.MakeDeepConstant();
			}
		}

		protected CancellationToken StopToken => _stopTokenSource.Token;

	#region Interface IAsyncDisposable

		public virtual ValueTask DisposeAsync()
		{
			_suspendTokenSource.Dispose();
			_stopTokenSource.Dispose();

			return default;
		}

	#endregion

		public virtual ValueTask InitializeAsync(CancellationToken token) => default;

		private void FillInterpreterOptions(out InterpreterOptions interpreterOptions)
		{
			interpreterOptions = new InterpreterOptions
								 {
										 Configuration = _configuration,
										 PersistenceLevel = _options.PersistenceLevel,
										 StorageProvider = _options.StorageProvider,
										 ResourceLoaders = _options.ResourceLoaders,
										 CustomActionProviders = _options.CustomActionFactories,
										 StopToken = _stopTokenSource.Token,
										 SuspendToken = _suspendTokenSource.Token,
										 Logger = _options.Logger,
										 DataModelHandlerFactories = _options.DataModelHandlerFactories,
										 UnhandledErrorBehaviour = _options.UnhandledErrorBehaviour,
										 ContextRuntimeItems = _contextRuntimeItems
								 };
		}

		protected virtual StateMachineController CreateStateMachineController(SessionId sessionId, IStateMachine? stateMachine,
																			  IStateMachineOptions? stateMachineOptions,
																			  Uri? stateMachineLocation, in InterpreterOptions defaultOptions) =>
				new StateMachineController(sessionId, stateMachineOptions, stateMachine, stateMachineLocation, _stateMachineHost, _options.SuspendIdlePeriod, defaultOptions);

		private static XmlReaderSettings GetXmlReaderSettings(bool useAsync = false) => useAsync ? DefaultAsyncXmlReaderSettings : DefaultSyncXmlReaderSettings;

		private static XmlParserContext GetXmlParserContext()
		{
			var nameTable = new NameTable();
			var nsManager = new XmlNamespaceManager(nameTable);
			return new XmlParserContext(nameTable, nsManager, xmlLang: null, XmlSpace.None);
		}

		private static IBuilderFactory GetBuilderFactory() => BuilderFactory.Instance;

		private static IStateMachine GetStateMachine(string scxml, IErrorProcessor errorProcessor)
		{
			using var stringReader = new StringReader(scxml);
			var xmlParserContext = GetXmlParserContext();
			using var xmlReader = XmlReader.Create(stringReader, GetXmlReaderSettings(), xmlParserContext);
			var scxmlDirector = new ScxmlDirector(xmlReader, GetBuilderFactory(), errorProcessor, xmlParserContext.NamespaceManager);

			return scxmlDirector.ConstructStateMachine();
		}

		private async ValueTask<IStateMachine> GetStateMachine(Uri source, IErrorProcessor errorProcessor, CancellationToken token)
		{
			if (!_options.ResourceLoaders.IsDefaultOrEmpty)
			{
				foreach (var resourceLoader in _options.ResourceLoaders)
				{
					if (resourceLoader.CanHandle(source))
					{
						var xmlParserContext = GetXmlParserContext();
						using var xmlReader = await resourceLoader.RequestXmlReader(source, GetXmlReaderSettings(), xmlParserContext, token).ConfigureAwait(false);
						var scxmlDirector = new ScxmlDirector(xmlReader, GetBuilderFactory(), errorProcessor, xmlParserContext.NamespaceManager);

						return scxmlDirector.ConstructStateMachine();
					}
				}
			}

			throw new ProcessorException(Resources.Exception_Cannot_find_ResourceLoader_to_load_external_resource);
		}

		protected async ValueTask<(IStateMachine StateMachine, Uri? Location)> LoadStateMachine(StateMachineOrigin origin, Uri? hostBaseUri, IErrorProcessor errorProcessor, CancellationToken token)
		{
			var location = CombineUri(hostBaseUri, origin.BaseUri);

			switch (origin.Type)
			{
				case StateMachineOriginType.StateMachine:
					return (origin.AsStateMachine(), location);

				case StateMachineOriginType.Scxml:
				{
					return (GetStateMachine(origin.AsScxml(), errorProcessor), location);
				}
				case StateMachineOriginType.Source:
				{
					location = CombineUri(location, origin.AsSource());
					var stateMachine = await GetStateMachine(location, errorProcessor, token).ConfigureAwait(false);

					return (stateMachine, location);
				}
				default:
					throw new ArgumentException(Resources.Exception_StateMachine_origin_missed);
			}
		}

		[return: NotNullIfNotNull("relativeUri")]
		private static Uri? CombineUri(Uri? baseUri, Uri? relativeUri)
		{
			if (baseUri is { } && baseUri.IsAbsoluteUri && relativeUri is { } && !relativeUri.IsAbsoluteUri)
			{
				return new Uri(baseUri, relativeUri);
			}

			return relativeUri;
		}

		public virtual async ValueTask<StateMachineController> CreateAndAddStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters,
																						IErrorProcessor errorProcessor, CancellationToken token)
		{
			var (stateMachine, location) = await LoadStateMachine(origin, _options.BaseUri, errorProcessor, token).ConfigureAwait(false);

			FillInterpreterOptions(out var interpreterOptions);
			interpreterOptions.Arguments = parameters;
			interpreterOptions.ErrorProcessor = errorProcessor;
			interpreterOptions.Host = CreateHostData(location);

			stateMachine.Is<IStateMachineOptions>(out var stateMachineOptions);

			var stateMachineController = CreateStateMachineController(sessionId, stateMachine, stateMachineOptions, location, interpreterOptions);
			RegisterStateMachineController(stateMachineController);

			return stateMachineController;
		}

		protected StateMachineController AddSavedStateMachine(SessionId sessionId, Uri? stateMachineLocation, IStateMachineOptions stateMachineOptions, IErrorProcessor errorProcessor)
		{
			FillInterpreterOptions(out var interpreterOptions);
			interpreterOptions.ErrorProcessor = errorProcessor;
			interpreterOptions.Host = CreateHostData(stateMachineLocation);

			var stateMachineController = CreateStateMachineController(sessionId, stateMachine: default, stateMachineOptions, stateMachineLocation, interpreterOptions);
			RegisterStateMachineController(stateMachineController);

			return stateMachineController;
		}

		private static DataModelObject? CreateHostData(Uri? stateMachineLocation)
		{
			if (stateMachineLocation is { })
			{
				var obj = new DataModelObject { { Location, stateMachineLocation.ToString() } };
				obj.MakeDeepConstant();

				return obj;
			}

			return null;
		}

		private void RegisterStateMachineController(StateMachineController stateMachineController)
		{
			var sessionId = stateMachineController.SessionId;
			var result = _stateMachinesBySessionId.TryAdd(sessionId, stateMachineController);

			Infrastructure.Assert(result);
		}

		public virtual ValueTask RemoveStateMachine(SessionId sessionId)
		{
			var result = _stateMachinesBySessionId.TryRemove(sessionId, out var stateMachineController);

			Infrastructure.Assert(result);

			return stateMachineController.DisposeAsync();
		}

		public StateMachineController? FindStateMachineController(SessionId sessionId)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));

			return _stateMachinesBySessionId.TryGetValue(sessionId, out var controller) ? controller : null;
		}

		public void ValidateSessionId(SessionId sessionId, out StateMachineController controller)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));

			var result = _stateMachinesBySessionId.TryGetValue(sessionId, out controller);

			Infrastructure.Assert(result);
		}

		public virtual ValueTask AddService(SessionId sessionId, InvokeId invokeId, IService service, CancellationToken token)
		{
			var result = _serviceByInvokeId.TryAdd(invokeId, service);

			Infrastructure.Assert(result);

			if (service is StateMachineController stateMachineController)
			{
				if (_stateMachinesBySessionId.TryGetValue(sessionId, out var controller))
				{
					result = _parentServiceBySessionId.TryAdd(stateMachineController.SessionId, controller);

					Infrastructure.Assert(result);
				}
			}

			return default;
		}

		public virtual ValueTask<IService?> TryCompleteService(SessionId sessionId, InvokeId invokeId)
		{
			if (!_serviceByInvokeId.TryGetValue(invokeId, out var service))
			{
				return new ValueTask<IService?>((IService?) null);
			}

			if (!_serviceByInvokeId.TryUpdate(invokeId, newValue: null, service))
			{
				return new ValueTask<IService?>((IService?) null);
			}

			if (service is StateMachineController stateMachineController)
			{
				_parentServiceBySessionId.TryRemove(stateMachineController.SessionId, out _);
			}

			return new ValueTask<IService?>(service);
		}

		public virtual ValueTask<IService?> TryRemoveService(SessionId sessionId, InvokeId invokeId)
		{
			if (!_serviceByInvokeId.TryRemove(invokeId, out var service) || service is null)
			{
				return new ValueTask<IService?>((IService?) null);
			}

			if (service is StateMachineController stateMachineController)
			{
				_parentServiceBySessionId.TryRemove(stateMachineController.SessionId, out _);
			}

			return new ValueTask<IService?>(service);
		}

		public bool TryGetService(InvokeId invokeId, out IService? service) => _serviceByInvokeId.TryGetValue(invokeId, out service);

		public IService GetService(SessionId sessionId, string target)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
			if (target is null) throw new ArgumentNullException(nameof(target));

			if (target == @"#_parent")
			{
				if (_parentServiceBySessionId.TryGetValue(sessionId, out var service))
				{
					return service;
				}
			}
			else if (target.StartsWith(SessionIdPrefix))
			{
				if (_stateMachinesBySessionId.TryGetValue(SessionId.FromString(target.Substring(SessionIdPrefix.Length)), out var service))
				{
					return service;
				}
			}
			else if (target.StartsWith(InvokeIdPrefix))
			{
				if (_serviceByInvokeId.TryGetValue(InvokeId.FromString(target.Substring(InvokeIdPrefix.Length)), out var service) && service is { })
				{
					return service;
				}
			}

			throw new ProcessorException(Resources.Exception_Cannot_find_target);
		}

		public async ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token)
		{
			if (_stateMachinesBySessionId.TryGetValue(sessionId, out var controller))
			{
				controller.TriggerDestroySignal();

				if (!controller.Result.IsCompleted)
				{
					try
					{
						await controller.Result.WaitAsync(token).ConfigureAwait(false);
					}
					catch (OperationCanceledException ex) when (ex.CancellationToken == token)
					{
						throw;
					}
					catch
					{
						// ignored
					}
				}
			}
		}

		public async ValueTask WaitAllAsync(CancellationToken token)
		{
			var exit = false;

			while (!exit)
			{
				exit = true;

				foreach (var pair in _stateMachinesBySessionId)
				{
					var controller = pair.Value;
					if (controller.Result.IsCompleted)
					{
						continue;
					}

					try
					{
						await controller.Result.WaitAsync(token).ConfigureAwait(false);
					}
					catch (OperationCanceledException ex) when (ex.CancellationToken == token)
					{
						throw;
					}
					catch
					{
						// ignored
					}

					exit = false;
				}
			}
		}

		public void Stop() => _stopTokenSource.Cancel();

		public void Suspend() => _suspendTokenSource.Cancel();
	}
}