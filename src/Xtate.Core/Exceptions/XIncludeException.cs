#region Copyright � 2019-2021 Sergii Artemenko

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
using System.Xml;

namespace Xtate.XInclude
{
	[Serializable]
	public class XIncludeException : XtateException, ISerializable
	{
		public XIncludeException() { }

		public XIncludeException(string? message) : base(message) { }

		public XIncludeException(string? message, XmlReader? xmlReader) : base(AddLocationInfo(message, xmlReader)) => Init(xmlReader);

		public XIncludeException(string? message, Exception? inner) : base(message, inner) { }

		protected XIncludeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			LineNumber = info.GetInt32(@"LineNumber");
			LinePosition = info.GetInt32(@"LinePosition");
			Location = info.GetString(@"Location");
		}

		public int? LineNumber { get; private set; }

		public int? LinePosition { get; private set; }

		public string? Location { get; private set; }

	#region Interface ISerializable

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(name: @"LineNumber", LineNumber);
			info.AddValue(name: @"LinePosition", LinePosition);
			info.AddValue(name: @"Location", Location);
		}

	#endregion

		private static string? AddLocationInfo(string? message, XmlReader? xmlReader)
		{
			if (xmlReader is null)
			{
				return message;
			}

			if (xmlReader.BaseURI is { } baseURI && !string.IsNullOrEmpty(baseURI))
			{
				if (xmlReader is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo())
				{
					return Res.Format(Resources.Exception_XIncludeException_Location_Line_Position, message, baseURI, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
				}

				return Res.Format(Resources.Exception_XIncludeException_Location, message, baseURI);
			}
			else
			{
				if (xmlReader is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo())
				{
					return Res.Format(Resources.Exception_XIncludeException_Line_Position, message, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
				}

				return message;
			}
		}

		private void Init(XmlReader? xmlReader)
		{
			if (xmlReader?.BaseURI is { } baseURI)
			{
				Location = baseURI;
			}

			if (xmlReader is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo())
			{
				LineNumber = xmlLineInfo.LineNumber;
				LinePosition = xmlLineInfo.LinePosition;
			}
		}
	}
}