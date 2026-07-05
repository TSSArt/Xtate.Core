using Xtate.IoC;
using Xtate.IoC.DependencyInjection;
using Xtate.IoC.TransformArgs.DependencyInjection;

namespace Xtate.Core.Test;

[TestClass]
public class AncestorTest
{
	[TestMethod]
	public async Task Method2Test()
	{
		await using var container = Container.Create<IoCModule>(s =>
																  {
																	  s.AddType<Anc>();
																	  s.AddType<Child>();
																  });

		var anc = await container.GetRequiredService<Anc>();

		Assert.IsNotNull(anc);
		Assert.IsNotNull(anc.Child);
		Assert.AreSame(anc, anc.Child.Ancestor());
	}

	[TestMethod]
	public async Task Method3Test()
	{
		await using var container = Container.Create<IoCModule>(s =>
																{
																	s.AddType<Anc2>();
																	s.AddType<Child2>();
																});

		var anc = await container.GetRequiredService<Anc2>();

		Assert.IsNotNull(anc);
		Assert.IsNotNull(anc.Child1);
		Assert.IsNotNull(anc.Child2);
		Assert.AreNotSame(anc.Child1, anc.Child2);
		Assert.AreSame(anc, anc.Child1.Ancestor());
		Assert.AreSame(anc, anc.Child2.Ancestor());
	}

	[TestMethod]
	public async Task Method4Test()
	{
		await using var container = Container.Create<IoCModule>(s =>
																{
																	s.AddType<Anc3>();
																	s.AddType<Child3>();
																});

		var anc = await container.GetRequiredService<Anc3>();

		Assert.IsNotNull(anc);
		Assert.IsNotNull(anc.Child1);
		Assert.IsNotNull(anc.Child2);
		Assert.IsNotNull(anc.Child3);
		Assert.AreNotSame(anc.Child1, anc.Child2);
		Assert.AreNotSame(anc.Child1, anc.Child3);
		Assert.AreNotSame(anc.Child2, anc.Child3);
		Assert.AreSame(anc, anc.Child1.Ancestor());
		Assert.AreSame(anc, anc.Child2.Ancestor());
		Assert.AreSame(anc, anc.Child3.Ancestor());
	}

	[TestMethod]
	public async Task MethodATest()
	{
		await using var container = Container.Create<IoCModule>(s =>
																{
																	s.AddType<ConfigFileReader>();
																	s.AddType<SettingsFileReader>();

																	s.AddImplementation<FileInstance, string>().For<IFile>();

																	s.ForService<IFile, string>().UseArgValue("Config").IfAncestor<ConfigFileReader>();
																	s.ForService<IFile, string>().UseArgValue("Settings").IfAncestor<SettingsFileReader>();
																});

		var configFileReader = await container.GetRequiredService<ConfigFileReader>();
		var settingsFileReader = await container.GetRequiredService<SettingsFileReader>();

		Assert.IsNotNull(configFileReader);
		Assert.AreEqual(expected: "Config", configFileReader.File.Name);
		Assert.IsNotNull(settingsFileReader);
		Assert.AreEqual(expected: "Settings", settingsFileReader.File.Name);
	}

	public interface IFile
	{
		string Name { get; }
	}

	[InstantiatedByIoC]
	public class ConfigFileReader
	{
		public required IFile File { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class SettingsFileReader
	{
		public required IFile File { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class FileInstance(string name) : IFile
	{
	#region Interface IFile

		public string Name { get; } = name;

	#endregion
	}

	[InstantiatedByIoC]
	public class Anc
	{
		public required Child Child { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class Anc2
	{
		public required Child2 Child1 { get; [SetByIoC] init; }

		public required Child2 Child2 { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class Anc3
	{
		public required Child3 Child1 { get; [SetByIoC] init; }

		// ReSharper disable once MemberHidesStaticFromOuterClass
		public required Child3 Child2 { get; [SetByIoC] init; }

		public required Child3 Child3 { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class Child
	{
		public required IoC.AncestorTracker.Ancestor<Anc> Ancestor { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class Child2
	{
		public required IoC.AncestorTracker.Ancestor<Anc2> Ancestor { get; [SetByIoC] init; }
	}

	[InstantiatedByIoC]
	public class Child3
	{
		public required IoC.AncestorTracker.Ancestor<Anc3> Ancestor { get; [SetByIoC] init; }
	}
}