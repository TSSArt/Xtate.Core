// Copyright © 2019-2024 Sergii Artemenko
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

using System.Reflection;
using System.Xml;
using Xtate.Core;
using Xtate.CustomAction;
using Xtate.IoC;

namespace Xtate.Test.HostedTests;

public abstract class HostedTestBase
{
	protected StateMachineHost       Host      { get; private set; } = default!;
	protected Mock<ILogWriter> LogWriter { get; private set; } = default!;

	[TestInitialize]
	public async Task Initialize()
	{
		LogWriter = new Mock<ILogWriter>();
		LogWriter.Setup(s => s.IsEnabled(It.IsAny<Type>(), It.IsAny<Level>())).Returns(true);
		/*
		Host = new StateMachineHostBuilder()
			   //TODO:
			   //.AddCustomActionFactory(SystemActionFactory.Instance)
			   //.AddResourceLoaderFactory(ResxResourceLoaderFactory.Instance)
			   .SetLogger(Logger.Object)
			   .Build(ServiceLocator.Default);
		return Host.StartHostAsync();
		*/
		var sc = new ServiceCollection();
		sc.AddModule<StateMachineHostModule>();
		sc.AddSharedImplementationSync<StartActionProvider>(SharedWithin.Scope).For<ICustomActionProvider>();
		sc.AddSharedImplementationSync<DestroyActionProvider>(SharedWithin.Scope).For<ICustomActionProvider>();
		sc.AddTypeSync<StartAction, XmlReader>();
		sc.AddTypeSync<DestroyAction, XmlReader>();
		sc.AddForwarding(_ => LogWriter.Object);
		var sp = sc.BuildProvider();
		Host = await sp.GetRequiredService<StateMachineHost>();
	}

	[TestCleanup]
	public Task Cleanup() => Host.StopHostAsync();

	protected async Task Execute([PathReference("~/HostedTests/Scxml/")] string scxmlPath)
	{
		var name = Assembly.GetExecutingAssembly().GetName().Name;
		await Host.ExecuteStateMachineAsync(new Uri($"resx://{name}/{name}/HostedTests/Scxml/" + scxmlPath));
	}
}