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
	public sealed class HttpIoProcessorFactory : IIoProcessorFactory
	{
		private readonly Uri        _baseUri;
		private readonly IPEndPoint _ipEndPoint;

		public HttpIoProcessorFactory(Uri baseUri, IPEndPoint ipEndPoint)
		{
			_baseUri = baseUri;
			_ipEndPoint = ipEndPoint;
		}

	#region Interface IIoProcessorFactory

		public async ValueTask<IIoProcessor> Create(IEventConsumer eventConsumer, CancellationToken token)
		{
			var httpIoProcessor = new HttpIoProcessor(eventConsumer, _baseUri, _ipEndPoint);

			await httpIoProcessor.Start(token).ConfigureAwait(false);

			return httpIoProcessor;
		}

	#endregion
	}
}