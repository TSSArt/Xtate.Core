// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Core;

internal class KeepAlive : TaskCompletionSource, IKeepAlive, IDisposable
{
#region Interface IDisposable

	public void Dispose() => TrySetResult();

#endregion

#region Interface IKeepAlive

	public Task Wait() => Task;

#endregion
}

/*
public class ScopeManagerOld : IScopeManagerOld
{
	public required IServiceScopeFactory     _serviceScopeFactory;

	public required  IStateMachineHost        _stateMachineHost;
	public required  IStateMachineHostContext _stateMachineHostContext;






#region Interface IScopeManager

	[Obsolete]
	public virtual async ValueTask<IStateMachineController> RunStateMachine(IStateMachineStartOptions stateMachineStartOptions)
	{
		var scope = CreateStateMachineScope(stateMachineStartOptions);

		IStateMachineRunner? stateMachineRunner = default;
		try
		{
			stateMachineRunner = await scope.ServiceProvider.GetRequiredService<IStateMachineRunner>().ConfigureAwait(false);

			return await stateMachineRunner.Run(CancellationToken.None).ConfigureAwait(false);
		}
		finally
		{
			DisposeScopeOnComplete(stateMachineRunner, scope).Forget();
		}
	}

	public async ValueTask<T> GetScopedService<T>(Action<IServiceCollection> configureServices, CancellationToken token) where T : notnull
	{
		var scope = _serviceScopeFactory.CreateScope(configureServices);

		var registration = token.Register(Disposer.Dispose, scope);
		await using (registration.ConfigureAwait(false))
		{
			IKeepAlive? scopeCloser = default;
			try
			{
				token.ThrowIfCancellationRequested();

				scopeCloser = await scope.ServiceProvider.GetRequiredService<IKeepAlive>().ConfigureAwait(false);

				return await scope.ServiceProvider.GetRequiredService<T>().ConfigureAwait(false);
			}
			finally
			{
				if (scopeCloser is not null)
				{
					DisposeScopeOnComplete(scopeCloser, scope).Forget();
				}
				else
				{
					await scope.DisposeAsync().ConfigureAwait(false);
				}
			}
		}
	}

#endregion

	private static async ValueTask DisposeScopeOnComplete(IStateMachineRunner stateMachineRunner, IServiceScope scope)
	{
		try
		{
			await stateMachineRunner.GetResult().ConfigureAwait(false);
		}
		finally
		{
			await scope.DisposeAsync().ConfigureAwait(false);
		}
	}

	private static async ValueTask DisposeScopeOnComplete(IKeepAlive keepAlive, IServiceScope scope)
	{
		try
		{
			await keepAlive.Wait().ConfigureAwait(false);
		}
		finally
		{
			await scope.DisposeAsync().ConfigureAwait(false);
		}
	}

	[Obsolete]
	protected virtual IServiceScope CreateStateMachineScope(IStateMachineStartOptions stateMachineStartOptions)
	{
		switch (stateMachineStartOptions.Origin.Type)
		{
			case StateMachineOriginType.StateMachine:
			{
				var stateMachine = stateMachineStartOptions.Origin.AsStateMachine();

				return _serviceScopeFactory.CreateScope(
					services =>
					{
						services.AddConstant(stateMachine);
						services.AddConstant<IStateMachineArguments>(new StateMachineArguments(stateMachineStartOptions.Parameters));
						services.AddConstant(stateMachineStartOptions);
						services.AddConstant(_stateMachineHost);
						services.AddConstant(_stateMachineHostContext);
						services.AddImplementation<StateMachineRunner>().For<IStateMachineRunner>();
					});
			}

			case StateMachineOriginType.Scxml:
			{
				var scxmlStateMachine = new ScxmlStateMachine(stateMachineStartOptions.Origin.AsScxml());
				var stateMachineLocation = stateMachineStartOptions.Origin.BaseUri is { } uri ? new StateMachineLocation(uri) : null;

				return _serviceScopeFactory.CreateScope(
					services =>
					{
						services.AddConstant<IScxmlStateMachine>(scxmlStateMachine);
						services.AddConstant<IStateMachineArguments>(new StateMachineArguments(stateMachineStartOptions.Parameters));
						if (stateMachineLocation is not null)
						{
							services.AddForwarding<IStateMachineLocation>(_ => stateMachineLocation);
						}

						services.AddConstant(stateMachineStartOptions);
						services.AddConstant(_stateMachineHost);
						services.AddConstant(_stateMachineHostContext);
						services.AddImplementation<StateMachineRunner>().For<IStateMachineRunner>();
					});
			}

			case StateMachineOriginType.Source:
			{
				var location = stateMachineStartOptions.Origin.BaseUri.CombineWith(stateMachineStartOptions.Origin.AsSource());
				var stateMachineLocation = new StateMachineLocation(location);

				return _serviceScopeFactory.CreateScope(
					services =>
					{
						services.AddConstant<IStateMachineLocation>(stateMachineLocation);
						services.AddConstant<IStateMachineArguments>(new StateMachineArguments(stateMachineStartOptions.Parameters));
						services.AddConstant(stateMachineStartOptions);
						services.AddConstant(_stateMachineHost);
						services.AddConstant(_stateMachineHostContext);
						services.AddImplementation<StateMachineRunner>().For<IStateMachineRunner>();
					});
			}
			default:
				throw new ArgumentException(Resources.Exception_StateMachineOriginMissed);
		}
	}
}*/