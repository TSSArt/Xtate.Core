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

using System.Xml;
using Xtate.XInclude;

namespace Xtate.Scxml;

public class ScxmlDeserializer : IScxmlDeserializer
{
	public required Func<XmlReader, ValueTask<ScxmlDirector>>  ScxmlDirectorFactory  { private get; [UsedImplicitly] init; }
	public required Func<XmlReader, ValueTask<XIncludeReader>> XIncludeReaderFactory { private get; [UsedImplicitly] init; }
	public required IStateMachineValidator                     StateMachineValidator { private get; [UsedImplicitly] init; }
	public required IXIncludeOptions?                          XIncludeOptions       { private get; [UsedImplicitly] init; }

#region Interface IScxmlDeserializer

	public async ValueTask<IStateMachine> Deserialize(XmlReader xmlReader)
	{
		if (XIncludeOptions?.XIncludeAllowed == true)
		{
			xmlReader = await XIncludeReaderFactory(xmlReader).ConfigureAwait(false);
		}

		var scxmlDirector = await ScxmlDirectorFactory(xmlReader).ConfigureAwait(false);

		var stateMachine = await scxmlDirector.ConstructStateMachine().ConfigureAwait(false);

		StateMachineValidator.Validate(stateMachine);

		return stateMachine;
	}

#endregion
}