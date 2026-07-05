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

using Xtate.IoC;
using Xtate.Scxml.DependencyInjection;
using Xtate.Scxml.Services;
using Xtate.StateMachine;

namespace Xtate.Class;

public class LocationStateMachine : StateMachineClass
{
	public LocationStateMachine(string absoluteUri) : this(new Uri(absoluteUri, UriKind.Absolute)) { }

	public LocationStateMachine(Uri absoluteUri)
	{
		if (!absoluteUri.IsAbsoluteUri)
		{
			throw new ArgumentException(message: "The URI must be absolute.", nameof(absoluteUri));
		}

		Location = absoluteUri;
	}

	public LocationStateMachine(Uri? baseUri, string relativeUri) : this(baseUri is not null ? new Uri(baseUri, relativeUri) : new Uri(relativeUri, UriKind.RelativeOrAbsolute)) { }

	public LocationStateMachine(Uri? baseUri, Uri relativeUri) : this(baseUri is not null ? new Uri(baseUri, relativeUri) : relativeUri) { }

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddModule<ScxmlModule>();
		services.AddFactory<ScxmlLocationStateMachineGetter>().For<IStateMachine>(SharedWithin.Scope);
	}
}