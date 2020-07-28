#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class CommunicationException : XtateException
	{
		public CommunicationException() { }

		public CommunicationException(string message) : base(message) { }

		public CommunicationException(string message, Exception innerException) : base(message, innerException) { }

		public CommunicationException(Exception inner, SessionId sessionId, SendId? sendId = default) : base(message: null, inner)
		{
			SessionId = sessionId;
			SendId = sendId;
		}

		protected CommunicationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public SessionId SessionId { get; } = default!;

		public SendId? SendId { get; }
	}
}