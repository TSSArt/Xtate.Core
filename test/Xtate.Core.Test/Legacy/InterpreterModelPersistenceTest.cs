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
using Xtate.IoC;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Test;
using IServiceProvider = System.IServiceProvider;

namespace Xtate.Core.Test.Legacy;

public class Evaluator : IExternalScriptExpression, IIntegerEvaluator, IStringEvaluator, IExecEvaluator, IArrayEvaluator, IObjectEvaluator, IBooleanEvaluator, ILocationEvaluator, IValueExpression,
						 ILocationExpression, IConditionExpression, IScriptExpression, IExternalDataExpression
{
<<<<<<< Updated upstream
	public class Evaluator : IExternalScriptExpression, IIntegerEvaluator, IStringEvaluator, IExecEvaluator, IArrayEvaluator, IObjectEvaluator, IBooleanEvaluator, ILocationEvaluator, IValueExpression,
							 ILocationExpression, IConditionExpression, IScriptExpression, IExternalDataExpression, IResourceEvaluator
	{
		public Evaluator(string? expression) => Expression = expression;

		public Evaluator(Uri? entityUri) => Uri = entityUri;

	#region Interface IArrayEvaluator

		ValueTask<IObject[]> IArrayEvaluator.EvaluateArray() => new(Array.Empty<IObject>());

	#endregion

	#region Interface IBooleanEvaluator

		ValueTask<bool> IBooleanEvaluator.EvaluateBoolean() => new(false);

	#endregion

	#region Interface IExecEvaluator

		ValueTask IExecEvaluator.Execute() => default;

	#endregion

		ValueTask<IObject> IResourceEvaluator.EvaluateObject(Resource resource) => default;

	#region Interface IExternalScriptExpression

		Uri? IExternalScriptExpression.Uri => Uri;

	#endregion

	#region Interface IIntegerEvaluator

		ValueTask<int> IIntegerEvaluator.EvaluateInteger() => new(0);

	#endregion

	#region Interface ILocationEvaluator

		ValueTask ILocationEvaluator.DeclareLocalVariable() => default;

		ValueTask ILocationEvaluator.SetValue(IObject value) => default;

		ValueTask<IObject> ILocationEvaluator.GetValue() => new((IObject) null!);

		ValueTask<string> ILocationEvaluator.GetName() => new("?");

	#endregion

	#region Interface IObjectEvaluator

		ValueTask<IObject> IObjectEvaluator.EvaluateObject() => new((IObject) null!);

	#endregion

	#region Interface IStringEvaluator

		ValueTask<string> IStringEvaluator.EvaluateString() => new("");

	#endregion

	#region Interface IValueExpression

		string? IScriptExpression.Expression => Expression;

		string? IConditionExpression.Expression => Expression;

		string? ILocationExpression. Expression => Expression;

		string? IValueExpression.   Expression => Expression;

	#endregion

		Uri? IExternalDataExpression.Uri => Uri;

		private Uri? Uri { get; }
		private string? Expression { get; }

=======
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
>>>>>>> Stashed changes
	}

	protected override void Visit(ref ILocationExpression expression)
	{
<<<<<<< Updated upstream
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
=======
		expression = new Evaluator(expression.Expression);
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IConditionExpression entity)
	{
<<<<<<< Updated upstream
		private IStateMachine     _allStateMachine  = default!;
		private IDataModelHandler _dataModelHandler = default!;
		//private ServiceLocator _serviceLocator;
=======
		entity = new Evaluator(entity.Expression);
	}
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xtate.Core.Test.Legacy.test.scxml");

			var xmlReader = XmlReader.Create(stream!);
			/*
			_serviceLocator = ServiceLocator.Create(
				delegate(IServiceCollection s)
				{
					s.AddForwarding(_ => (IResourceLoader) DummyResourceLoader.Instance);
					s.AddForwarding<IStateMachineValidator, StateMachineValidator>();
				});

			var director = _serviceLocator.GetService<ScxmlDirector, XmlReader>(xmlReader);*/

			//_allStateMachine = await director.ConstructStateMachine();

			_dataModelHandler = new TestDataModelHandler
								{
									DefaultLogEvaluatorFactory = null,
									DefaultSendEvaluatorFactory = null,
									//DefaultDoneDataEvaluatorFactory = null,
									DefaultCancelEvaluatorFactory = null,
									DefaultIfEvaluatorFactory = null,
									DefaultRaiseEvaluatorFactory = null,
									DefaultForEachEvaluatorFactory = null,
									DefaultAssignEvaluatorFactory = null,
									DefaultScriptEvaluatorFactory = null,
									DefaultCustomActionEvaluatorFactory = null,
									DefaultInvokeEvaluatorFactory = null,
									DefaultContentBodyEvaluatorFactory = null,
									DefaultInlineContentEvaluatorFactory = null,
									DefaultExternalDataExpressionEvaluatorFactory = null,
									//DefaultParamEvaluatorFactory = null,
									CustomActionContainerFactory = null,
									d = null
								};
		}

		/*private InterpreterModelBuilder.Parameters CreateBuilderParameters() =>
			new(_serviceLocator, _allStateMachine, _dataModelHandler)
			{
				ResourceLoaderFactories = ImmutableArray.Create(DummyResourceLoader.Instance),
				SecurityContext = SecurityContext.Create(SecurityContextType.NewStateMachine)
			};*/

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
=======
>>>>>>> Stashed changes
			storeSupport.Store(new Bucket(storage));
			transactionLog = new byte[storage.GetTransactionLogSize()];
			storage.WriteTransactionLogToSpan(new Span<byte>(transactionLog));

			Assert.AreEqual(expected: 0, storage.GetTransactionLogSize());
		}

		IStateMachine restoredStateMachine;
		using (var newStorage = new InMemoryStorage(transactionLog))
		{
<<<<<<< Updated upstream
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
=======
			restoredStateMachine = new StateMachineReader().Build(new Bucket(newStorage));
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
			var stateMachine = FluentBuilderFactory
					.Create()
					.BeginState((Identifier) "a")
					.AddTransition(() => true, (Identifier) "a")
					.AddOnEntry(() => Console.WriteLine(@"OnEntry"))
					.EndState()
					.Build();

			var services = new ServiceCollection();
			services.RegisterStateMachineInterpreter();
			services.AddForwarding(_ => stateMachine);
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

		//TODO: remove IResourceLoader
		private class DummyResourceLoader : ResxResourceLoader//, IResourceLoaderFactory, IResourceLoaderFactoryActivator
		{
			protected override Stream GetResourceStream(Uri uri)
			{
				try
				{
					return base.GetResourceStream(uri);
				}
				catch (Exception e)
				{
					return new MemoryStream();
				}
			}

			/*
		#region Interface IResourceLoader

			public ValueTask<Resource> Request(Uri uri, NameValueCollection? headers) => new(new Resource(GetStream(uri)));

		#endregion

		#region Interface IResourceLoaderFactory

			public ValueTask<IResourceLoaderFactoryActivator?> TryGetActivator(Uri uri) => new(this);

		#endregion

		#region Interface IResourceLoaderFactoryActivator

			public ValueTask<IResourceLoader> CreateResourceLoader(ServiceLocator serviceLocator) => new(this);

		#endregion

			public  ValueTask<Resource> GetResource(Uri uri) => new(new Resource(GetStream(uri)));

			private Stream GetStream(Uri uri)
			{
				return new MemoryStream();
			}

			public ValueTask<Resource> GetResource(Uri uri, NameValueCollection? headers) => new(new Resource(GetStream(uri)));

			public ValueTask<IResourceLoader?> TryGetResourceLoader(Uri uri) => throw new NotImplementedException();*/
=======
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
>>>>>>> Stashed changes
		}
	}
}