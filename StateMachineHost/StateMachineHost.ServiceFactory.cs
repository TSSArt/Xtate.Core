using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Service;

namespace Xtate
{
	public sealed partial class StateMachineHost : IServiceFactory
	{
		private static readonly Uri ServiceFactoryTypeId      = new Uri("http://www.w3.org/TR/scxml/");
		private static readonly Uri ServiceFactoryAliasTypeId = new Uri(uriString: "scxml", UriKind.Relative);

	#region Interface IServiceFactory

		bool IServiceFactory.CanHandle(Uri type, Uri? source) => FullUriComparer.Instance.Equals(type, ServiceFactoryTypeId) || FullUriComparer.Instance.Equals(type, ServiceFactoryAliasTypeId);

		async ValueTask<IService> IServiceFactory.StartService(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication, CancellationToken token)
		{
			var sessionId = SessionId.FromString(invokeData.InvokeId.Value); // using InvokeId as SessionId
			var scxml = invokeData.RawContent ?? invokeData.Content.AsStringOrDefault();
			var parameters = invokeData.Parameters;
			var source = invokeData.Source;

			Infrastructure.Assert(scxml != null || source != null);

			var origin = scxml != null ? new StateMachineOrigin(scxml, baseUri) : new StateMachineOrigin(source!, baseUri);

			return await StartStateMachine(sessionId, origin, parameters, token).ConfigureAwait(false);
		}

	#endregion
	}
}