using Xtate.IoC;

namespace Xtate.Core;

public static class NameTableExtensions
{
	public static void RegisterNameTable(this IServiceCollection services)
	{
		if (services.IsRegistered<INameTableProvider>())
		{
			return;
		}

		services.AddSharedImplementationSync<NameTableProvider>(SharedWithin.Scope).For<INameTableProvider>();
	}
}