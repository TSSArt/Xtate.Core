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
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Builder;
using Xtate.Scxml;
using Xtate.Service;

namespace Xtate.Core
{
	internal class StateMachineHostContext : IAsyncDisposable
	{
		private const string SessionIdPrefix = "#_scxml_";
		private const string InvokeIdPrefix  = "#_";
		private const string Location        = "location";

		private readonly DataModelList?                                          _configuration;
		private readonly ImmutableDictionary<object, object>?                    _contextRuntimeItems;
		private readonly StateMachineHostOptions                                 _options;
		private readonly ConcurrentDictionary<SessionId, IService>               _parentServiceBySessionId = new();
		private readonly ConcurrentDictionary<(SessionId, InvokeId), IService?>  _serviceByInvokeId        = new();
		private readonly ConcurrentDictionary<SessionId, StateMachineController> _stateMachineBySessionId  = new();
		private readonly IStateMachineHost                                       _stateMachineHost;
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
				_configuration = new DataModelList();

				foreach (var pair in options.Configuration)
				{
					_configuration.Add(pair.Key, pair.Value);
				}

				_configuration.MakeDeepConstant();
			}
		}

		protected CancellationToken StopToken => _stopTokenSource.Token;

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore().ConfigureAwait(false);

			GC.SuppressFinalize(this);
		}

	#endregion

		protected virtual ValueTask DisposeAsyncCore()
		{
			_suspendTokenSource.Dispose();
			_stopTokenSource.Dispose();

			return default;
		}

		public virtual ValueTask InitializeAsync(CancellationToken token) => default;

		private InterpreterOptions CreateInterpreterOptions(Uri? baseUri, DataModelList? hostData, IErrorProcessor? errorProcessor, DataModelValue arguments = default) =>
				new()
				{
						Configuration = _configuration,
						PersistenceLevel = _options.PersistenceLevel,
						StorageProvider = _options.StorageProvider,
						ResourceLoaderFactories = _options.ResourceLoaderFactories,
						CustomActionProviders = _options.CustomActionFactories,
						StopToken = _stopTokenSource.Token,
						SuspendToken = _suspendTokenSource.Token,
						Logger = _options.Logger,
						DataModelHandlerFactories = _options.DataModelHandlerFactories,
						UnhandledErrorBehaviour = _options.UnhandledErrorBehaviour,
						ContextRuntimeItems = _contextRuntimeItems,
						BaseUri = baseUri,
						Host = hostData,
						ErrorProcessor = errorProcessor,
						Arguments = arguments
				};

		protected virtual StateMachineController CreateStateMachineController(SessionId sessionId, IStateMachine? stateMachine, IStateMachineOptions? stateMachineOptions, Uri? stateMachineLocation,
																			  InterpreterOptions defaultOptions, SecurityContext securityContext, DeferredFinalizer finalizer) =>
				new(sessionId, stateMachineOptions, stateMachine, stateMachineLocation, _stateMachineHost, _options.SuspendIdlePeriod, defaultOptions, securityContext, finalizer);

		private static XmlReaderSettings GetXmlReaderSettings(XmlNameTable nameTable, ScxmlXmlResolver xmlResolver) =>
				new()
				{
						Async = true,
						CloseInput = true,
						NameTable = nameTable,
						XmlResolver = xmlResolver,
						DtdProcessing = DtdProcessing.Parse
				};

		private static XmlParserContext GetXmlParserContext(XmlNameTable nameTable, Uri? baseUri)
		{
			var nsManager = new XmlNamespaceManager(nameTable);
			return new XmlParserContext(nameTable, nsManager, xmlLang: null, XmlSpace.None) { BaseURI = baseUri?.ToString() };
		}

		private static IBuilderFactory GetBuilderFactory() => BuilderFactory.Instance;

		private static ScxmlDirectorOptions GetScxmlDirectorOptions(IErrorProcessor errorProcessor, XmlParserContext xmlParserContext,
																	XmlReaderSettings xmlReaderSettings, ScxmlXmlResolver xmlResolver) =>
				new()
				{
						ErrorProcessor = errorProcessor,
						NamespaceResolver = xmlParserContext.NamespaceManager,
						XmlReaderSettings = xmlReaderSettings,
						XmlResolver = xmlResolver,
						XIncludeAllowed = true,
						Async = true
				};

		private async ValueTask<IStateMachine> GetStateMachine(Uri? uri, string? scxml, SecurityContext securityContext, IErrorProcessor errorProcessor, CancellationToken token)
		{
			var nameTable = new NameTable();
			var xmlResolver = new RedirectXmlResolver(_options.ResourceLoaderFactories, securityContext, token);
			var xmlParserContext = GetXmlParserContext(nameTable, uri);
			var xmlReaderSettings = GetXmlReaderSettings(nameTable, xmlResolver);
			var directorOptions = GetScxmlDirectorOptions(errorProcessor, xmlParserContext, xmlReaderSettings, xmlResolver);

			using var xmlReader = scxml is null
					? XmlReader.Create(uri!.ToString(), xmlReaderSettings, xmlParserContext)
					: XmlReader.Create(new StringReader(scxml), xmlReaderSettings, xmlParserContext);

			var scxmlDirector = new ScxmlDirector(xmlReader, GetBuilderFactory(), directorOptions);

			return await scxmlDirector.ConstructStateMachine().ConfigureAwait(false);
		}

		protected async ValueTask<(IStateMachine StateMachine, Uri? Location)> LoadStateMachine(StateMachineOrigin origin, Uri? hostBaseUri, SecurityContext securityContext,
																								IErrorProcessor errorProcessor, CancellationToken token)
		{
			var location = hostBaseUri.CombineWith(origin.BaseUri);

			switch (origin.Type)
			{
				case StateMachineOriginType.StateMachine:
					return (origin.AsStateMachine(), location);

				case StateMachineOriginType.Scxml:
				{
					var stateMachine = await GetStateMachine(location, origin.AsScxml(), securityContext, errorProcessor, token).ConfigureAwait(false);

					return (stateMachine, location);
				}
				case StateMachineOriginType.Source:
				{
					location = location.CombineWith(origin.AsSource());
					var stateMachine = await GetStateMachine(location, scxml: default, securityContext, errorProcessor, token).ConfigureAwait(false);

					return (stateMachine, location);
				}
				default:
					throw new ArgumentException(Resources.Exception_StateMachine_origin_missed);
			}
		}

		public virtual async ValueTask<StateMachineController> CreateAndAddStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, SecurityContext securityContext,
																						DeferredFinalizer finalizer, IErrorProcessor errorProcessor, CancellationToken token)
		{
			var (stateMachine, location) = await LoadStateMachine(origin, _options.BaseUri, securityContext, errorProcessor, token).ConfigureAwait(false);

			var interpreterOptions = CreateInterpreterOptions(location, CreateHostData(location), errorProcessor, parameters);

			stateMachine.Is<IStateMachineOptions>(out var stateMachineOptions);

			return CreateStateMachineController(sessionId, stateMachine, stateMachineOptions, location, interpreterOptions, securityContext, finalizer);
		}

		protected StateMachineController AddSavedStateMachine(SessionId sessionId, Uri? stateMachineLocation, IStateMachineOptions stateMachineOptions,
															  SecurityContext securityContext, DeferredFinalizer finalizer, IErrorProcessor errorProcessor)
		{
			var interpreterOptions = CreateInterpreterOptions(stateMachineLocation, CreateHostData(stateMachineLocation), errorProcessor);

			return CreateStateMachineController(sessionId, stateMachine: default, stateMachineOptions, stateMachineLocation, interpreterOptions, securityContext, finalizer);
		}

		private static DataModelList? CreateHostData(Uri? stateMachineLocation)
		{
			if (stateMachineLocation is not null)
			{
				var list = new DataModelList { { Location, stateMachineLocation.ToString() } };
				list.MakeDeepConstant();

				return list;
			}

			return null;
		}

		public void AddStateMachineController(StateMachineController stateMachineController)
		{
			var sessionId = stateMachineController.SessionId;
			var result = _stateMachineBySessionId.TryAdd(sessionId, stateMachineController);

			Infrastructure.Assert(result);
		}

		public virtual ValueTask RemoveStateMachineController(StateMachineController stateMachineController)
		{
			var result = _stateMachineBySessionId.TryRemove(stateMachineController.SessionId, out var controller);

			Infrastructure.Assert(result);
			Infrastructure.NotNull(controller);

			return stateMachineController.DisposeAsync();
		}

		public StateMachineController? FindStateMachineController(SessionId sessionId)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));

			return _stateMachineBySessionId.TryGetValue(sessionId, out var controller) ? controller : null;
		}

		public void ValidateSessionId(SessionId sessionId, out StateMachineController controller)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));

			var result = _stateMachineBySessionId.TryGetValue(sessionId, out var stateMachineController);

			Infrastructure.Assert(result);
			Infrastructure.NotNull(stateMachineController);

			controller = stateMachineController;
		}

		public virtual ValueTask AddService(SessionId sessionId, InvokeId invokeId, IService service, CancellationToken token)
		{
			var result = _serviceByInvokeId.TryAdd((sessionId, invokeId), service);

			Infrastructure.Assert(result);

			if (service is StateMachineController stateMachineController)
			{
				if (_stateMachineBySessionId.TryGetValue(sessionId, out var controller))
				{
					result = _parentServiceBySessionId.TryAdd(stateMachineController.SessionId, controller);

					Infrastructure.Assert(result);
				}
			}

			return default;
		}

		public virtual ValueTask<IService?> TryCompleteService(SessionId sessionId, InvokeId invokeId)
		{
			if (!_serviceByInvokeId.TryGetValue((sessionId, invokeId), out var service))
			{
				return new ValueTask<IService?>((IService?) null);
			}

			if (!_serviceByInvokeId.TryUpdate((sessionId, invokeId), newValue: null, service))
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
			if (!_serviceByInvokeId.TryRemove((sessionId, invokeId), out var service) || service is null)
			{
				return new ValueTask<IService?>((IService?) null);
			}

			if (service is StateMachineController stateMachineController)
			{
				_parentServiceBySessionId.TryRemove(stateMachineController.SessionId, out _);
			}

			return new ValueTask<IService?>(service);
		}

		public bool TryGetService(SessionId sessionId, InvokeId invokeId, out IService? service) => _serviceByInvokeId.TryGetValue((sessionId, invokeId), out service);

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
				if (_stateMachineBySessionId.TryGetValue(SessionId.FromString(target[SessionIdPrefix.Length..]), out var service))
				{
					return service;
				}
			}
			else if (target.StartsWith(InvokeIdPrefix))
			{
				var invokeId = InvokeId.FromString(target[InvokeIdPrefix.Length..]);
				if (_serviceByInvokeId.TryGetValue((sessionId, invokeId), out var service) && service is not null)
				{
					return service;
				}
			}

			throw new ProcessorException(Resources.Exception_Cannot_find_target);
		}

		public async ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token)
		{
			if (_stateMachineBySessionId.TryGetValue(sessionId, out var controller))
			{
				controller.TriggerDestroySignal();

				try
				{
					await controller.GetResult(token).ConfigureAwait(false);
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

		public async ValueTask WaitAllAsync(CancellationToken token)
		{
			var exit = false;

			while (!exit)
			{
				exit = true;

				foreach (var pair in _stateMachineBySessionId)
				{
					var controller = pair.Value;
					try
					{
						await controller.GetResult(token).ConfigureAwait(false);
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