using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Service
{
	public interface IServiceFactory
	{
		bool CanHandle(Uri type, Uri? source);

		ValueTask<IService> StartService(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication, CancellationToken token);
	}
}