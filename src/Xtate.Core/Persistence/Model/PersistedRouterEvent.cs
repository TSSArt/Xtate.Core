using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using Xtate.StateMachineHost;

namespace Xtate.Persistence;

public class PersistedRouterEvent : RouterEvent, IStoreSupport
{
	public PersistedRouterEvent(in Bucket bucket)
	{
		if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.RouterEvent)
		{
			throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
		}
		
		Name = bucket.GetEventName(Key.Name);
		Type = bucket.GetEnum(Key.Type).As<EventType>();
		SendId = bucket.GetSendId(Key.SendId);
		Origin = bucket.GetFullUri(Key.Origin);
		OriginType = bucket.GetFullUri(Key.OriginType);
		InvokeId = bucket.GetInvokeId(Key.InvokeUniqueId);
		Data = bucket.GetDataModelValue(Key.Data);
		SenderServiceId = bucket.GetServiceId(Key.SenderServiceId) ?? throw Infra.Fail<Exception>();
		IoProcessorData = bucket.GetDataModelValue(Key.RouterEventData).AsNullableList();
		DelayMs = bucket.GetInt32(Key.DelayMs);
		TargetType = bucket.GetFullUri(Key.TargetType);
		Target = bucket.GetFullUri(Key.Target);
	}

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.RouterEvent);
		bucket.AddServiceId(Key.SenderServiceId, SenderServiceId);
		bucket.AddDataModelValue(Key.RouterEventData, IoProcessorData);
		bucket.Add(Key.DelayMs, DelayMs);
		bucket.Add(Key.TargetType, TargetType);
		bucket.Add(Key.TargetType, Target);
		bucket.AddEventName(Key.Name, Name);
		bucket.Add(Key.Type, Type);
		bucket.AddId(Key.SendId, SendId);
		bucket.Add(Key.Origin, Origin);
		bucket.Add(Key.OriginType, OriginType);
		bucket.AddId(Key.InvokeId, InvokeId);
		bucket.AddDataModelValue(Key.Data, Data);
	}
}