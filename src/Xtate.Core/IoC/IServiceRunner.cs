namespace Xtate.Core;

public interface IServiceRunner
{
	ValueTask WaitForCompletion();
}