using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Service;

namespace Xtate
{
	internal class ServiceCommunication : IServiceCommunication
	{
		private readonly StateMachineController _creator;
		private readonly InvokeId               _invokeId;
		private readonly Uri                    _originType;
		private          Uri?                   _origin;

		public ServiceCommunication(StateMachineController creator, Uri originType, InvokeId invokeId)
		{
			_creator = creator;
			_originType = originType;
			_invokeId = invokeId;
		}

	#region Interface IServiceCommunication

		public ValueTask SendToCreator(IOutgoingEvent evt, CancellationToken token)
		{
			if (evt.Type != null || evt.SendId != null || evt.DelayMs != 0)
			{
				throw new ProcessorException(Resources.Exception_Type__SendId__DelayMs_can_t_be_specified_for_this_event);
			}

			if (evt.Target != EventEntity.ParentTarget && evt.Target != null)
			{
				throw new ProcessorException(Resources.Exception_Target_should_be_equal_to___parent__or_null);
			}

			_origin ??= new Uri("#_" + _invokeId.Value);

			var eventObject = new EventObject(EventType.External, evt, _origin, _originType, _invokeId);

			return _creator.Send(eventObject, token);
		}

	#endregion
	}
}