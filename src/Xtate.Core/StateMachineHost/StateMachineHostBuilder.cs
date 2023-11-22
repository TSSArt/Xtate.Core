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
using System.ComponentModel;
using Xtate.Core;
using Xtate.IoC;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.IoProcessor;
using Xtate.Persistence;
using Xtate.Service;

namespace Xtate
{
	public interface IStateMachineHostBuilder { }


	[PublicAPI]
	public sealed class StateMachineHostBuilder : IStateMachineHostBuilder
	{
		private Uri?                                              _baseUri;
		private ImmutableDictionary<string, string>.Builder?      _configuration;
		private ImmutableArray<ICustomActionFactory>.Builder?     _customActionFactories;
		private HostMode                                          _hostMode;
		private ImmutableArray<IIoProcessorFactory>.Builder?      _ioProcessorFactories;
		private ILoggerOld?                                          _logger;
		private PersistenceLevel                                  _persistenceLevel;
		private ImmutableArray<IResourceLoaderFactory>.Builder?   _resourceLoaderFactories;
		private ImmutableArray<IServiceFactory>.Builder?          _serviceFactories;
		private IStorageProvider?                                 _storageProvider;
		private TimeSpan?                                         _suspendIdlePeriod;
		private UnhandledErrorBehaviour                           _unhandledErrorBehaviour;
		private ValidationMode                                    _validationMode;
		
		private List<object> _actions = new();

		public StateMachineHost Build(ServiceLocator serviceLocator)
		{
			var serviceScope = serviceLocator.GetService<IServiceScopeFactory>()
											 .CreateScope(
												 collection =>
												 {
													 foreach (var action in _actions)
													 {
														 switch (action)
														 {
															 case Action<IServiceCollection> action1:
																 action1(collection);
																 break;
															 case Func<IServiceCollection, IServiceCollection> action2:
																 action2(collection);
																 break;
														 }
													 }
												 });

			serviceLocator = new ServiceLocator(serviceScope.ServiceProvider);

			var option = new StateMachineHostOptions(serviceLocator)
						 {
							 IoProcessorFactories = _ioProcessorFactories?.ToImmutable() ?? default,
							 ServiceFactories = _serviceFactories?.ToImmutable() ?? default,
							 CustomActionFactories = _customActionFactories?.ToImmutable() ?? default,
							 ResourceLoaderFactories = _resourceLoaderFactories?.ToImmutable() ?? default,
							 Configuration = _configuration?.ToImmutable() ?? ImmutableDictionary<string, string>.Empty,
							 BaseUri = _baseUri,
							 Logger = _logger,
							 PersistenceLevel = _persistenceLevel,
							 StorageProvider = _storageProvider,
							 SuspendIdlePeriod = _suspendIdlePeriod,
							 ValidationMode = _validationMode,
							 UnhandledErrorBehaviour = _unhandledErrorBehaviour,
							 HostMode = _hostMode
						 };

			return new StateMachineHost(option) {_dataConverter = new DataConverter(null), ServiceFactories = AsyncEnumerable.Empty<IServiceFactory>(), _ioProcessorFactories = AsyncEnumerable.Empty<IIoProcessorFactory>()};
		}

		//TODO:
		public StateMachineHostBuilder AddServices(Action<IServiceCollection> action)
		{
			_actions.Add(action);

			return this;
		}

		//TODO:
		public StateMachineHostBuilder AddServices(Func<IServiceCollection, IServiceCollection> action)
		{
			_actions.Add(action);

			return this;
		}

		public StateMachineHostBuilder SetLogger(ILoggerOld logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			return this;
		}

		public StateMachineHostBuilder DisableVerboseValidation()
		{
			_validationMode = ValidationMode.Default;

			return this;
		}

		public StateMachineHostBuilder SetSuspendIdlePeriod(TimeSpan suspendIdlePeriod)
		{
			if (suspendIdlePeriod <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(suspendIdlePeriod));

			_suspendIdlePeriod = suspendIdlePeriod;

			return this;
		}

		public StateMachineHostBuilder AddResourceLoaderFactory(IResourceLoaderFactory resourceLoaderFactory)
		{
			if (resourceLoaderFactory is null) throw new ArgumentNullException(nameof(resourceLoaderFactory));

			(_resourceLoaderFactories ??= ImmutableArray.CreateBuilder<IResourceLoaderFactory>()).Add(resourceLoaderFactory);

			return this;
		}

		public StateMachineHostBuilder SetPersistence(PersistenceLevel persistenceLevel, IStorageProvider storageProvider)
		{
			if (persistenceLevel < PersistenceLevel.None || persistenceLevel > PersistenceLevel.ExecutableAction)
			{
				throw new InvalidEnumArgumentException(nameof(persistenceLevel), (int) persistenceLevel, typeof(PersistenceLevel));
			}

			_persistenceLevel = persistenceLevel;
			_storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));

			return this;
		}

		public StateMachineHostBuilder AddIoProcessorFactory(IIoProcessorFactory ioProcessorFactory)
		{
			if (ioProcessorFactory is null) throw new ArgumentNullException(nameof(ioProcessorFactory));

			(_ioProcessorFactories ??= ImmutableArray.CreateBuilder<IIoProcessorFactory>()).Add(ioProcessorFactory);

			return this;
		}

		public StateMachineHostBuilder AddServiceFactory(IServiceFactory serviceFactory)
		{
			if (serviceFactory is null) throw new ArgumentNullException(nameof(serviceFactory));

			(_serviceFactories ??= ImmutableArray.CreateBuilder<IServiceFactory>()).Add(serviceFactory);

			return this;
		}

		public StateMachineHostBuilder AddCustomActionFactory(ICustomActionFactory customActionFactory)
		{
			if (customActionFactory is null) throw new ArgumentNullException(nameof(customActionFactory));

			(_customActionFactories ??= ImmutableArray.CreateBuilder<ICustomActionFactory>()).Add(customActionFactory);

			return this;
		}

		public StateMachineHostBuilder SetConfigurationValue(string key, string value)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			(_configuration ??= ImmutableDictionary.CreateBuilder<string, string>())[key] = value ?? throw new ArgumentNullException(nameof(value));

			return this;
		}

		public StateMachineHostBuilder SetBaseUri(Uri uri)
		{
			_baseUri = uri ?? throw new ArgumentNullException(nameof(uri));

			return this;
		}

		public StateMachineHostBuilder SetUnhandledErrorBehaviour(UnhandledErrorBehaviour unhandledErrorBehaviour)
		{
			if (unhandledErrorBehaviour < UnhandledErrorBehaviour.DestroyStateMachine || unhandledErrorBehaviour > UnhandledErrorBehaviour.IgnoreError)
			{
				throw new InvalidEnumArgumentException(nameof(unhandledErrorBehaviour), (int) unhandledErrorBehaviour, typeof(UnhandledErrorBehaviour));
			}

			_unhandledErrorBehaviour = unhandledErrorBehaviour;

			return this;
		}

		public StateMachineHostBuilder SetClusterHostMode()
		{
			_hostMode = HostMode.Cluster;

			return this;
		}

		public StateMachineHostBuilder SetStandaloneHostMode()
		{
			_hostMode = HostMode.Standalone;

			return this;
		}
	}
}