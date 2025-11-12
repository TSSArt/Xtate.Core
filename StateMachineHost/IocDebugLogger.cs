using System.Text;
using Xtate.IoC;

namespace Xtate.Core;

public class IocDebugLogger : IServiceProviderActions, IServiceProviderDataActions, IAsyncDisposable
{
	private const int Registration = 1;
	
	private const int Statistics = 2;

	private const int Service = 3;

	private readonly AsyncLocal<ServiceLogger> _logger = new();

	private readonly ConcurrentDictionary<TypeKey, Stat> _stats = new();

	private int _registrationCount;

	private StringBuilder? _registrations;

	public required Deferred<ILogger<IocDebugLogger>> Logger { get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IServiceProviderActions

	public IServiceProviderDataActions RegisterServices(int count)
	{
		_registrationCount = count;
		_registrations = new StringBuilder(count * 96);

		return this;
	}

	public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey)
	{
		GetLogger().ServiceRequesting(typeKey);

		return null;
	}

	public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey)
	{
		var stat = GetStat(typeKey);
		stat.FactoryCalling();

		GetLogger().FactoryCalling(stat.CallsCount);

		return null;
	}

	public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey)
	{
		GetStat(typeKey).FactoryCalled();

		return null;
	}

	public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey)
	{
		GetLogger().ServiceRequested();

		return null;
	}

#endregion

#region Interface IServiceProviderDataActions

	public void RegisterService(ServiceEntry serviceEntry)
	{
		if (_registrations is not { } content)
		{
			return;
		}

		_registrations.AppendLine(@$"{serviceEntry.InstanceScope,-18} | {serviceEntry.Key}");

		if (-- _registrationCount > 0)
		{
			return;
		}

		_registrations = null;

		LogAsync().Forget();

		return;

		async ValueTask LogAsync()
		{
			var logger = await Logger().ConfigureAwait(false);

			if (logger.IsEnabled(Level.Debug))
			{
				await logger.Write(Level.Debug, Registration, content.ToString()).ConfigureAwait(false);
			}
		}
	}

	[ExcludeFromCodeCoverage]
	public void ServiceRequesting<T, TArg>(TArg argument) => throw new NotSupportedException();

	[ExcludeFromCodeCoverage]
	public void ServiceRequested<T, TArg>(T? instance) => throw new NotSupportedException();

	[ExcludeFromCodeCoverage]
	public void FactoryCalling<T, TArg>(TArg argument) => throw new NotSupportedException();

	[ExcludeFromCodeCoverage]
	public void FactoryCalled<T, TArg>(T? instance) => throw new NotSupportedException();

#endregion

	private ServiceLogger GetLogger() => _logger.Value ??= new ServiceLogger(Logger);

	private Stat GetStat(TypeKey serviceKey) => _stats.GetOrAdd(serviceKey, key => new Stat(key));

	private async ValueTask LogStatistics()
	{
		var logger = await Logger().ConfigureAwait(false);

		if (!logger.IsEnabled(Level.Debug))
		{
			return;
		}

		var maxServiceLength = _stats.Max(p => p.Value.ServiceName.Length);

		var list = from pair in _stats
				   let nc = (pair.Value.ServiceName, pair.Value.CallsCount)
				   orderby nc.CallsCount descending, nc.ServiceName
				   select nc;

		var sb = new StringBuilder();

		foreach (var (serviceName, callsCount) in list)
		{
			sb.Append(serviceName)
			  .Append(value: ' ', maxServiceLength - serviceName.Length)
			  .Append(@" | ")
			  .Append(callsCount)
			  .AppendLine(callsCount > 1 ? @" calls" : @" call");
		}

		await logger.Write(Level.Debug, Statistics, sb.ToString()).ConfigureAwait(false);
	}

	protected virtual ValueTask DisposeAsyncCore() => LogStatistics();

	private class ServiceLogger(Deferred<ILogger<IocDebugLogger>> logger)
	{
		private bool _factoryCalled;

		private int _level;

		private bool _noFactory;

		private int _previousLevel;

		private readonly StringBuilder _content = new ();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServiceRequesting(TypeKey typeKey)
		{
			_level ++;

			if (_factoryCalled)
			{
				_content.AppendLine();

				_factoryCalled = false;
			}

			WriteIdent();

			_content.Append(typeKey);

			_noFactory = true;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void FactoryCalling(int callsCount)
		{
			_content.Append(@" {#").Append(callsCount).Append('}');

			_factoryCalled = true;
			_noFactory = false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServiceRequested()
		{
			if (_noFactory)
			{
				_content.AppendLine(@" {CACHED}");
			}
			else
			{
				if (_factoryCalled)
				{
					_content.AppendLine();
					_factoryCalled = false;
				}
				else
				{
					WriteIdent();
					_content.AppendLine(@"__");
				}
			}

			_noFactory = false;

			if (-- _level == 0)
			{
				LogAsync().Forget();
			}

			return;

			async ValueTask LogAsync()
			{
				var logger1 = await logger().ConfigureAwait(false);

				if (logger1.IsEnabled(Level.Debug))
				{
					var content = _content.ToString();
					_content.Clear();

					await logger1.Write(Level.Debug, Service, content).ConfigureAwait(false);
				}
				else
				{
					_content.Clear();
				}
			}
		}

		private void WriteIdent()
		{
			var padding = false;

			for (var i = 1; i < _level; i ++)
			{
				WriteWithPadding(@"|");
			}

			WriteWithPadding(_level < _previousLevel ? @"\" : @">> ");

			_previousLevel = _level;

			return;

			void WriteWithPadding(string str)
			{
				if (padding)
				{
					_content.Append(@"  ");
				}

				padding = true;
				_content.Append(str);
			}
		}
	}

	private class Stat(TypeKey typeKey)
	{
		private int _deepLevel;

		public string ServiceName => typeKey.ToString() ?? string.Empty;

		public int CallsCount { get; private set; }

		public void FactoryCalling()
		{
			CallsCount ++;

			if (++ _deepLevel > 100)
			{
				throw new DependencyInjectionException(@"Cycle reference detected in container configuration");
			}
		}

		public void FactoryCalled() => _deepLevel --;
	}
}