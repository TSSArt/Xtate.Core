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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
	[SuppressMessage(category: "ReSharper", checkId: "ClassNeverInstantiated.Global")]
	internal sealed class RequiredMemberAttribute : Attribute;

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	[SuppressMessage(category: "ReSharper", checkId: "UnusedType.Global")]
	[ExcludeFromCodeCoverage]
	internal sealed class CompilerFeatureRequiredAttribute : Attribute
	{
		public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

		[SuppressMessage(category: "ReSharper", checkId: "MemberCanBePrivate.Global")]
		[SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global")]
		public string FeatureName { get; }

		[SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global")]
		public string? Language { get; init; }
	}
}

namespace Xtate.IoC.Test
{
	public class ArgClass
	{
		public required int Arg;
	}

	public class DepClass;

	public class DepSyncClass;

	public class Sync2Class
	{
		public required DepSyncClass DepSyncClass;
	}

	public class SyncClass
	{
		public required Func<DepSyncClass> Factory;

		public required Func<int, DepSyncClass> Factory2;

		public required Func<int, long, DepSyncClass> Factory3;

		public required Func<int, long, DepSyncClass?> FactoryOpt3;
	}

	public class Class
	{
		public required Func<int, long, ValueTask<DepClass>> DepClass2ArgsFieldFactory;

		public required Func<int, long, ValueTask<DepClass?>> DepClass2ArgsFieldOptFactory;

		public required Func<int, long, IAsyncEnumerable<DepClass>> DepClass2ArgsFieldServices;

		public required DepClass DepClassField;

		public required Func<ValueTask<DepClass>> DepClassFieldFactory;

		public required DepClass DepClassPropPrivate { private get; init; }

		public required DepClass DepClassProp { get; init; }

		public DepClass DepClassPropPublic => DepClassPropPrivate;
	}

	[TestClass]
	public class RequiredPropertyTest
	{
		[TestMethod]
		public async Task BasicPropertyTest()
		{
			// Arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddType<Class>();
			serviceCollection.AddType<DepClass>();
			serviceCollection.AddType<DepClass, int, long>();
			var serviceProvider = serviceCollection.BuildProvider();

			// Act
			var class1Instance = await serviceProvider.GetRequiredService<Class>();
			var inst = await class1Instance.DepClassFieldFactory();
			var inst2 = await class1Instance.DepClass2ArgsFieldFactory(arg1: 3, arg2: 3);
			var inst3 = await class1Instance.DepClass2ArgsFieldOptFactory(arg1: 3, arg2: 3);

			// Assert
			Assert.IsNotNull(class1Instance);
			Assert.IsNotNull(class1Instance.DepClassProp);
			Assert.IsNotNull(class1Instance.DepClassField);
			Assert.IsNotNull(class1Instance.DepClassPropPublic);
			Assert.IsNotNull(inst);
			Assert.IsNotNull(inst2);
			Assert.IsNotNull(inst3);
			Assert.IsNotNull(class1Instance.DepClass2ArgsFieldServices);
		}

		[TestMethod]
		public async Task BasicSyncPropertyTest()
		{
			// Arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddTypeSync<SyncClass>();
			serviceCollection.AddTypeSync<DepSyncClass>();
			serviceCollection.AddTypeSync<DepSyncClass, int>();
			serviceCollection.AddTypeSync<DepSyncClass, int, long>();
			var serviceProvider = serviceCollection.BuildProvider();

			// Act
			var class1Instance = await serviceProvider.GetRequiredService<SyncClass>();
			var inst1 = class1Instance.Factory();
			var inst2 = class1Instance.Factory2(1);
			var inst3 = class1Instance.Factory3(arg1: 1, arg2: 4);
			var inst4 = class1Instance.FactoryOpt3(arg1: 1, arg2: 4);

			// Assert
			Assert.IsNotNull(inst1);
			Assert.IsNotNull(inst2);
			Assert.IsNotNull(inst3);
			Assert.IsNotNull(inst4);
		}

		[TestMethod]
		public async Task InvalidSyncPropertyTest()
		{
			// Arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddTypeSync<Sync2Class>();
			serviceCollection.AddType<DepSyncClass>();
			var serviceProvider = serviceCollection.BuildProvider();

			// Act

			// Assert
			await Assert.ThrowsExceptionAsync<DependencyInjectionException>([ExcludeFromCodeCoverage] async () => await serviceProvider.GetRequiredService<Sync2Class>());
		}

		[TestMethod]
		public async Task ArgAsyncPropertyTest()
		{
			// Arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddType<ArgClass, int>();
			var serviceProvider = serviceCollection.BuildProvider();

			// Act
			var class1Instance = await serviceProvider.GetRequiredService<ArgClass, int>(55);

			// Assert
			Assert.AreEqual(expected: 55, class1Instance.Arg);
		}

		[TestMethod]
		public async Task ArgPropertyTest()
		{
			// Arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddTypeSync<ArgClass, int>();
			var serviceProvider = serviceCollection.BuildProvider();

			// Act
			var class1Instance = await serviceProvider.GetRequiredService<ArgClass, int>(55);

			// Assert
			Assert.AreEqual(expected: 55, class1Instance.Arg);
		}
	}
}