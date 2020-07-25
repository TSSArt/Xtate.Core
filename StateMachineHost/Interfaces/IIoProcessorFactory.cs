using System.Threading;
using System.Threading.Tasks;

namespace Xtate.IoProcessor
{
	public interface IIoProcessorFactory
	{
		ValueTask<IIoProcessor> Create(IEventConsumer eventConsumer, CancellationToken token);
	}
}