namespace Xtate.Core;

public class ArgsTransformerAsync<T, TArg, TNewArg>(Func<TNewArg, ValueTask<TArg>> transform) : IArgsTransformer<T, TArg, TNewArg>
{
#region Interface IArgsTransformer<T,TArg,TNewArg>

	public bool CanTransformSync() => false;

	public bool CanTransformAsync() => true;

	public TArg TransformSync(TNewArg newArg) => throw new NotSupportedException();

	public ValueTask<TArg> TransformAsync(TNewArg newArg) => transform(newArg);

#endregion
}