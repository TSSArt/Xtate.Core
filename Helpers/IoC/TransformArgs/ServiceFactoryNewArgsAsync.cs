using Xtate.IoC;

namespace Xtate.Core;

[InstantiatedByIoC]
public class ServiceFactoryNewArgsAsync<T, TArg, TNewArg>
{
	public required IEnumerable<IArgsTransformer<T, TArg, TNewArg>> ArgsTransformers { private get; [SetByIoC] init; }

	public required Func<TArg, ValueTask<T>> ServiceFactory { private get; [SetByIoC] init; }

	[CalledByIoC]
	public ValueTask<T> Factory(TNewArg newArg)
	{
		foreach (var argsTransformer in ArgsTransformers)
		{
			if (argsTransformer.CanTransformSync())
			{
				return ServiceFactory(argsTransformer.TransformSync(newArg));
			}

			if (argsTransformer.CanTransformAsync())
			{
				return FactoryAsync(argsTransformer, newArg);
			}
		}

		throw new DependencyInjectionException("There is no suitable args transformer available.");
	}

	private async ValueTask<T> FactoryAsync(IArgsTransformer<T, TArg, TNewArg> argsTransformer, TNewArg newArg)
	{
		var arg = await argsTransformer.TransformAsync(newArg).ConfigureAwait(false);

		return await ServiceFactory(arg).ConfigureAwait(false);
	}
}