using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.IoProcessor
{
	public abstract class IoProcessorBase : IIoProcessor
	{
		private readonly IEventConsumer _eventConsumer;
		private readonly Uri?           _ioProcessorAliasId;

		protected IoProcessorBase(IEventConsumer eventConsumer)
		{
			_eventConsumer = eventConsumer ?? throw new ArgumentNullException(nameof(eventConsumer));

			var ioProcessorAttribute = GetType().GetCustomAttribute<IoProcessorAttribute>(false);

			if (ioProcessorAttribute == null)
			{
				throw new InfrastructureException(Res.Format(Resources.Exception_IoProcessorAttributeWasNotProvided, GetType()));
			}

			IoProcessorId = new Uri(ioProcessorAttribute.Type, UriKind.RelativeOrAbsolute);
			_ioProcessorAliasId = ioProcessorAttribute.Alias != null ? new Uri(ioProcessorAttribute.Alias, UriKind.RelativeOrAbsolute) : null;
		}

		protected Uri IoProcessorId { get; }

	#region Interface IIoProcessor

		Uri IIoProcessor.GetTarget(SessionId sessionId) => GetTarget(sessionId);

		ValueTask IIoProcessor.Dispatch(SessionId sessionId, IOutgoingEvent evt, CancellationToken token) => OutgoingEvent(sessionId, evt, token);

		bool IIoProcessor.CanHandle(Uri? type, Uri? target) => FullUriComparer.Instance.Equals(type, IoProcessorId) || FullUriComparer.Instance.Equals(type, _ioProcessorAliasId);

		Uri IIoProcessor.Id => IoProcessorId;

	#endregion

		protected abstract Uri GetTarget(SessionId sessionId);

		protected abstract ValueTask OutgoingEvent(SessionId sessionId, IOutgoingEvent evt, CancellationToken token);

		protected ValueTask IncomingEvent(SessionId sessionId, IEvent evt, CancellationToken token) => _eventConsumer.Dispatch(sessionId, evt, token);
	}
}