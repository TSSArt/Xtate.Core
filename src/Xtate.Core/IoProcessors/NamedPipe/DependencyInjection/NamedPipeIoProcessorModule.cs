using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.IoProcessors.NamedPipe.Services;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;

namespace Xtate.IoProcessors.NamedPipe.DependencyInjection;

public class NamedPipeIoProcessorModule : Module<StateMachineProcessorModule>
{
	protected override void AddServices()
	{
		Services.AddSharedImplementation<NamedPipeIoProcessorHost>(SharedWithin.Container).For<IIoProcessorHost>();
		Services.AddSharedImplementation<NamedPipeIoProcessor>(SharedWithin.Scope).For<IIoProcessor>().For<IEventRouter>();
		Services.AddType<NamedPipeController>();
	}
}