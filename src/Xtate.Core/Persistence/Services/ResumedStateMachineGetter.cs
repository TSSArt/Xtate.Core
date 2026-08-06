// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.StateMachine;

namespace Xtate.Persistence.Services;

public class ResumedStateMachineGetter
{
	private readonly IStateMachine? _stateMachine;

	private readonly IStateMachineArguments _stateMachineArguments;

	private readonly IStateMachineLocation _stateMachineLocation;

	public ResumedStateMachineGetter(IStateMachineSessionId sessionId, IStorage storage, Func<ReadOnlyMemory<byte>, InMemoryStorage> storageFactory)
	{
		var bucket = new Bucket(storage);

		if (bucket.TryGet(Key.Version, out int version) && version != 1)
		{
			throw new PersistenceException(Resources.Exception_PersistedStateCantBeReadUnsupportedVersion);
		}

		var storedSessionId = bucket.GetSessionId(Key.SessionId);

		if (storedSessionId is not null && storedSessionId != sessionId.SessionId)
		{
			throw new PersistenceException(Resources.Exception_PersistedStateCantBeReadStoredAndProvidedSessionIdsDoesNotMatch);
		}

		_stateMachineLocation = new Location(bucket.GetUri(Key.Location));

		_stateMachineArguments = new Arguments(bucket.GetDataModelValue(Key.Arguments));

		_stateMachine = bucket.TryGet(Key.StateMachineDefinition, out var memory) ? new StateMachineReader().Build(new Bucket(storageFactory(memory))) : null;
	}

	[CalledByIoC]
	public IStateMachine GetStateMachine() => _stateMachine ?? throw new PersistenceException(Resources.Exception_StateMachineDefinitionIsMissing);

	[CalledByIoC]
	public IStateMachineLocation GetStateMachineLocation() => _stateMachineLocation;

	[CalledByIoC]
	public IStateMachineArguments GetStateMachineArguments() => _stateMachineArguments;

	private class Location(Uri? location) : IStateMachineLocation
	{
	#region Interface IStateMachineLocation

		Uri? IStateMachineLocation.Location => location;

	#endregion
	}

	private class Arguments(DataModelValue arguments) : IStateMachineArguments
	{
	#region Interface IStateMachineArguments

		DataModelValue IStateMachineArguments.Arguments => arguments;

	#endregion
	}
}