#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

namespace Xtate
{
	internal sealed class EventNode : IOutgoingEvent, IStoreSupport, IAncestorProvider
	{
		private readonly IOutgoingEvent _event;

		public EventNode(IOutgoingEvent evt) => _event = evt;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _event;

	#endregion

	#region Interface IOutgoingEvent

		public ImmutableArray<IIdentifier> NameParts => _event.NameParts;
		public SendId?                     SendId    => _event.SendId;
		public DataModelValue              Data      => _event.Data;
		public Uri?                        Target    => _event.Target;
		public Uri?                        Type      => _event.Type;
		public int                         DelayMs   => _event.DelayMs;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.Id, EventName.ToName(_event.NameParts));
		}

	#endregion
	}
}