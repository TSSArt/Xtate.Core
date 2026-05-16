using Xtate.IoC;

namespace Xtate.Core;

[InstantiatedByIoC]
public class ServiceFactoryNewArgsSync<T, TArg, TNewArg>
{
	public required IEnumerable<IArgsTransformer<T, TArg, TNewArg>> ArgsTransformers { private get; [SetByIoC] init; }

	public required Func<TArg, T> ServiceFactory { private get; [SetByIoC] init; }

	[CalledByIoC]
	public T Factory(TNewArg newArg)
	{
		foreach (var argsTransformer in ArgsTransformers)
		{
			if (argsTransformer.CanTransformSync())
			{
				return ServiceFactory(argsTransformer.TransformSync(newArg));
			}

			if (argsTransformer.CanTransformAsync())
			{
				throw new DependencyInjectionException("Async args transformer is not supported in synchrounous context.");
			}
		}

		throw new DependencyInjectionException("There is no suitable args transformer available.");
	}
}