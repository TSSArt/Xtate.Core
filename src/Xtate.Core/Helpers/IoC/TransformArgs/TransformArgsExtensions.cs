using Xtate.IoC;
using Empty = System.ValueTuple;

namespace Xtate.Core;

public static class TransformArgsExtensions
{
	extension(IServiceCollection services)
	{
		public ServiceSelectorSync<T, Empty> ForServiceSync<T>() where T : notnull => services.ForServiceSync<T, Empty>();

		public ServiceSelectorSync<T, TArg> ForServiceSync<T, TArg>() where T : notnull => new(services);

		public ServiceSelectorSync<T, (TArg1, TArg2)> ForServiceSync<T, TArg1, TArg2>() where T : notnull => services.ForServiceSync<T, (TArg1, TArg2)>();

		public ServiceSelectorSync<T, (TArg1, TArg2, TArg3)> ForServiceSync<T, TArg1, TArg2, TArg3>() where T : notnull => services.ForServiceSync<T, (TArg1, TArg2, TArg3)>();

		public ServiceSelectorSync<T, (TArg1, TArg2, TArg3, TArg4)> ForServiceSync<T, TArg1, TArg2, TArg3, TArg4>() where T : notnull => services.ForServiceSync<T, (TArg1, TArg2, TArg3, TArg4)>();

		public ServiceSelectorAsync<T, Empty> ForService<T>() where T : notnull => services.ForService<T, Empty>();

		public ServiceSelectorAsync<T, TArg> ForService<T, TArg>() where T : notnull => new(services);

		public ServiceSelectorAsync<T, (TArg1, TArg2)> ForService<T, TArg1, TArg2>() where T : notnull => services.ForService<T, (TArg1, TArg2)>();

		public ServiceSelectorAsync<T, (TArg1, TArg2, TArg3)> ForService<T, TArg1, TArg2, TArg3>() where T : notnull => services.ForService<T, (TArg1, TArg2, TArg3)>();

		public ServiceSelectorAsync<T, (TArg1, TArg2, TArg3, TArg4)> ForService<T, TArg1, TArg2, TArg3, TArg4>() where T : notnull => services.ForService<T, (TArg1, TArg2, TArg3, TArg4)>();
	}
}