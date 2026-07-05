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

using Xtate.IoC.TransformArgs.Internal;

namespace Xtate.IoC.TransformArgs.Services;

[InstantiatedByIoC]
public class ServiceFactoryNewArgsAsync<T, TArg, TNewArg>
{
	public required IEnumerable<IArgsTransformer<T, TArg, TNewArg>> ArgsTransformers { private get; [SetByIoC] init; }

	public required Func<TArg, ValueTask<T>> ServiceFactory { private get; [SetByIoC] init; }

	[CalledByIoC]
	public ValueTask<T> Factory(TNewArg newArg)
	{
		foreach (var argsTransformer in ArgsTransformers)
		{
			if (argsTransformer.CanTransformSync())
			{
				return ServiceFactory(argsTransformer.TransformSync(newArg));
			}

			if (argsTransformer.CanTransformAsync())
			{
				return FactoryAsync(argsTransformer, newArg);
			}
		}

		throw new DependencyInjectionException("There is no suitable args transformer available.");
	}

	private async ValueTask<T> FactoryAsync(IArgsTransformer<T, TArg, TNewArg> argsTransformer, TNewArg newArg)
	{
		var arg = await argsTransformer.TransformAsync(newArg).ConfigureAwait(false);

		return await ServiceFactory(arg).ConfigureAwait(false);
	}
}