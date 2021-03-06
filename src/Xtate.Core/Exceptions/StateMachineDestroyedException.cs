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
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class StateMachineDestroyedException : XtateException
	{
		public StateMachineDestroyedException() { }

		public StateMachineDestroyedException(string? message) : base(message) { }

		public StateMachineDestroyedException(string? message, Exception? inner) : base(message, inner) { }

		protected StateMachineDestroyedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}