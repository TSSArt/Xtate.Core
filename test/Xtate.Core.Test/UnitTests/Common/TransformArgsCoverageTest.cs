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
using Xtate.IoC.DependencyInjection;
using Xtate.IoC.TransformArgs.DependencyInjection;
using Xtate.IoC.TransformArgs.Internal;
using Xtate.IoC.TransformArgs.Services;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class TransformArgsCoverageTest
{
	[TestMethod]
	public async Task SyncAndAsyncTransformersReportCapabilitiesAndTransformArguments()
	{
		var sync = new ArgsTransformerSync<Result, int, string>(static value => value.Length);
		var asyncTransformer = new ArgsTransformerAsync<Result, int, string>(static value => new ValueTask<int>(value.Length * 2));

		Assert.IsTrue(sync.CanTransformSync());
		Assert.IsTrue(sync.CanTransformAsync());
		Assert.AreEqual(expected: 3, sync.TransformSync("abc"));
		Assert.AreEqual(expected: 4, await sync.TransformAsync("four"));

		Assert.IsFalse(asyncTransformer.CanTransformSync());
		Assert.IsTrue(asyncTransformer.CanTransformAsync());
		Assert.AreEqual(expected: 6, await asyncTransformer.TransformAsync("abc"));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => asyncTransformer.TransformSync("unsupported"));
	}

	[TestMethod]
	public void SynchronousFactoryUsesFirstSyncTransformerAndRejectsAsyncOrMissingTransformer()
	{
		var skipped = new DisabledTransformer<Result, int, string>();
		var sync = new ArgsTransformerSync<Result, int, string>(static value => value.Length);
		var factory = new ServiceFactoryNewArgsSync<Result, int, string>
					  {
						  ArgsTransformers = [skipped, sync],
						  ServiceFactory = static value => new Result(value)
					  };

		Assert.AreEqual(expected: 5, factory.Factory("value").Value);

		var asyncOnly = new ServiceFactoryNewArgsSync<Result, int, string>
						{
							ArgsTransformers = [new ArgsTransformerAsync<Result, int, string>(static value => new ValueTask<int>(value.Length))],
							ServiceFactory = static value => new Result(value)
						};
		var missing = new ServiceFactoryNewArgsSync<Result, int, string>
					  {
						  ArgsTransformers = [],
						  ServiceFactory = static value => new Result(value)
					  };

		var asyncException = Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => asyncOnly.Factory("value"));
		var missingException = Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => missing.Factory("value"));
		StringAssert.Contains(asyncException.Message, "Async args transformer");
		StringAssert.Contains(missingException.Message, "no suitable args transformer", StringComparison.OrdinalIgnoreCase);
	}

	[TestMethod]
	public async Task AsynchronousFactoryUsesSyncOrAsyncTransformersAndRejectsMissingTransformer()
	{
		var syncFactory = new ServiceFactoryNewArgsAsync<Result, int, string>
						  {
							  ArgsTransformers = [new ArgsTransformerSync<Result, int, string>(static value => value.Length)],
							  ServiceFactory = static value => new ValueTask<Result>(new Result(value))
						  };
		var asyncFactory = new ServiceFactoryNewArgsAsync<Result, int, string>
						   {
							   ArgsTransformers =
							   [
								   new DisabledTransformer<Result, int, string>(),
								   new ArgsTransformerAsync<Result, int, string>(static value => new ValueTask<int>(value.Length * 2))
							   ],
							   ServiceFactory = static value => new ValueTask<Result>(new Result(value))
						   };
		var missing = new ServiceFactoryNewArgsAsync<Result, int, string>
					  {
						  ArgsTransformers = [new DisabledTransformer<Result, int, string>()],
						  ServiceFactory = static value => new ValueTask<Result>(new Result(value))
					  };

		Assert.AreEqual(expected: 4, (await syncFactory.Factory("four")).Value);
		Assert.AreEqual(expected: 10, (await asyncFactory.Factory("value")).Value);
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => missing.Factory("value"));
	}

	[TestMethod]
	public void SyncSelectorRegistersValueFactoryAndAllTupleTransforms()
	{
		var services = new ServiceCollection();

		services.ForServiceSync<Result, int>().UseArgValue(7).IfAncestor<Ancestor>();
		services.ForServiceSync<Result, int>().UseArgFactory(static () => 8).IfAncestor<Ancestor>();
		services.ForServiceSync<Result, int>().TransformArgs<string>(static value => value.Length).IfAncestor<Ancestor>();
		services.ForServiceSync<Result, int>().TransformArgs<string, bool>(static (value, flag) => flag ? value.Length : 0).IfAncestor<Ancestor>();
		services.ForServiceSync<Result, int>().TransformArgs<string, bool, int>(static (value, flag, add) => flag ? value.Length + add : 0).IfAncestor<Ancestor>();
		services.ForServiceSync<Result, int>().TransformArgs<string, bool, int, int>(static (value, flag, add, multiply) => flag ? (value.Length + add) * multiply : 0).IfAncestor<Ancestor>();

		_ = services.ForServiceSync<Result>();
		_ = services.ForServiceSync<Result, string, bool>();
		_ = services.ForServiceSync<Result, string, bool, int>();
		_ = services.ForServiceSync<Result, string, bool, int, long>();

		Assert.IsGreaterThan(0, services.Count());
	}

	[TestMethod]
	public void AsyncSelectorRegistersValueFactoriesAndSyncAndAsyncTupleTransforms()
	{
		var services = new ServiceCollection();

		services.ForService<Result, int>().UseArgValue(7).IfAncestor<Ancestor>();
		services.ForService<Result, int>().UseArgFactory(static () => 8).IfAncestor<Ancestor>();
		services.ForService<Result, int>().UseArgFactory(static () => new ValueTask<int>(9)).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string>(static value => value.Length).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string>(static value => new ValueTask<int>(value.Length)).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string, bool>(static (value, flag) => flag ? value.Length : 0).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string, bool>(static (value, flag) => new ValueTask<int>(flag ? value.Length : 0)).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string, bool, int>(static (value, flag, add) => flag ? value.Length + add : 0).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string, bool, int>(static (value, flag, add) => new ValueTask<int>(flag ? value.Length + add : 0)).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string, bool, int, int>(static (value, flag, add, multiply) => flag ? (value.Length + add) * multiply : 0).IfAncestor<Ancestor>();
		services.ForService<Result, int>().TransformArgs<string, bool, int, int>(static (value, flag, add, multiply) => new ValueTask<int>(flag ? (value.Length + add) * multiply : 0)).IfAncestor<Ancestor>();

		_ = services.ForService<Result>();
		_ = services.ForService<Result, string, bool>();
		_ = services.ForService<Result, string, bool, int>();
		_ = services.ForService<Result, string, bool, int, long>();

		Assert.IsGreaterThan(0, services.Count());
	}

	private sealed record Result(int Value);

	private sealed class Ancestor;

	private sealed class DisabledTransformer<T, TArg, TNewArg> : IArgsTransformer<T, TArg, TNewArg>
	{
		public bool CanTransformSync() => false;

		public bool CanTransformAsync() => false;

		public TArg TransformSync(TNewArg newArg) => default!;

		public ValueTask<TArg> TransformAsync(TNewArg newArg) => new(default(TArg)!);
	}
}
