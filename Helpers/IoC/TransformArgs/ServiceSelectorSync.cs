using Xtate.IoC;

namespace Xtate.Core;

public readonly struct ServiceSelectorSync<T, TArg>(IServiceCollection services) where T : notnull
{
	public TransformArgs<T, TArg, ValueTuple> UseArgValue(TArg constant) => TransformArgs<ValueTuple>(_ => constant);

	public TransformArgs<T, TArg, ValueTuple> UseArgFactory(Func<TArg> factory) => TransformArgs<ValueTuple>(_ => factory());

	public TransformArgs<T, TArg, TNewArg> TransformArgs<TNewArg>(Func<TNewArg, TArg> transform) => new(synchronous: true, services, transform);

	public TransformArgs<T, TArg, (TNewArg1, TNewArg2)> TransformArgs<TNewArg1, TNewArg2>(Func<TNewArg1, TNewArg2, TArg> transform) =>
		TransformArgs<(TNewArg1, TNewArg2)>(newArgs => transform(newArgs.Item1, newArgs.Item2));

	public TransformArgs<T, TArg, (TNewArg1, TNewArg2, TNewArg3)> TransformArgs<TNewArg1, TNewArg2, TNewArg3>(Func<TNewArg1, TNewArg2, TNewArg3, TArg> transform) =>
		TransformArgs<(TNewArg1, TNewArg2, TNewArg3)>(newArgs => transform(newArgs.Item1, newArgs.Item2, newArgs.Item3));

	public TransformArgs<T, TArg, (TNewArg1, TNewArg2, TNewArg3, TNewArg4)> TransformArgs<TNewArg1, TNewArg2, TNewArg3, TNewArg4>(Func<TNewArg1, TNewArg2, TNewArg3, TNewArg4, TArg> transform) =>
		TransformArgs<(TNewArg1, TNewArg2, TNewArg3, TNewArg4)>(newArgs => transform(newArgs.Item1, newArgs.Item2, newArgs.Item3, newArgs.Item4));
}