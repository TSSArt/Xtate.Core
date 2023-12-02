namespace Xtate.CustomAction;

public interface ICustomActionActivator
{
	CustomActionBase Activate(string xml);
}