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
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.CustomAction;
using Xtate.DataModel;

namespace Xtate
{
	internal sealed class CustomActionDispatcher : ICustomAction, ICustomActionDispatcher, ICustomActionContext
	{
		private readonly ICustomAction   _customAction;
		private readonly IErrorProcessor _errorProcessor;
		private readonly IFactoryContext _factoryContext;

		private ICustomActionExecutor?                       _executor;
		private ImmutableArray<ILocationEvaluator>           _locationEvaluators;
		private ImmutableArray<ILocationExpression>.Builder? _locations;
		private ImmutableArray<IValueEvaluator>              _valueEvaluators;
		private ImmutableArray<IValueExpression>.Builder?    _values;

		public CustomActionDispatcher(IErrorProcessor errorProcessor, ICustomAction customAction, IFactoryContext factoryContext)
		{
			_errorProcessor = errorProcessor;
			_customAction = customAction;
			_factoryContext = factoryContext;

			Infrastructure.NotNull(customAction.XmlNamespace);
			Infrastructure.NotNull(customAction.XmlName);
			Infrastructure.NotNull(customAction.Xml);
		}

	#region Interface ICustomAction

		public ImmutableArray<ILocationExpression> Locations { get; private set; }

		public ImmutableArray<IValueExpression> Values { get; private set; }

		public string XmlNamespace => _customAction.XmlNamespace!;

		public string XmlName => _customAction.XmlName!;

		public string Xml => _customAction.Xml!;

	#endregion

	#region Interface ICustomActionContext

		ILocationAssigner ICustomActionContext.RegisterLocationExpression(string expression)
		{
			if (expression is null) throw new ArgumentNullException(nameof(expression));

			if (_executor is not null)
			{
				throw Infrastructure.Fail<Exception>(Resources.Exception_Registration_should_no_occur_after_initialization);
			}

			_locations ??= ImmutableArray.CreateBuilder<ILocationExpression>();

			var locationAssigner = new LocationAssigner(this, _locations.Count, expression);
			_locations.Add(locationAssigner);

			return locationAssigner;
		}

		IExpressionEvaluator ICustomActionContext.RegisterValueExpression(string expression, ExpectedValueType expectedValueType)
		{
			if (expression is null) throw new ArgumentNullException(nameof(expression));

			if (_executor is not null)
			{
				throw Infrastructure.Fail<Exception>(Resources.Exception_Registration_should_no_occur_after_initialization);
			}

			_values ??= ImmutableArray.CreateBuilder<IValueExpression>();

			var expressionEvaluator = new ExpressionEvaluator(this, _values.Count, expression, expectedValueType);
			_values.Add(expressionEvaluator);

			return expressionEvaluator;
		}

		void ICustomActionContext.AddValidationError<T>(string message, Exception? exception)
		{
			_errorProcessor.AddError<T>(this, message, exception);
		}

		string ICustomActionContext.Xml => _customAction.Xml!;

	#endregion

	#region Interface ICustomActionDispatcher

		public void SetEvaluators(ImmutableArray<ILocationEvaluator> locationEvaluators, ImmutableArray<IValueEvaluator> objectEvaluators)
		{
			_locationEvaluators = locationEvaluators;
			_valueEvaluators = objectEvaluators;
		}

		public ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			Infrastructure.NotNull(_executor);

			return _executor.Execute(executionContext, token);
		}

	#endregion

		public async ValueTask SetupExecutor(ImmutableArray<ICustomActionFactory> customActionFactories, CancellationToken token)
		{
			try
			{
				var executor = await GetExecutor(customActionFactories, token).ConfigureAwait(false);

				if (executor is null)
				{
					_errorProcessor.AddError<CustomActionDispatcher>(this, Resources.ErrorMessage_Custom_action_executor_can_t_be_found);
				}

				Locations = _locations?.ToImmutable() ?? default;
				Values = _values?.ToImmutable() ?? default;

				_executor = executor;
			}
			catch (Exception ex)
			{
				_errorProcessor.AddError<PreDataModelProcessor>(this, Resources.ErrorMessage_Error_on_creation_CustomAction_executor, ex);
			}

			_executor ??= NoActionExecutor.Instance;
		}

		private async ValueTask<ICustomActionExecutor?> GetExecutor(ImmutableArray<ICustomActionFactory> customActionFactories, CancellationToken token)
		{
			if (customActionFactories.IsDefaultOrEmpty)
			{
				return null;
			}

			foreach (var factory in customActionFactories)
			{
				var activator = await factory.TryGetActivator(_factoryContext, XmlNamespace, XmlName, token).ConfigureAwait(false);

				if (activator is not null)
				{
					return await activator.CreateExecutor(_factoryContext, this, token).ConfigureAwait(false);
				}
			}

			return null;
		}

		private class LocationAssigner : ILocationAssigner, ILocationExpression
		{
			private readonly CustomActionDispatcher _dispatcher;
			private readonly int                    _index;

			public LocationAssigner(CustomActionDispatcher dispatcher, int index, string expression)
			{
				_dispatcher = dispatcher;
				_index = index;
				Expression = expression;
			}

		#region Interface ILocationAssigner

			public ValueTask Assign(IExecutionContext executionContext, DataModelValue value, CancellationToken token)
			{
				if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

				Infrastructure.Assert(!_dispatcher._locationEvaluators.IsDefault);

				var locationEvaluator = _dispatcher._locationEvaluators[_index];

				return locationEvaluator.SetValue(value, customData: null, executionContext, token);
			}

		#endregion

		#region Interface ILocationExpression

			public string Expression { get; }

		#endregion
		}

		private class ExpressionEvaluator : IExpressionEvaluator, IValueExpression
		{
			private readonly CustomActionDispatcher _dispatcher;
			private readonly ExpectedValueType      _expectedValueType;
			private readonly int                    _index;

			public ExpressionEvaluator(CustomActionDispatcher dispatcher, int index, string expression, ExpectedValueType expectedValueType)
			{
				_dispatcher = dispatcher;
				_index = index;
				_expectedValueType = expectedValueType;
				Expression = expression;
			}

		#region Interface IExpressionEvaluator

			public ValueTask<DataModelValue> Evaluate(IExecutionContext executionContext, CancellationToken token)
			{
				if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

				Infrastructure.Assert(!_dispatcher._valueEvaluators.IsDefault);

				var valueEvaluator = _dispatcher._valueEvaluators[_index];

				return Evaluate(valueEvaluator, executionContext, token);
			}

		#endregion

		#region Interface IValueExpression

			public string Expression { get; }

		#endregion

			private async ValueTask<DataModelValue> Evaluate(IValueEvaluator objectEvaluator, IExecutionContext executionContext, CancellationToken token)
			{
				switch (_expectedValueType)
				{
					case ExpectedValueType.String when objectEvaluator is IStringEvaluator evaluator:
						return await evaluator.EvaluateString(executionContext, token).ConfigureAwait(false);

					case ExpectedValueType.Integer when objectEvaluator is IIntegerEvaluator evaluator:
						return await evaluator.EvaluateInteger(executionContext, token).ConfigureAwait(false);

					case ExpectedValueType.Boolean when objectEvaluator is IBooleanEvaluator evaluator:
						return await evaluator.EvaluateBoolean(executionContext, token).ConfigureAwait(false);
				}

				var obj = await ((IObjectEvaluator) objectEvaluator).EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj);
			}
		}

		private class NoActionExecutor : ICustomActionExecutor
		{
			public static ICustomActionExecutor Instance { get; } = new NoActionExecutor();

		#region Interface ICustomActionExecutor

			public ValueTask Execute(IExecutionContext executionContext, CancellationToken token) => Infrastructure.Fail<ValueTask>();

		#endregion
		}
	}
}