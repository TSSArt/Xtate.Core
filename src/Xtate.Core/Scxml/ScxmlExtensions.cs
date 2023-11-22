using System.Xml;
using Xtate.Builder;
using Xtate.Core;
using Xtate.IoC;
using Xtate.XInclude;

namespace Xtate.Scxml
{
	public static class ScxmlExtensions
	{
		public static void RegisterScxml(this IServiceCollection services)
		{
			if (services.IsRegistered<IScxmlSerializer>())
			{
				return;
			}

			services.RegisterErrorProcessor();
			services.RegisterStateMachineBuilder();
			services.RegisterResourceLoaders();
			services.RegisterNameTable();

			services.AddType<ScxmlSerializerWriter, XmlWriter>();
			services.AddImplementation<ScxmlSerializer>().For<IScxmlSerializer>();
			services.AddType<ScxmlDirector, XmlReader>();
			services.AddTypeSync<XmlBaseReader, XmlReader>();
			services.AddTypeSync<XIncludeReader, XmlReader>();
			services.AddImplementationSync<RedirectXmlResolver>().For<ScxmlXmlResolver>().For<XmlResolver>().For<IXIncludeXmlResolver>();
			services.AddImplementation<ScxmlDeserializer>().For<IScxmlDeserializer>();
		}
	}
}
