﻿#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class EventNode : IOutgoingEvent, IStoreSupport, IAncestorProvider
	{
		private readonly IOutgoingEvent _outgoingEvent;

		public EventNode(IOutgoingEvent outgoingEvent) => _outgoingEvent = outgoingEvent;

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _outgoingEvent;

	#endregion

	#region Interface IOutgoingEvent

		public ImmutableArray<IIdentifier> NameParts => _outgoingEvent.NameParts;
		public SendId?                     SendId    => _outgoingEvent.SendId;
		public DataModelValue              Data      => _outgoingEvent.Data;
		public Uri?                        Target    => _outgoingEvent.Target;
		public Uri?                        Type      => _outgoingEvent.Type;
		public int                         DelayMs   => _outgoingEvent.DelayMs;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.Id, EventName.ToName(_outgoingEvent.NameParts));
		}

	#endregion
	}
}