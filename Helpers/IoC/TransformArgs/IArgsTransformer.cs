namespace Xtate.Core;

public interface IArgsTransformer<[UsedImplicitly] T, TArg, in TNewArg>
{
	bool CanTransformSync();

	bool CanTransformAsync();

	TArg TransformSync(TNewArg newArg);

	ValueTask<TArg> TransformAsync(TNewArg newArg);
}