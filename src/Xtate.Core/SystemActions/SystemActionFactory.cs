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

namespace Xtate.CustomAction;

<<<<<<< Updated upstream
namespace Xtate.CustomAction
{
	public class StartActionProvider() : CustomActionProvider<StartAction>("http://xtate.net/scxml/system", "start") { }
	public class DestroyActionProvider() : CustomActionProvider<DestroyAction>("http://xtate.net/scxml/system", "destroy") { }

	[PublicAPI]
	public class SystemActionFactory : CustomActionFactoryBase
	{
		private const string Namespace = "http://xtate.net/scxml/system";

		public required Func<StartAction>   StartActionFactory   { private get; init; }
		public required Func<DestroyAction> DestroyActionFactory { private get; init; }

		protected override void Register(ICustomActionCatalog catalog)
		{
			if (catalog is null) throw new ArgumentNullException(nameof(catalog));

			//TODO: uncomment
			//catalog.Register(Namespace, name: @"start", (context, reader) => new StartAction(context, reader));
			//catalog.Register(Namespace, name: @"destroy", (context, reader) => new DestroyAction(context, reader));
		}
	}
}
=======
public class StartActionProvider() : CustomActionProvider<StartAction>(ns: "http://xtate.net/scxml/system", name: "start");

public class DestroyActionProvider() : CustomActionProvider<DestroyAction>(ns: "http://xtate.net/scxml/system", name: "destroy");
>>>>>>> Stashed changes
