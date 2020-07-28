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