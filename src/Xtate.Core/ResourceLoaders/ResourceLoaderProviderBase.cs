<<<<<<< Updated upstream
﻿using System;
using System.Threading.Tasks;
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko

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
>>>>>>> Stashed changes

namespace Xtate.Core;

public abstract class ResourceLoaderProviderBase<TResourceLoader> : IResourceLoaderProvider where TResourceLoader : class, IResourceLoader
{
<<<<<<< Updated upstream
	private readonly Func<ValueTask<TResourceLoader>> _factory;

	protected ResourceLoaderProviderBase(Func<ValueTask<TResourceLoader>> factory) => _factory = factory;

#region Interface IResourceLoaderProvider

	public async ValueTask<IResourceLoader?> TryGetResourceLoader(Uri uri)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));

		return CanHandle(uri) ? await _factory().ConfigureAwait(false) : default;
	}
=======
	public required Func<ValueTask<TResourceLoader>> ResourceLoaderFactory { private get; [UsedImplicitly] init; }

#region Interface IResourceLoaderProvider

	public async ValueTask<IResourceLoader?> TryGetResourceLoader(Uri uri) => CanHandle(uri) ? await ResourceLoaderFactory().ConfigureAwait(false) : default;
>>>>>>> Stashed changes

#endregion

	protected abstract bool CanHandle(Uri uri);
}