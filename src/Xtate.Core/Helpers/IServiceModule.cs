using Xtate.IoC;

namespace Xtate;

public interface IServiceModule
{
	void Register(IServiceCollection servicesCollection);
}