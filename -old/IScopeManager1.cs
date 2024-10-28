namespace Xtate.Core;

public interface IScopeManager1
{
	ValueTask<T> GetService<T>() where T : notnull;
}