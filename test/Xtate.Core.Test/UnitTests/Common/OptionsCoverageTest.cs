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
using Xtate.IoC.Options;
using Xtate.IoC.Options.DependencyInjection;
using Xtate.IoC.Options.Internal;
using Xtate.IoC.Options.Services;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class OptionsCoverageTest
{
	[TestMethod]
	public async Task ConfigureAsyncInvokesProvidedDelegate()
	{
		var options = new TestOptions();
		var configure = new ConfigureAsync<TestOptions>(static value =>
														{
															value.Entries.Add("configured");

															return ValueTask.CompletedTask;
														});

		await configure.Configure(options);

		CollectionAssert.AreEqual(new[] { "configured" }, options.Entries);
	}

	[TestMethod]
	public async Task ConfigureSyncInvokesProvidedDelegateAndReturnsCompletedTask()
	{
		var options = new TestOptions();
		var configure = new ConfigureSync<TestOptions>(static value => value.Entries.Add("sync-configured"));

		var task = configure.Configure(options);

		Assert.IsTrue(task.IsCompletedSuccessfully);
		await task;
		CollectionAssert.AreEqual(new[] { "sync-configured" }, options.Entries);
	}

	[TestMethod]
	public async Task OptionsAsyncAppliesConfiguratorsThenPostConfiguratorsAndCachesInstance()
	{
		var configureCount = 0;
		var configure = new ConfigureAsync<TestOptions>(value =>
														{
															configureCount ++;
															value.Entries.Add("configure");

															return ValueTask.CompletedTask;
														});
		var postConfigure = new PostConfigure(static value =>
											  {
												  value.Entries.Add("post-configure");

												  return ValueTask.CompletedTask;
											  });
		var options = new OptionsAsyncImpl<TestOptions>
					  {
						  Configurators = ToAsyncEnumerable<IConfigureOptions<TestOptions>>(configure),
						  PostConfigurators = ToAsyncEnumerable<IPostConfigureOptions<TestOptions>>(postConfigure)
					  };

		var first = await options.GetValue();
		var second = await options.GetValue();

		Assert.AreSame(first, second);
		Assert.AreEqual(expected: 1, configureCount);
		CollectionAssert.AreEqual(new[] { "configure", "post-configure" }, first.Entries);
	}

	[TestMethod]
	public async Task ServiceCollectionConfigureRegistersSynchronousConfigurator()
	{
		var services = new ServiceCollection();
		services.Configure<TestOptions>(static options => options.Entries.Add("sync"));
		var provider = services.BuildProvider();
		var configure = await provider.GetRequiredService<IConfigureOptions<TestOptions>>();
		var options = new TestOptions();

		await configure.Configure(options);

		CollectionAssert.AreEqual(new[] { "sync" }, options.Entries);
	}

	[TestMethod]
	public async Task ServiceCollectionConfigureRegistersAsynchronousConfigurator()
	{
		var services = new ServiceCollection();
		services.Configure<TestOptions>(static options =>
										{
											options.Entries.Add("async");

											return ValueTask.CompletedTask;
										});
		var provider = services.BuildProvider();
		var configure = await provider.GetRequiredService<IConfigureOptions<TestOptions>>();
		var options = new TestOptions();

		await configure.Configure(options);

		CollectionAssert.AreEqual(new[] { "async" }, options.Entries);
	}

	private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] values)
	{
		await Task.Yield();

		foreach (var value in values)
		{
			yield return value;
		}
	}

	public sealed class TestOptions
	{
		public List<string> Entries { get; } = [];
	}

	private sealed class PostConfigure(Func<TestOptions, ValueTask> configure) : IPostConfigureOptions<TestOptions>
	{
	#region Interface IPostConfigureOptions<TestOptions>

		public ValueTask Configure(TestOptions options) => configure(options);

	#endregion
	}
}