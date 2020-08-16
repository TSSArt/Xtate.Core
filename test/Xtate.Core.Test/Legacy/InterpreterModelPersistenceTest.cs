#region Copyright © 2019-2020 Sergii Artemenko

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

		public ValueTask<IObject[]> EvaluateArray(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject[]>(new IObject[0]);

	#endregion

	#region Interface IBooleanEvaluator

		public ValueTask<bool> EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) => new ValueTask<bool>(false);

	#endregion

	#region Interface IExecEvaluator

		public ValueTask Execute(IExecutionContext executionContext, CancellationToken token) => default;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; }

	#endregion

	#region Interface IIntegerEvaluator

		public ValueTask<int> EvaluateInteger(IExecutionContext executionContext, CancellationToken token) => new ValueTask<int>(0);

	#endregion

	#region Interface ILocationEvaluator

		public void DeclareLocalVariable(IExecutionContext executionContext) { }

		public ValueTask SetValue(IObject value, IExecutionContext executionContext, CancellationToken token) => default;

		public ValueTask<IObject> GetValue(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject>((IObject) null!);

		public string GetName(IExecutionContext executionContext) => "?";

	#endregion

	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject>((IObject) null!);

	#endregion

	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString(IExecutionContext executionContext, CancellationToken token) => new ValueTask<string>("");

	#endregion

	#region Interface IValueExpression

		public string? Expression { get; }

	#endregion
	}

	public class TestDataModelHandler : DataModelHandlerBase
	{
		public TestDataModelHandler() : base(DefaultErrorProcessor.Instance) { }

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
		public void Initialize()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xtate.Core.Test.Legacy.test.scxml");

			var xmlReader = XmlReader.Create(stream!);

			var director = new ScxmlDirector(xmlReader, BuilderFactory.Instance, DefaultErrorProcessor.Instance, namespaceResolver: null);

			_allStateMachine = director.ConstructStateMachine(StateMachineValidator.Instance);

			_dataModelHandler = new TestDataModelHandler();
		}

		[TestMethod]
		public async Task SaveInterpreterModelTest()
		{
			var model = await new InterpreterModelBuilder(_allStateMachine, _dataModelHandler, customActionProviders: default, default!, DefaultErrorProcessor.Instance).Build(default);
			var storeSupport = model.Root.As<IStoreSupport>();

			var storage = new InMemoryStorage(false);
			storeSupport.Store(new Bucket(storage));

			new StateMachineReader().Build(new Bucket(storage));
		}

		[TestMethod]
		public async Task SaveRestoreInterpreterModelWithStorageRecreateTest()
		{
			var model = new InterpreterModelBuilder(_allStateMachine, _dataModelHandler, customActionProviders: default, default!, DefaultErrorProcessor.Instance).Build(default).Result;
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

			await new InterpreterModelBuilder(restoredStateMachine, _dataModelHandler, customActionProviders: default, default!, DefaultErrorProcessor.Instance).Build(default);
		}

		[TestMethod]
		public async Task SaveRestoreInterpreterModelRuntimeModelTest()
		{
			var _ = new StateMachineFluentBuilder(BuilderFactory.Instance)
					.BeginState((Identifier) "a")
					.AddTransition(context => true, (Identifier) "a")
					.AddOnEntry(context => Console.WriteLine(@"OnEntry"))
					.EndState()
					.Build();

			var model = await new InterpreterModelBuilder(_allStateMachine, _dataModelHandler, customActionProviders: default, default!, DefaultErrorProcessor.Instance).Build(default);
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

			await new InterpreterModelBuilder(restoredStateMachine, _dataModelHandler, customActionProviders: default, default!, DefaultErrorProcessor.Instance).Build(default);
		}
	}
}