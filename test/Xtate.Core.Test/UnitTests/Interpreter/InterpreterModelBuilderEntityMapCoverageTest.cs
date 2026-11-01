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

using System.Reflection;
using Xtate.DataModel;
using Xtate.Interpreter.DependencyInjection;
using Xtate.IoC;
using Xtate.ResourceLoaders;
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterModelBuilderEntityMapCoverageTest
{
	[TestMethod]
	public async Task EmbeddedExternalScriptContentIsUsedWithoutFetchingTheResourceAgain()
	{
		var source = new EmbeddedScriptSource();
		var script = new Mock<IScript>();
		script.SetupGet(value => value.Source).Returns(source);
		script.As<IExecEvaluator>();
		var stateMachine = new StateMachineEntity { Script = script.Object, States = [new FinalEntity()] };
		var resources = new Mock<IResourceLoader>(MockBehavior.Strict);
		var services = new ServiceCollection();
		services.AddModule<InterpreterModelBuilderModule>();
		services.AddConstant<IStateMachine>(stateMachine);
		services.AddConstant(Mock.Of<IDataModelHandler>());
		services.AddConstant(resources.Object);
		var provider = services.BuildProvider();
		var builder = await provider.GetRequiredService<InterpreterModelBuilder>();
		var model = await builder.BuildModel(true);
		Assert.IsNotNull(model.Root.Script);
		Assert.AreEqual("embedded script", source.ReceivedContent);
		resources.VerifyNoOtherCalls();
	}

	private sealed class EmbeddedScriptSource : IExternalScriptExpression, IExternalScriptProvider, IExternalScriptConsumer
	{
		public Uri Uri => new("https://example.test/embedded.js");
		public string Content => "embedded script";
		public string? ReceivedContent { get; private set; }
		public void SetContent(string content) => ReceivedContent = content;
	}
	[TestMethod]
	public void EntityMapReturnsStoredEntityAndRejectsNullAndOutOfRangeSlots()
	{
		var stored = new EntitySource();
		var type = typeof(InterpreterModelBuilder).GetNestedType(name: "EntityMap", BindingFlags.NonPublic)!;
		var map = (IEntityMap)Activator.CreateInstance(type, [new IEntity?[] { null, stored }])!;

		Assert.IsFalse(map.TryGetEntityByDocumentId(id: 0, out var missing));
		Assert.IsNull(missing);
		Assert.IsTrue(map.TryGetEntityByDocumentId(id: 1, out var actual));
		Assert.AreSame(stored, actual);
		Assert.IsFalse(map.TryGetEntityByDocumentId(id: 2, out var outside));
		Assert.IsNull(outside);
	}

	private sealed class EntitySource : IEntity;
}
