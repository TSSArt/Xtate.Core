﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.Builder;
using Xtate.DataModel;
using Xtate.Persistence;
using Xtate.Scxml;

namespace Xtate.Core.Test.Legacy
{
	public class Evaluator : IExternalScriptExpression, IIntegerEvaluator, IStringEvaluator, IExecEvaluator, IArrayEvaluator, IObjectEvaluator, IBooleanEvaluator, ILocationEvaluator, IValueExpression,
							 ILocationExpression, IConditionExpression, IScriptExpression
	{
		public Evaluator(string? expression) => Expression = expression;

		public Evaluator(Uri? entityUri) => Uri = entityUri;

	#region Interface IArrayEvaluator

		public ValueTask<IObject[]> EvaluateArray(IExecutionContext executionContext, CancellationToken token) => new(Array.Empty<IObject>());

	#endregion

	#region Interface IBooleanEvaluator

		public ValueTask<bool> EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) => new(false);

	#endregion

	#region Interface IExecEvaluator

		public ValueTask Execute(IExecutionContext executionContext, CancellationToken token) => default;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; }

	#endregion

	#region Interface IIntegerEvaluator

		public ValueTask<int> EvaluateInteger(IExecutionContext executionContext, CancellationToken token) => new(0);

	#endregion

	#region Interface ILocationEvaluator

		public void DeclareLocalVariable(IExecutionContext executionContext) { }

		public ValueTask SetValue(IObject value, IExecutionContext executionContext, CancellationToken token) => default;

		public ValueTask<IObject> GetValue(IExecutionContext executionContext, CancellationToken token) => new((IObject) null!);

		public string GetName(IExecutionContext executionContext) => "?";

	#endregion

	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token) => new((IObject) null!);

	#endregion

	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString(IExecutionContext executionContext, CancellationToken token) => new("");

	#endregion

	#region Interface IValueExpression

		public string? Expression { get; }

	#endregion
	}

	public class TestDataModelHandler : DataModelHandlerBase
	{
		public TestDataModelHandler() : base(DefaultErrorProcessor.Instance) { }

		public override ITypeInfo TypeInfo => TypeInfo<TestDataModelHandler>.Instance;

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
	}

	[TestClass]
	public class InterpreterModelPersistenceTest
	{
		private IStateMachine     _allStateMachine  = default!;
		private IDataModelHandler _dataModelHandler = default!;

		[TestInitialize]
		public async Task Initialize()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xtate.Core.Test.Legacy.test.scxml");

			var xmlReader = XmlReader.Create(stream!);

			var director = new ScxmlDirector(xmlReader, BuilderFactory.Instance, new ScxmlDirectorOptions { StateMachineValidator = StateMachineValidator.Instance });

			_allStateMachine = await director.ConstructStateMachine();

			_dataModelHandler = new TestDataModelHandler();
		}

		private InterpreterModelBuilder.Parameters CreateBuilderParameters() =>
			new(_allStateMachine, _dataModelHandler)
			{
				ResourceLoaderFactories = ImmutableArray.Create(DummyResourceLoader.Instance),
				SecurityContext = SecurityContext.Create(SecurityContextType.NewStateMachine, new DeferredFinalizer())
			};

		[TestMethod]
		public async Task SaveInterpreterModelTest()
		{
			var model = await new InterpreterModelBuilder(CreateBuilderParameters()).Build(default);
			var storeSupport = model.Root.As<IStoreSupport>();

			var storage = new InMemoryStorage(false);
			storeSupport.Store(new Bucket(storage));

			new StateMachineReader().Build(new Bucket(storage));
		}

		[TestMethod]
		public async Task SaveRestoreInterpreterModelWithStorageRecreateTest()
		{
			var model = new InterpreterModelBuilder(CreateBuilderParameters()).Build(default)
																			  .SynchronousGetResult();
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

			await new InterpreterModelBuilder(CreateBuilderParameters() with { StateMachine = restoredStateMachine }).Build(default);
		}

		[TestMethod]
		public async Task SaveRestoreInterpreterModelRuntimeModelTest()
		{
			var _ = FluentBuilderFactory
					.Create()
					.BeginState((Identifier) "a")
					.AddTransition(_ => true, (Identifier) "a")
					.AddOnEntry(_ => Console.WriteLine(@"OnEntry"))
					.EndState()
					.Build();

			var model = await new InterpreterModelBuilder(CreateBuilderParameters()).Build(default);
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

			await new InterpreterModelBuilder(CreateBuilderParameters() with { StateMachine = restoredStateMachine }).Build(default);
		}

		private class DummyResourceLoader : IResourceLoaderFactory, IResourceLoaderFactoryActivator, IResourceLoader
		{
			public static readonly IResourceLoaderFactory Instance = new DummyResourceLoader();

		#region Interface IResourceLoader

			public ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token) => new(new Resource(new MemoryStream()));

		#endregion

		#region Interface IResourceLoaderFactory

			public ValueTask<IResourceLoaderFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri uri, CancellationToken token) => new(this);

		#endregion

		#region Interface IResourceLoaderFactoryActivator

			public ValueTask<IResourceLoader> CreateResourceLoader(IFactoryContext factoryContext, CancellationToken token) => new(this);

		#endregion
		}
	}
}