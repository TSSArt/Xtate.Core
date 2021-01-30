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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.IoProcessor
{
	[PublicAPI]
	public sealed class NamedIoProcessorFactory : IIoProcessorFactory
	{
		private const int DefaultMaxMessageSize = 1024 * 1024;
		private const int FreeSlotsCount        = 2;

		private static readonly string HostName = GetHostName();

		private readonly string _host;
		private readonly int?   _maxMessageSize;
		private readonly string _name;

		public NamedIoProcessorFactory(string name, int? maxMessageSize = default)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			_name = name;
			_maxMessageSize = maxMessageSize;
			_host = HostName;
		}

		public NamedIoProcessorFactory(string host, string name, int? maxMessageSize = default)
		{
			if (string.IsNullOrEmpty(host)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(host));
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			_host = host;
			_name = name;
			_maxMessageSize = maxMessageSize;
		}

	#region Interface IIoProcessorFactory

		public async ValueTask<IIoProcessor> Create(IEventConsumer eventConsumer, CancellationToken token)
		{
			if (eventConsumer is null) throw new ArgumentNullException(nameof(eventConsumer));

			var processor = new NamedIoProcessor(eventConsumer, _host, _name, _maxMessageSize ?? DefaultMaxMessageSize);

			for (var i = 0; i < FreeSlotsCount; i ++)
			{
				processor.StartListener().Forget();
			}

			await processor.CheckPipeline(token).ConfigureAwait(false);

			return processor;
		}

	#endregion

		private static string GetHostName()
		{
			try
			{
				return Dns.GetHostName();
			}
			catch
			{
				return @".";
			}
		}
	}
}