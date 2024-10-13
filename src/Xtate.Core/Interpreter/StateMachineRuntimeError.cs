// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Core;

public class StateMachineRuntimeError
{
	private readonly object _stateMachineToken = new();

	public bool IsPlatformError(Exception exception)
	{
		for (var ex = exception; ex is not null; ex = ex.InnerException)
		{
			if (ex is PlatformException platformException)
			{
				if (platformException.Token == _stateMachineToken)
				{
					return true;
				}

				break;
			}
		}

		return false;
	}

	public bool IsCommunicationError(Exception? exception, out SendId? sendId)
	{
		for (var ex = exception; ex is not null; ex = ex.InnerException)
		{
			if (ex is CommunicationException communicationException)
			{
				if (communicationException.Token == _stateMachineToken)
				{
					sendId = communicationException.SendId;

					return true;
				}

				break;
			}
		}

		sendId = default;

		return false;
	}

	public CommunicationException NoExternalCommunication() => new(Resources.Exception_ExternalCommunicationDoesNotConfiguredForStateMachineInterpreter) { Token = _stateMachineToken };

	public CommunicationException CommunicationError(Exception innerException, SendId? sendId = default) => new(innerException, sendId) { Token = _stateMachineToken };

	public PlatformException PlatformError(Exception innerException) => new(innerException) { Token = _stateMachineToken };
}