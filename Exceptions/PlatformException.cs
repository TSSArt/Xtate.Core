#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class PlatformException : XtateException
	{
		public PlatformException() { }

		public PlatformException(string message) : base(message) { }

		public PlatformException(string message, Exception innerException) : base(message, innerException) { }

		public PlatformException(Exception inner, SessionId sessionId) : base(message: null, inner) => SessionId = sessionId;

		protected PlatformException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public SessionId SessionId { get; } = default!;
	}
}