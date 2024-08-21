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

public enum StateMachineOriginType
{
	Scxml,
	Source,
	StateMachine
}

public readonly struct StateMachineOrigin
{
	private readonly object? _value;

	public StateMachineOrigin(IStateMachine stateMachine, Uri? baseUri = default)
	{
		Infra.Requires(stateMachine);

		_value = stateMachine;
		BaseUri = baseUri;
	}

	public StateMachineOrigin(Uri source, Uri? baseUri = default)
	{
		Infra.Requires(source);

		_value = source;
		BaseUri = baseUri;
	}

	public StateMachineOrigin(string scxml, Uri? baseUri = default)
	{
		Infra.Requires(scxml);

		_value = scxml;
		BaseUri = baseUri;
	}

	public StateMachineOriginType Type =>
		_value switch
		{
			string        => StateMachineOriginType.Scxml,
			Uri           => StateMachineOriginType.Source,
			IStateMachine => StateMachineOriginType.StateMachine,
			_             => throw Infra.Unmatched(_value?.GetType())
		};

	public Uri? BaseUri { get; }

	public string AsScxml()
	{
		if (_value is string str)
		{
			return str;
		}

		throw new ArgumentException(Resources.Exception_ValueIsNotSCXML);
	}

	public Uri AsSource()
	{
		if (_value is Uri uri)
		{
			return uri;
		}

		throw new ArgumentException(Resources.Exception_ValueIsNotSource);
	}

	public IStateMachine AsStateMachine()
	{
		if (_value is IStateMachine stateMachine)
		{
			return stateMachine;
		}

		throw new ArgumentException(Resources.Exception_ValueIsNotStateMachine);
	}
}