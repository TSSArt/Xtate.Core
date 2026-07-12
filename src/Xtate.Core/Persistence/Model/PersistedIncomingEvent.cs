// Copyright © 2019-2026 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Xtate.Interpreter;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Persistence;

public class PersistedIncomingEvent : IncomingEvent, IStoreSupport
{
	public PersistedIncomingEvent(in Bucket bucket)
	{
		if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.EventObject)
		{
			throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
		}

		Name = bucket.GetEventName(Key.Name);
		Type = bucket.GetEnum(Key.Type).As<EventType>();
		SendId = bucket.GetSendId(Key.SendId);
		Origin = bucket.GetFullUri(Key.Origin);
		OriginType = bucket.GetFullUri(Key.OriginType);
		InvokeId = bucket.GetInvokeId(Key.InvokeId);
		Data = bucket.GetDataModelValue(Key.Data);
	}

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.EventObject);
		bucket.AddEventName(Key.Name, Name);
		bucket.Add(Key.Type, Type);
		bucket.AddId(Key.SendId, SendId);
		bucket.Add(Key.Origin, Origin);
		bucket.Add(Key.OriginType, OriginType);
		bucket.AddId(Key.InvokeId, InvokeId);
		bucket.AddDataModelValue(Key.Data, Data);
	}

#endregion
}