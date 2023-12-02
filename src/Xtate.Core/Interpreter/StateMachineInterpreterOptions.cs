#region Copyright © 2019-2023 Sergii Artemenko

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

using System.Threading.Channels;

namespace Xtate.Core;

public class StateMachineInterpreterOptions : IStateMachineInterpreterOptions
{
	//private readonly IInterpreterModel  _interpreterModel;

	public StateMachineInterpreterOptions(IStateMachineStartOptions stateMachineStartOptions)
	{
		//_interpreterModel = interpreterModel;
		SessionId = stateMachineStartOptions.SessionId;
		options = new InterpreterOptions();
	}

#region Interface IStateMachineInterpreterOptions

	public SessionId             SessionId    { get; }
	public ChannelReader<IEvent> eventChannel { get; }
	public InterpreterOptions    options      { get; }

#endregion

	//public IInterpreterModel     model        => _interpreterModel;
}