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

namespace Xtate.IoC;

internal static class DecoratorSyncFactoryProvider<TImplementation, TService, TArg>
{
	public static Delegate Delegate() => Infra.TypeInitHandle(() => Nested.DelegateField);

	private static Delegate GetDelegate()
	{
		if (typeof(TService).IsAssignableFrom(typeof(TImplementation)))
		{
			return ClassSyncFactoryProvider<TImplementation, TService>.GetDecoratorServiceDelegate<TArg>();
		}

		throw new DependencyInjectionException(string.Format(Resources.Exception_TypeCantBeCastedTo, typeof(TImplementation), typeof(TService)));
	}

	private static class Nested
	{
		[SuppressMessage(category: "ReSharper", checkId: "StaticMemberInGenericType")]
		public static readonly Delegate DelegateField = GetDelegate();
	}
}