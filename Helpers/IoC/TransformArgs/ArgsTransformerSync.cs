namespace Xtate.Core;

public class ArgsTransformerSync<T, TArg, TNewArg>(Func<TNewArg, TArg> transform) : IArgsTransformer<T, TArg, TNewArg>
{
#region Interface IArgsTransformer<T,TArg,TNewArg>

	public bool CanTransformSync() => true;

	public bool CanTransformAsync() => true;

	public TArg TransformSync(TNewArg newArg) => transform(newArg);

	public ValueTask<TArg> TransformAsync(TNewArg newArg) => new(transform(newArg));

#endregion
}