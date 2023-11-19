#region Copyright © 2019-2022 Sergii Artemenko

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

namespace Xtate.Core.IoC;

public sealed class ImplementationAsyncFactoryProvider<T, TArg> : ClassFactoryProvider<T, T>, IDelegateFactory
{
	private static readonly ImplementationAsyncFactoryProvider<T, TArg> Instance = new();

	public static readonly Delegate Delegate = IsResolvedType() ? GetServiceAsync<TArg> : GetResolver;

	private static IDelegateFactory GetResolver() => Instance;

	public override Delegate GetDelegate() => Delegate;

	public Delegate? GetDelegate<TResolved>() => Resolved<TResolved>.Delegate;

	private static class Resolved<TResolved>
	{
		public static Delegate? Delegate = GetDelegate();

		private static Delegate? GetDelegate()
		{
			if (!IsMatch(typeof(TResolved)))
			{
				return default;
			}

			var factoryProviderType = typeof(ImplementationAsyncFactoryProvider<,>).MakeGenericType(typeof(TResolved), typeof(TArg));

			return ((FactoryProvider) Activator.CreateInstance(factoryProviderType)!).GetDelegate();
		}
	}
}