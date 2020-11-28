#region Copyright © 2019-2020 Sergii Artemenko

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

namespace Xtate.CustomAction
{
	[PublicAPI]
	public class SystemActionFactory : CustomActionFactoryBase
	{
		private const string Namespace = "http://xtate.net/scxml/system";

		public static ICustomActionFactory Instance { get; } = new SystemActionFactory();

		protected override void Register(ICustomActionCatalog catalog)
		{
			if (catalog is null) throw new ArgumentNullException(nameof(catalog));

			catalog.Register(Namespace, name: @"start", (context, reader) => new StartAction(context, reader));
			catalog.Register(Namespace, name: @"destroy", (context, reader) => new DestroyAction(context, reader));
		}
	}
}