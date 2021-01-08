#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Runtime.Serialization;
using System.Text;
using Xtate.Core;

namespace Xtate
{
	[Serializable]
	public class StateMachineValidationException : XtateException
	{
		protected StateMachineValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public StateMachineValidationException(ImmutableArray<ErrorItem> validationMessages, SessionId? sessionId = default, StateMachineOrigin origin = default)
				: base(GetMessage(validationMessages))
		{
			Origin = origin;
			SessionId = sessionId;
			ValidationMessages = validationMessages;
		}

		public SessionId?                SessionId          { get; }
		public StateMachineOrigin        Origin             { get; }
		public ImmutableArray<ErrorItem> ValidationMessages { get; }

		private static string? GetMessage(ImmutableArray<ErrorItem> validationMessages)
		{
			if (validationMessages.IsDefaultOrEmpty)
			{
				return null;
			}

			if (validationMessages.Length == 1)
			{
				return validationMessages[0].ToString();
			}

			var sb = new StringBuilder();
			var index = 1;
			foreach (var error in validationMessages)
			{
				if (index > 1)
				{
					sb.AppendLine();
				}

				sb.Append(Res.Format(Resources.Exception_StateMachineValidationException_Message, index ++, validationMessages.Length, error));
			}

			return sb.ToString();
		}
	}
}