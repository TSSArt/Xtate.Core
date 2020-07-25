namespace Xtate.CustomAction
{
	public interface ICustomActionFactory
	{
		bool CanHandle(string ns, string name);

		ICustomActionExecutor CreateExecutor(ICustomActionContext customActionContext);
	}
}