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

using System.Runtime.CompilerServices;

namespace Xtate.Core.IoC;

public interface ITypeKeyAction
{
	void TypedAction<T>(TypeKey typeKey);
}

public abstract class SimpleTypeKey : TypeKey { }

public abstract class GenericTypeKey : TypeKey
{
	public abstract SimpleTypeKey DefinitionKey { get; }
}

public abstract class TypeKey
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TypeKey ServiceKey<T, TArg>() => Service<T, TArg>.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TypeKey ImplementationKey<T, TArg>() => Implementation<T, TArg>.Value;

	private static string ToString<T, TArg>(string typeName, T type) => @$"{typeName} {{Type = {type}, ArgumentType = {ArgumentType.TypeOf<TArg>()}}}";

	public abstract void DoTypedAction(ITypeKeyAction typeKeyAction);

	private static class Service<T, TArg>
	{
		public static readonly TypeKey Value = ServiceType.TypeOf<T>().IsClosedGeneric ? new Generic() : new Simple();

		private class Simple : SimpleTypeKey
		{
			public override string ToString() => ToString<ServiceType, TArg>(@"SimpleService", ServiceType.TypeOf<T>());

			public override void DoTypedAction(ITypeKeyAction typeKeyAction) => Infra.Fail();
		}

		private class Generic : GenericTypeKey
		{
			public override SimpleTypeKey DefinitionKey { get; } = new DefinitionServiceTypeKey<TArg>(ServiceType.TypeOf<T>().Definition);

			public override string ToString() => ToString<ServiceType, TArg>(@"GenericService", ServiceType.TypeOf<T>());

			public override void DoTypedAction(ITypeKeyAction typeKeyAction) => typeKeyAction.TypedAction<T>(this);
		}
	}

	private static class Implementation<T, TArg>
	{
		public static readonly TypeKey Value = ImplementationType.TypeOf<T>().IsClosedGeneric ? new Generic() : new Simple();

		private class Simple : SimpleTypeKey
		{
			public override string ToString() => ToString<ImplementationType, TArg>(@"SimpleImplementation", ImplementationType.TypeOf<T>());

			public override void DoTypedAction(ITypeKeyAction typeKeyAction) => Infra.Fail();
		}

		private class Generic : GenericTypeKey
		{
			public override SimpleTypeKey DefinitionKey { get; } = new DefinitionImplementationTypeKey<TArg>(ImplementationType.TypeOf<T>().Definition);

			public override string ToString() => ToString<ImplementationType, TArg>(@"GenericImplementation", ImplementationType.TypeOf<T>());

			public override void DoTypedAction(ITypeKeyAction typeKeyAction) => typeKeyAction.TypedAction<T>(this);
		}
	}

	private class DefinitionServiceTypeKey<TArg> : SimpleTypeKey
	{
		private readonly ServiceType _openGeneric;

		public DefinitionServiceTypeKey(in ServiceType openGeneric) => _openGeneric = openGeneric;

		public override int GetHashCode() => _openGeneric.GetHashCode();

		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is DefinitionServiceTypeKey<TArg> other && _openGeneric == other._openGeneric);

		public override string ToString() => ToString<ServiceType, TArg>(@"DefinitionService", _openGeneric);

		public override void DoTypedAction(ITypeKeyAction typeKeyAction) => Infra.Fail();
	}

	private class DefinitionImplementationTypeKey<TArg> : SimpleTypeKey
	{
		private readonly ImplementationType _openGeneric;

		public DefinitionImplementationTypeKey(in ImplementationType openGeneric) => _openGeneric = openGeneric;

		public override int GetHashCode() => _openGeneric.GetHashCode();

		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is DefinitionImplementationTypeKey<TArg> other && _openGeneric == other._openGeneric);

		public override string ToString() => ToString<ImplementationType, TArg>(@"DefinitionImplementation", _openGeneric);

		public override void DoTypedAction(ITypeKeyAction typeKeyAction) => Infra.Fail();
	}
}