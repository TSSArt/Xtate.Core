using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text;

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