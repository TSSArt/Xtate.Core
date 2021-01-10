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
using Xtate.Annotations;

namespace Xtate.Core
{
	public enum StateMachineOriginType
	{
		None,
		Scxml,
		Source,
		StateMachine
	}

	[PublicAPI]
	public readonly struct StateMachineOrigin
	{
		private readonly object? _val;

		public StateMachineOrigin(IStateMachine stateMachine, Uri? baseUri = null)
		{
			_val = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
			BaseUri = baseUri;
		}

		public StateMachineOrigin(Uri source, Uri? baseUri = null)
		{
			_val = source ?? throw new ArgumentNullException(nameof(source));
			BaseUri = baseUri;
		}

		public StateMachineOrigin(string scxml, Uri? baseUri = null)
		{
			_val = scxml ?? throw new ArgumentNullException(nameof(scxml));
			BaseUri = baseUri;
		}

		public StateMachineOriginType Type =>
				_val switch
				{
						string => StateMachineOriginType.Scxml,
						Uri => StateMachineOriginType.Source,
						IStateMachine => StateMachineOriginType.StateMachine,
						null => StateMachineOriginType.None,
						_ => Infrastructure.UnexpectedValue<StateMachineOriginType>(_val)
				};

		public Uri? BaseUri { get; }

		public string AsScxml()
		{
			if (_val is string str)
			{
				return str;
			}

			throw new ArgumentException(Resources.Exception_ValueIsNotSCXML);
		}

		public Uri AsSource()
		{
			if (_val is Uri uri)
			{
				return uri;
			}

			throw new ArgumentException(Resources.Exception_ValueIsNotSource);
		}

		public IStateMachine AsStateMachine()
		{
			if (_val is IStateMachine stateMachine)
			{
				return stateMachine;
			}

			throw new ArgumentException(Resources.Exception_ValueIsNotStateMachine);
		}
	}
}