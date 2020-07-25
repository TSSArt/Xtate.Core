using System;
using System.Globalization;
using System.Text;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public class ErrorItem
	{
		public ErrorItem(Type source, string message, Exception? exception, int lineNumber = 0, int linePosition = 0)
		{
			Source = source;
			Message = message;
			Exception = exception;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		public ErrorSeverity Severity     { get; } = ErrorSeverity.Error;
		public Type          Source       { get; }
		public string        Message      { get; }
		public Exception?    Exception    { get; }
		public int           LineNumber   { get; }
		public int           LinePosition { get; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat(CultureInfo.InvariantCulture, format: @"{0}: [{1}] ", Severity, Source.Name);

			if (LineNumber > 0)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, format: @"(Ln: {0}, Col: {1}) ", LineNumber, LinePosition);
			}

			sb.Append(Message);

			if (Exception != null)
			{
				sb.Append(@"\r\n\tException ==> ").Append(Exception);
			}

			return sb.ToString();
		}
	}
}