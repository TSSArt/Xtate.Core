﻿// Copyright © 2019-2024 Sergii Artemenko
// 
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

using Xtate.IoProcessor;

namespace Xtate.Core;

[Obsolete]
public class IoProcessorService
{
	public required ServiceList<IEventRouter> IoProcessors { private get; [UsedImplicitly] init; }

	public required ExternalServiceEventRouter ExternalServiceEventRouter { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public IEventRouter GetIoProcessor(FullUri? type)
	{
	/*	if (ExternalServiceCollection.CanHandle(type))
		{
			return ExternalServiceCollection;
		}*/

		foreach (var ioProcessor in IoProcessors)
		{
			if (ioProcessor.CanHandle(type))
			{
				return ioProcessor;
			}
		}

		throw new ProcessorException(Res.Format(Resources.Exception_InvalidType, type));
	}
}