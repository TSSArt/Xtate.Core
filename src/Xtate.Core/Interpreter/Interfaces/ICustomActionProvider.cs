namespace Xtate.CustomAction;

public interface ICustomActionProvider
{
	ICustomActionActivator? TryGetActivator(string ns, string name);
}