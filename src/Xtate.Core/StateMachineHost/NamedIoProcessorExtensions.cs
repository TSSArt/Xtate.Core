﻿#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
using Xtate.Annotations;
using Xtate.IoProcessor;

namespace Xtate
{
	[PublicAPI]
	public static class NamedIoProcessorExtensions
	{
		public static StateMachineHostBuilder AddNamedIoProcessor(this StateMachineHostBuilder builder, string name)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			builder.AddIoProcessorFactory(new NamedIoProcessorFactory(name));

			return builder;
		}

		public static StateMachineHostBuilder AddNamedIoProcessor(this StateMachineHostBuilder builder, string host, string name)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			builder.AddIoProcessorFactory(new NamedIoProcessorFactory(host, name));

			return builder;
		}
	}
}