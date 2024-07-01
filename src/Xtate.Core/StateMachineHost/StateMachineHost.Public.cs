#region Copyright © 2019-2023 Sergii Artemenko

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

<<<<<<< Updated upstream
using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.IoC;
=======
>>>>>>> Stashed changes
using Xtate.Persistence;

namespace Xtate;


public sealed partial class StateMachineHost(StateMachineHostOptions options) : IAsyncDisposable, IDisposable
{
<<<<<<< Updated upstream
	[PublicAPI]
	public sealed partial class StateMachineHost : IAsyncDisposable, IDisposable
	{
		private StateMachineHostOptions  _options;
		private          bool                     _asyncOperationInProgress;
		private          StateMachineHostContext? _context;
		private          bool                     _disposed;

		public StateMachineHost(StateMachineHostOptions options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));

			//TODO: start
			var serviceScopeFactory = _options.ServiceLocator.GetService<IServiceScopeFactory>();
			var serviceScope = serviceScopeFactory.CreateScope(s => s.AddForwarding<StateMachineHost>(_ => this));
			_options = _options with { ServiceLocator = new ServiceLocator(serviceScope.ServiceProvider) };
			//TODO: end

			//StateMachineHostInit();
		}
=======
	private         bool                                     _asyncOperationInProgress;
	private         StateMachineHostContext?                 _context;
	public required Func<ValueTask<StateMachineHostContext>> ContextFactory;
	private         bool                                     _disposed;
	private         readonly StateMachineHostOptions _options = options ?? throw new ArgumentNullException(nameof(options));
>>>>>>> Stashed changes

	#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			_disposed = true;
		}

		if (_context is { } context)
		{
<<<<<<< Updated upstream
			if (_asyncOperationInProgress)
			{
				throw new InvalidOperationException(Resources.Exception_AnotherAsynchronousOperationInProgress);
			}

			if (_context is not null)
			{
				return;
			}

			var context = _options.HostMode switch
						  {
							  HostMode.Cluster => new StateMachineHostClusterContext(this, _options),
							  HostMode.Standalone => _options.PersistenceLevel != PersistenceLevel.None && _options.StorageProvider is not null
								  ? new StateMachineHostPersistedContext(this, _options)
								  : new StateMachineHostContext(this, _options, InProcEventSchedulerFactory.Instance),
							  _ => Infra.Unexpected<StateMachineHostContext>(_options.HostMode)
						  };

			try
			{
				_asyncOperationInProgress = true;

				await context.InitializeAsync(token).ConfigureAwait(false);
				await StateMachineHostStartAsync(token).ConfigureAwait(false);

				_context = context;

				//TODO: start
				var serviceScopeFactory = _options.ServiceLocator.GetService<IServiceScopeFactory>();
				var serviceScope = serviceScopeFactory.CreateScope(s => s.AddForwarding<StateMachineHostContext>(_ => context));
				_options = _options with { ServiceLocator = new ServiceLocator(serviceScope.ServiceProvider) };
				//TODO: end
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == token)
			{
				context.Stop();
				await context.DisposeAsync().ConfigureAwait(false);
			}
			finally
			{
				_asyncOperationInProgress = false;
			}
		}

		public async Task StopHostAsync(CancellationToken token = default)
		{
			if (_asyncOperationInProgress)
			{
				throw new InvalidOperationException(Resources.Exception_AnotherAsynchronousOperationInProgress);
			}

			var context = _context;
			if (context is null)
			{
				return;
			}

			_asyncOperationInProgress = true;
=======
>>>>>>> Stashed changes
			_context = default;
			await context.DisposeAsync().ConfigureAwait(false);
		}

		await StateMachineHostStopAsync().ConfigureAwait(false);

<<<<<<< Updated upstream
			if (context is not null)
			{
				await context.WaitAllAsync(token).ConfigureAwait(false);
			}
		}

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml, Uri? baseUri, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml, baseUri), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source, Uri? baseUri, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source, baseUri), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine, Uri? baseUri, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine, baseUri), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml, string sessionId, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source, string sessionId, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine, string sessionId, DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml,
																  Uri? baseUri,
																  string sessionId,
																  DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml, baseUri), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source,
																  Uri? baseUri,
																  string sessionId,
																  DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source, baseUri), parameters);

		public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine,
																  Uri? baseUri,
																  string sessionId,
																  DataModelValue parameters = default) =>
			ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine, baseUri), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml, Uri? baseUri, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml, baseUri), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source, Uri? baseUri, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source, baseUri), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine, Uri? baseUri, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine, baseUri), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml, string sessionId, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source, string sessionId, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine, string sessionId, DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml,
																		 Uri? baseUri,
																		 string sessionId,
																		 DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml, baseUri), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source,
																		 Uri? baseUri,
																		 string sessionId,
																		 DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source, baseUri), parameters);

		public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine,
																		 Uri? baseUri,
																		 string sessionId,
																		 DataModelValue parameters = default) =>
			StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine, baseUri), parameters);

		private async ValueTask<IStateMachineController> StartStateMachineWrapper(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters)
		{
			var finalizer = new DeferredFinalizer();
			await using (finalizer.ConfigureAwait(false))
			{
				return await StartStateMachine(sessionId, origin, parameters, SecurityContextType.NewTrustedStateMachine, finalizer, CancellationToken.None).ConfigureAwait(false);
			}
		}

		private async ValueTask<DataModelValue> ExecuteStateMachineWrapper(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters)
		{
			var controller = await StartStateMachineWrapper(sessionId, origin, parameters).ConfigureAwait(false);

			return await controller.GetResult(CancellationToken.None).ConfigureAwait(false);
		}

		public ValueTask DestroyStateMachineAsync(string sessionId) => DestroyStateMachine(SessionId.FromString(sessionId), CancellationToken.None);
=======
		_disposed = true;
>>>>>>> Stashed changes
	}

#endregion

#region Interface IDisposable

	public void Dispose() => DisposeAsync().SynchronousWait();

#endregion

	public async Task StartHostAsync(CancellationToken token = default)
	{
		_context = await ContextFactory().ConfigureAwait(false);

		if (_asyncOperationInProgress)
		{
			throw new InvalidOperationException(Resources.Exception_AnotherAsynchronousOperationInProgress);
		}

		if (_context is not null)
		{
			return;
		}

		var context = _options.HostMode switch
					  {
						  HostMode.Cluster => new StateMachineHostClusterContext(this, _options),
						  HostMode.Standalone => _options.PersistenceLevel != PersistenceLevel.None && _options.StorageProvider is not null
							  ? new StateMachineHostPersistedContext(this, _options)
							  : new StateMachineHostContext(this, _options, InProcEventSchedulerFactory.Instance),
						  _ => Infra.Unexpected<StateMachineHostContext>(_options.HostMode)
					  };

		try
		{
			_asyncOperationInProgress = true;

			await context.InitializeAsync(token).ConfigureAwait(false);
			await StateMachineHostStartAsync(token).ConfigureAwait(false);

			_context = context;

			//TODO: start
			//var serviceScopeFactory = _options.ServiceLocator.GetService<IServiceScopeFactory>();
			//var serviceScope = serviceScopeFactory.CreateScope(s => s.AddForwarding<StateMachineHostContext>(_ => context));
			//_options = _options with { ServiceLocator = new ServiceLocator(serviceScope.ServiceProvider) };

			//TODO: end
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == token)
		{
			context.Stop();
			await context.DisposeAsync().ConfigureAwait(false);
		}
		finally
		{
			_asyncOperationInProgress = false;
		}
	}

	public async Task StopHostAsync(CancellationToken token = default)
	{
		if (_asyncOperationInProgress)
		{
			throw new InvalidOperationException(Resources.Exception_AnotherAsynchronousOperationInProgress);
		}

		var context = _context;
		if (context is null)
		{
			return;
		}

		_asyncOperationInProgress = true;
		_context = default;

		try
		{
			context.Suspend();

			await context.WaitAllAsync(token).ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == token)
		{
			context.Stop();
		}
		finally
		{
			await StateMachineHostStopAsync().ConfigureAwait(false);
			await context.DisposeAsync().ConfigureAwait(false);

			_asyncOperationInProgress = false;
		}
	}

	public async Task WaitAllStateMachinesAsync(CancellationToken token = default)
	{
		var context = _context;

		if (context is not null)
		{
			await context.WaitAllAsync(token).ConfigureAwait(false);
		}
	}

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml, Uri? baseUri, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml, baseUri), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source, Uri? baseUri, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source, baseUri), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine, Uri? baseUri, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine, baseUri), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml, string sessionId, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source, string sessionId, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine, string sessionId, DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(string scxml,
															  Uri? baseUri,
															  string sessionId,
															  DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml, baseUri), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(Uri source,
															  Uri? baseUri,
															  string sessionId,
															  DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source, baseUri), parameters);

	public ValueTask<DataModelValue> ExecuteStateMachineAsync(IStateMachine stateMachine,
															  Uri? baseUri,
															  string sessionId,
															  DataModelValue parameters = default) =>
		ExecuteStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine, baseUri), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml, Uri? baseUri, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(scxml, baseUri), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source, Uri? baseUri, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(source, baseUri), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine, Uri? baseUri, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.New(), new StateMachineOrigin(stateMachine, baseUri), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml, string sessionId, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source, string sessionId, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine, string sessionId, DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(string scxml,
																	 Uri? baseUri,
																	 string sessionId,
																	 DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(scxml, baseUri), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(Uri source,
																	 Uri? baseUri,
																	 string sessionId,
																	 DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(source, baseUri), parameters);

	public ValueTask<IStateMachineController> StartStateMachineAsync(IStateMachine stateMachine,
																	 Uri? baseUri,
																	 string sessionId,
																	 DataModelValue parameters = default) =>
		StartStateMachineWrapper(SessionId.FromString(sessionId), new StateMachineOrigin(stateMachine, baseUri), parameters);

	private async ValueTask<IStateMachineController> StartStateMachineWrapper(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters)
	{
		//var finalizer = new DeferredFinalizer();
		//await using (finalizer.ConfigureAwait(false))
		{
			return await StartStateMachine(sessionId, origin, parameters, SecurityContextType.NewTrustedStateMachine, CancellationToken.None).ConfigureAwait(false);
		}
	}

	private async ValueTask<DataModelValue> ExecuteStateMachineWrapper(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters)
	{
		var controller = await StartStateMachineWrapper(sessionId, origin, parameters).ConfigureAwait(false);

		return await controller.GetResult(CancellationToken.None).ConfigureAwait(false);
	}

	public ValueTask DestroyStateMachineAsync(string sessionId) => DestroyStateMachine(SessionId.FromString(sessionId), CancellationToken.None);
}