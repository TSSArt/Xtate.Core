namespace Xtate.Core;

public interface IExternalServiceRunner
{
	ValueTask WaitForCompletion();
}