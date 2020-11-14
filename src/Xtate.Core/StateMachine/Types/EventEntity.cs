#region Copyright © 2019-2020 Sergii Artemenko

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
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public struct EventEntity : IOutgoingEvent
	{
		public static readonly Uri InternalTarget = new(uriString: "_internal", UriKind.Relative);
		public static readonly Uri ParentTarget   = new(uriString: "_parent", UriKind.Relative);

		public EventEntity(string? val) : this()
		{
			if (!string.IsNullOrEmpty(val))
			{
				NameParts = EventName.ToParts(val);
			}
		}

		public string? RawData { get; set; }

	#region Interface IOutgoingEvent

		public DataModelValue              Data      { get; set; }
		public int                         DelayMs   { get; set; }
		public ImmutableArray<IIdentifier> NameParts { get; set; }
		public SendId?                     SendId    { get; set; }
		public Uri?                        Target    { get; set; }
		public Uri?                        Type      { get; set; }

	#endregion
	}
}