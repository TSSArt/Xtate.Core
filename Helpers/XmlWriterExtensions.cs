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

#if !NET6_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

namespace Xtate.Core
{
	[PublicAPI]
	public static class XmlWriterExtensions
	{
		public static ConfiguredAwaitable ConfigureAwait(this XmlWriter xmlWriter, bool continueOnCapturedContext) => new(xmlWriter, continueOnCapturedContext);

		[UsedImplicitly]
		public static ValueTask DisposeAsync(this XmlWriter xmlWriter)
		{
			if (xmlWriter is null) throw new ArgumentNullException(nameof(xmlWriter));

			xmlWriter.Dispose();

			return default;
		}

		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types")]
		public readonly struct ConfiguredAwaitable
		{
			private readonly bool _continueOnCapturedContext;

			private readonly XmlWriter _xmlWriter;

			public ConfiguredAwaitable(XmlWriter xmlWriter, bool continueOnCapturedContext)
			{
				_xmlWriter = xmlWriter;
				_continueOnCapturedContext = continueOnCapturedContext;
			}

			public ConfiguredValueTaskAwaitable DisposeAsync() => _xmlWriter.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
		}
	}
}
#endif