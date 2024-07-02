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

using System.IO;
using Xtate.Builder;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Test;
using IServiceProvider = System.IServiceProvider;

namespace Xtate.Core.Test.Legacy;

public class Evaluator : IExternalScriptExpression, IIntegerEvaluator, IStringEvaluator, IExecEvaluator, IArrayEvaluator, IObjectEvaluator, IBooleanEvaluator, ILocationEvaluator, IValueExpression,
						 ILocationExpression, IConditionExpression, IScriptExpression, IExternalDataExpression
{
	public Evaluator(string? expression) => Expression = expression;

	public Evaluator(Uri? entityUri) => Uri = entityUri;

	private Uri?    Uri        { get; }
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

	ValueTask<IObject> IObjectEvaluator.EvaluateObject() => new(DefaultObject.Null);

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
		services.RegisterStateMachineInterpreter();
		services.RegisterStateMachineFactory();
		services.AddForwarding<IStateMachineLocation>(_ => new StateMachineLocation(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml")));
		services.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var serviceProvider = services.BuildProvider();
		var model = await serviceProvider.GetRequiredService<IInterpreterModel>();
		var storeSupport = model.Root.As<IStoreSupport>();

		var storage = new InMemoryStorage(false);
		storeSupport.Store(new Bucket(storage));

		new StateMachineReader().Build(new Bucket(storage));
	}

	[TestMethod]
	public async Task SaveRestoreInterpreterModelWithStorageRecreateTest()
	{
		var services = new ServiceCollection();
		services.RegisterStateMachineInterpreter();
		services.RegisterStateMachineFactory();
		services.AddForwarding<IStateMachineLocation>(_ => new StateMachineLocation(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml")));
		services.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var serviceProvider = services.BuildProvider();
		var model = await serviceProvider.GetRequiredService<IInterpreterModel>();
		var storeSupport = model.Root.As<IStoreSupport>();

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
		services2.RegisterStateMachineInterpreter();
		services2.RegisterStateMachineFactory();
		services2.AddForwarding(_ => restoredStateMachine);
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
		services0.RegisterStateMachineFluentBuilder();
		var buildProvider = services0.BuildProvider();
		var fluentBuilder = buildProvider.GetRequiredServiceSync<StateMachineFluentBuilder>();

		var stateMachine = fluentBuilder
						   .BeginState((Identifier) "a")
						   .AddTransition(() => true, (Identifier) "a")
						   .AddOnEntry(() => Console.WriteLine(@"OnEntry"))
						   .EndState()
						   .Build();

		//var writer = new StreamWriter("C:\\Projects\\1.log");
		//var debugger = new ServiceProviderDebugger(writer);
		var services = new ServiceCollection();
		//services.AddForwarding<IServiceProviderDebugger>(s => debugger);
		services.RegisterStateMachineInterpreter();
		services.RegisterPersistence();
		services.AddForwarding(_ => stateMachine);
		var storageProvider = new StateMachinePersistenceTest.TestStorage();
		services.AddForwarding<IStorageProvider>(_ => storageProvider);
		var serviceProvider = services.BuildProvider();
		var model = await serviceProvider.GetRequiredService<IInterpreterModel>();
		var storeSupport = model.Root.As<IStoreSupport>();

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
		services2.RegisterStateMachineInterpreter();
		services2.RegisterStateMachineFactory();
		services2.AddForwarding(_ => restoredStateMachine);
		services2.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
		services2.AddImplementation<DummyResourceLoader>().For<IResourceLoader>();
		var serviceProvider2 = services2.BuildProvider();
		var model2 = await serviceProvider2.GetRequiredService<IInterpreterModel>();
		Assert.IsNotNull(model2.Root);
	}

	[UsedImplicitly]
	private class DummyResourceLoader : ResxResourceLoader
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