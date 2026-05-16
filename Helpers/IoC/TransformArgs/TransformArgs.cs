using Xtate.IoC;

namespace Xtate.Core;

public readonly struct TransformArgs<T, TArg, TNewArg>
{
	private readonly IServiceCollection _services;

	private readonly Delegate _transform;

	public TransformArgs(bool synchronous, IServiceCollection services, Delegate transform)
	{
		_services = services;
		_transform = transform;

		if (services.IsRegistered(TypeKey.ImplementationKey<ServiceFactoryNewArgsAsync<T, TArg, TNewArg>, ValueTuple>()))
		{
			return;
		}

		if (!synchronous)
		{
			services.AddFactory<ServiceFactoryNewArgsAsync<T, TArg, TNewArg>>().For<T, TNewArg>();
		}
		else if (!services.IsRegistered(TypeKey.ImplementationKey<ServiceFactoryNewArgsSync<T, TArg, TNewArg>, ValueTuple>()))
		{
			services.AddFactorySync<ServiceFactoryNewArgsSync<T, TArg, TNewArg>>().For<T, TNewArg>();
		}
	}

	public TransformArgs<T, TArg, TNewArg> IfAncestor<TAncestor>() where TAncestor : notnull
	{
		switch (_transform)
		{
			case Func<TNewArg, TArg> transformSync:
				_services.AddForwarding(sp => sp.GetRequiredServiceSync<AncestorTracker>().IsAncestorTypeEquals(level: 2, typeof(TAncestor))
											? (IArgsTransformer<T, TArg, TNewArg>) new ArgsTransformerSync<T, TArg, TNewArg>(transformSync)
											: null!);

				break;

			case Func<TNewArg, ValueTask<TArg>> transformAsync:
				_services.AddForwarding(sp => sp.GetRequiredServiceSync<AncestorTracker>().IsAncestorTypeEquals(level: 2, typeof(TAncestor))
											? (IArgsTransformer<T, TArg, TNewArg>) new ArgsTransformerAsync<T, TArg, TNewArg>(transformAsync)
											: null!);

				break;
		}

		return this;
	}
}