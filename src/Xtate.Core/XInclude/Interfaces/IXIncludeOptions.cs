namespace Xtate.XInclude;

public interface IXIncludeOptions
{
	bool XIncludeAllowed { get; }

	int MaxNestingLevel { get; }
}