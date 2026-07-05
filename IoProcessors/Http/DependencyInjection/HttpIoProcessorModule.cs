using Xtate.IoC;
using Xtate.IoProcessors.Http.Services;
using Xtate.StateMachineHost.DependencyInjection;

namespace Xtate.IoProcessors.Http.DependencyInjection;

public class HttpIoProcessorModule : Module<StateMachineProcessorModule>
{
	protected override void AddServices()
	{
		Services.AddType<HttpController>();
	}
}