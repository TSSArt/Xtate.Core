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

namespace Xtate.IoC.TransformArgs.Internal;

public readonly struct ServiceSelectorSync<T, TArg>(IServiceCollection services) where T : notnull
{
	public TransformArgs<T, TArg, Empty> UseArgValue(TArg constant) => TransformArgs<Empty>(_ => constant);

	public TransformArgs<T, TArg, Empty> UseArgFactory(Func<TArg> factory) => TransformArgs<Empty>(_ => factory());

	public TransformArgs<T, TArg, TNewArg> TransformArgs<TNewArg>(Func<TNewArg, TArg> transform) => new(synchronous: true, services, transform);

	public TransformArgs<T, TArg, (TNewArg1, TNewArg2)> TransformArgs<TNewArg1, TNewArg2>(Func<TNewArg1, TNewArg2, TArg> transform) =>
		TransformArgs<(TNewArg1, TNewArg2)>(newArgs => transform(newArgs.Item1, newArgs.Item2));

	public TransformArgs<T, TArg, (TNewArg1, TNewArg2, TNewArg3)> TransformArgs<TNewArg1, TNewArg2, TNewArg3>(Func<TNewArg1, TNewArg2, TNewArg3, TArg> transform) =>
		TransformArgs<(TNewArg1, TNewArg2, TNewArg3)>(newArgs => transform(newArgs.Item1, newArgs.Item2, newArgs.Item3));

	public TransformArgs<T, TArg, (TNewArg1, TNewArg2, TNewArg3, TNewArg4)> TransformArgs<TNewArg1, TNewArg2, TNewArg3, TNewArg4>(Func<TNewArg1, TNewArg2, TNewArg3, TNewArg4, TArg> transform) =>
		TransformArgs<(TNewArg1, TNewArg2, TNewArg3, TNewArg4)>(newArgs => transform(newArgs.Item1, newArgs.Item2, newArgs.Item3, newArgs.Item4));
}