using Xtate.Annotations;

namespace Xtate.CustomAction
{
	[PublicAPI]
	[CustomActionProvider("http://xtate.net/scxml/system")]
	public class SystemActionFactory : CustomActionFactoryBase
	{
		public static readonly ICustomActionFactory Instance = new SystemActionFactory();

		private SystemActionFactory()
		{
			Register(name: "start", (xmlReader, context) => new StartAction(xmlReader, context));
		}
	}
}