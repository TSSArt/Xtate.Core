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

using System.IO;
using Xtate.Ancestor.Extensions;
using Xtate.Class;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.DependencyInjection;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Persistence.DependencyInjection;
using Xtate.Persistence.Services;
using Xtate.ResourceLoaders;
using Xtate.ResourceLoaders.Resx.Services;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachineFluentBuilder.DependencyInjection;
using Xtate.Test;

namespace Xtate.Core.Test.Legacy;

public class Evaluator : IExternalScriptExpression, IIntegerEvaluator, IStringEvaluator, IExecEvaluator, IArrayEvaluator, IObjectEvaluator, IBooleanEvaluator, ILocationEvaluator, IValueExpression,
						 ILocationExpression, IConditionExpression, IScriptExpression, IExternalDataExpression
{
	public Evaluator(string? expression) => Expression = expression;

	public Evaluator(Uri? entityUri) => Uri = entityUri;

	private Uri? Uri { get; }

	private string? Expression { get; }

#region Interface IArrayEvaluator

	ValueTask<IObject[]> IArrayEvaluator.EvaluateArray() => new([]);

#endregion

#region Interface IBooleanEvaluator

	ValueTask<bool> IBooleanEvaluator.EvaluateBoolean() => new(false);

#endregion

#region Interface IConditionExpression

	string? IConditionExpression.Expression => Expression;

#endregion

#region Interface IExecEvaluator

	ValueTask IExecEvaluator.Execute() => default;

#endregion

#region Interface IExternalDataExpression

	Uri IExternalDataExpression.Uri => Uri!;

#endregion

#region Interface IExternalScriptExpression

	Uri? IExternalScriptExpression.Uri => Uri;

#endregion

#region Interface IIntegerEvaluator

	ValueTask<int> IIntegerEvaluator.EvaluateInteger() => new(0);

#endregion

#region Interface ILocationEvaluator

	ValueTask ILocationEvaluator.SetValue(IObject value) => default;

	ValueTask<IObject> ILocationEvaluator.GetValue() => new((IObject) null!);

	ValueTask<string> ILocationEvaluator.GetName() => new("?");

#endregion

#region Interface ILocationExpression

	string? ILocationExpression.Expression => Expression;

#endregion

#region Interface IObjectEvaluator

	ValueTask<IObject> IObjectEvaluator.EvaluateObject() => new(DataModelValue.Null);

#endregion

#region Interface IScriptExpression

	string? IScriptExpression.Expression => Expression;

#endregion

#region Interface IStringEvaluator

	ValueTask<string> IStringEvaluator.EvaluateString() => new("");

#endregion

#region Interface IValueExpression

	string? IValueExpression.Expression => Expression;

#endregion
}

public class TestDataModelHandler : DataModelHandlerBase
{
	protected override void Visit(ref IValueExpression expression)
	{
		expression = new Evaluator(expression.Expression);
	}

	protected override void Visit(ref ILocationExpression expression)
	{
		expression = new Evaluator(expression.Expression);
	}

	protected override void Visit(ref IConditionExpression entity)
	{
		entity = new Evaluator(entity.Expression);
	}

	protected override void Visit(ref IScriptExpression entity)
	{
		entity = new Evaluator(entity.Expression);
	}

	protected override void Visit(ref IExternalScriptExpression entity)
	{
		entity = new Evaluator(entity.Uri);
	}

	protected override void Visit(ref IExternalDataExpression entity)
	{
		entity = new Evaluator(entity.Uri);
	}
}

[TestClass]
public class InterpreterModelPersistenceTest
{
	[TestMethod]
	public async Task SaveInterpreterModelTest()
	{
		var services = new ServiceCollection();
		services.AddModule<StateMachineInterpreterModule>();
		services.AddModule<PersistenceModule>();

		//services.AddModule<StateMachineFactoryModule>();
		//services.AddConstant<IStateMachineLocation>(new LocationStateMachine(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml")));
		var smc = new LocationStateMachine("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml");
		smc.AddServices(services);
		services.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var optionsMock = new Mock<IXIncludeOptions>();
		optionsMock.Setup(x => x.XIncludeAllowed).Returns(true);
		services.AddConstant(optionsMock.Object);
		var serviceProvider = services.BuildProvider();
		var model = await serviceProvider.GetRequiredService<IInterpreterModel>();
		var storeSupport = model.Root.UseAncestor.As<IStoreSupport>();

		var storage = new InMemoryStorage(false);
		storeSupport.Store(new Bucket(storage));

		new StateMachineReader().Build(new Bucket(storage));
	}

	[TestMethod]
	public async Task SaveRestoreInterpreterModelWithStorageRecreateTest()
	{
		var services = new ServiceCollection();
		services.AddModule<StateMachineInterpreterModule>();
		services.AddModule<PersistenceModule>();

		//services.AddModule<StateMachineFactoryModule>();
		//services.AddConstant<IStateMachineLocation>(new LocationStateMachine(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml")));
		var smc = new LocationStateMachine("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml");
		smc.AddServices(services);
		services.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var optionsMock = new Mock<IXIncludeOptions>();
		optionsMock.Setup(x => x.XIncludeAllowed).Returns(true);
		services.AddConstant(optionsMock.Object);
		var serviceProvider = services.BuildProvider();
		var model = await serviceProvider.GetRequiredService<IInterpreterModel>();
		var storeSupport = model.Root.UseAncestor.As<IStoreSupport>();

		byte[] transactionLog;

		using (var storage = new InMemoryStorage(false))
		{
			storeSupport.Store(new Bucket(storage));
			transactionLog = new byte[storage.GetTransactionLogSize()];
			storage.WriteTransactionLogToSpan(new Span<byte>(transactionLog));

			Assert.AreEqual(expected: 0, storage.GetTransactionLogSize());
		}

		IStateMachine restoredStateMachine;

		using (var newStorage = new InMemoryStorage(transactionLog))
		{
			restoredStateMachine = new StateMachineReader().Build(new Bucket(newStorage));
		}

		var services2 = new ServiceCollection();
		services2.AddModule<StateMachineInterpreterModule>();

		//services2.AddModule<StateMachineFactoryModule>();
		services2.AddConstant(restoredStateMachine);
		services2.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services2.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var serviceProvider2 = services2.BuildProvider();
		var model2 = await serviceProvider2.GetRequiredService<IInterpreterModel>();
		Assert.IsNotNull(model2.Root);
	}

	[TestMethod]
	public async Task SaveRestoreInterpreterModelRuntimeModelTest()
	{
		var services0 = new ServiceCollection();
		services0.AddModule<StateMachineFluentBuilderModule>();
		var buildProvider = services0.BuildProvider();
		var fluentBuilder = buildProvider.GetRequiredServiceSync<StateMachineFluentBuilder.StateMachineFluentBuilder>();

		var stateMachine = fluentBuilder
						   .BeginState((Identifier) "a")
						   .AddTransition([ExcludeFromCodeCoverage]() => true, (Identifier) "a")
						   .AddOnEntry([ExcludeFromCodeCoverage]() => Console.WriteLine(@"OnEntry"))
						   .EndState()
						   .Build();

		//var writer = new StreamWriter("D:\\Ser\\Projects\\1.log");
		////var debugger = new ServiceProviderDebugger(writer);
		var services = new ServiceCollection();

		//services.AddConstant<IServiceProviderActions>(debugger);
		services.AddModule<StateMachineInterpreterModule>();
		services.AddModule<PersistenceModule>();
		services.AddConstant(stateMachine);
		var storageProvider = new StateMachinePersistenceTest.TestStorage();
		services.AddConstant<IStorageProvider>(storageProvider);
		var serviceProvider = services.BuildProvider();
		var model = await serviceProvider.GetRequiredService<IInterpreterModel>();
		var storeSupport = model.Root.UseAncestor.As<IStoreSupport>();

		byte[] transactionLog;

		using (var storage = new InMemoryStorage(false))
		{
			storeSupport.Store(new Bucket(storage));
			transactionLog = new byte[storage.GetTransactionLogSize()];
			storage.WriteTransactionLogToSpan(new Span<byte>(transactionLog));
		}

		IStateMachine restoredStateMachine;

		using (var newStorage = new InMemoryStorage(transactionLog))
		{
			restoredStateMachine = new StateMachineReader().Build(new Bucket(newStorage), model.EntityMap);
		}

		var services2 = new ServiceCollection();
		services2.AddModule<StateMachineInterpreterModule>();

		//services2.AddModule<StateMachineFactoryModule>();
		services2.AddConstant(restoredStateMachine);
		services2.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services2.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var serviceProvider2 = services2.BuildProvider();
		var model2 = await serviceProvider2.GetRequiredService<IInterpreterModel>();
		Assert.IsNotNull(model2.Root);
	}

	[UsedImplicitly]
	public class DummyResourceLoader : ResxResourceLoader
	{
		protected override Stream GetResourceStream(Uri uri)
		{
			try
			{
				return base.GetResourceStream(uri);
			}
			catch
			{
				return new MemoryStream();
			}
		}
	}
}