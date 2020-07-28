#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.CustomAction;
using Xtate.DataModel;

namespace Xtate
{
	internal sealed class CustomActionDispatcher : ICustomAction, ICustomActionDispatcher, ICustomActionContext
	{
		private readonly CustomActionEntity                         _customAction;
		private readonly ImmutableArray<ICustomActionFactory> _customActionFactories;
		private readonly IErrorProcessor                      _errorProcessor;

		private ICustomActionExecutor?                       _executor;
		private ImmutableArray<ILocationEvaluator>           _locationEvaluators;
		private ImmutableArray<ILocationExpression>.Builder? _locations;
		private ImmutableArray<IObjectEvaluator>             _objectEvaluators;
		private ImmutableArray<IValueExpression>.Builder?    _values;

		public CustomActionDispatcher(ImmutableArray<ICustomActionFactory> customActionFactories, IErrorProcessor errorProcessor, in CustomActionEntity customAction)
		{
			_customActionFactories = customActionFactories;
			_errorProcessor = errorProcessor;
			_customAction = customAction;

			Infrastructure.Assert(customAction.Xml != null);
		}

	#region Interface ICustomAction

		public ImmutableArray<ILocationExpression> Locations { get; private set; }

		public ImmutableArray<IValueExpression> Values { get; private set; }

		public string Xml => _customAction.Xml!;

	#endregion

	#region Interface ICustomActionContext

		ILocationAssigner ICustomActionContext.RegisterLocationExpression(string expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			if (_executor != null)
			{
				throw new InfrastructureException(Resources.Exception_Registration_should_no_occur_after_initialization);
			}

			_locations ??= ImmutableArray.CreateBuilder<ILocationExpression>();

			var locationAssigner = new LocationAssigner(this, _locations.Count, expression);
			_locations.Add(locationAssigner);

			return locationAssigner;
		}

		IExpressionEvaluator ICustomActionContext.RegisterValueExpression(string expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			if (_executor != null)
			{
				throw new InfrastructureException(Resources.Exception_Registration_should_no_occur_after_initialization);
			}

			_values ??= ImmutableArray.CreateBuilder<IValueExpression>();

			var expressionEvaluator = new ExpressionEvaluator(this, _values.Count, expression);
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

		public void SetEvaluators(ImmutableArray<ILocationEvaluator> locationEvaluators, ImmutableArray<IObjectEvaluator> objectEvaluators)
		{
			_locationEvaluators = locationEvaluators;
			_objectEvaluators = objectEvaluators;
		}

		public ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			Infrastructure.Assert(_executor != null);

			return _executor.Execute(executionContext, token);
		}

	#endregion

		public void SetupExecutor()
		{
			try
			{
				var executor = GetExecutor();

				if (executor == null)
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

		private ICustomActionExecutor? GetExecutor()
		{
			if (_customActionFactories.IsDefaultOrEmpty)
			{
				return null;
			}

			using var stringReader = new StringReader(Xml);
			using var xmlReader = XmlReader.Create(stringReader);

			xmlReader.MoveToContent();

			var namespaceUri = xmlReader.NamespaceURI;
			var name = xmlReader.LocalName;

			foreach (var factory in _customActionFactories)
			{
				if (factory.CanHandle(namespaceUri, name))
				{
					return factory.CreateExecutor(this);
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
				if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

				Infrastructure.Assert(!_dispatcher._locationEvaluators.IsDefault);

				var locationEvaluator = _dispatcher._locationEvaluators[_index];

				return locationEvaluator.SetValue(value, executionContext, token);
			}

		#endregion

		#region Interface ILocationExpression

			public string Expression { get; }

		#endregion
		}

		private class ExpressionEvaluator : IExpressionEvaluator, IValueExpression
		{
			private readonly CustomActionDispatcher _dispatcher;
			private readonly int                    _index;

			public ExpressionEvaluator(CustomActionDispatcher dispatcher, int index, string expression)
			{
				_dispatcher = dispatcher;
				_index = index;
				Expression = expression;
			}

		#region Interface IExpressionEvaluator

			public ValueTask<DataModelValue> Evaluate(IExecutionContext executionContext, CancellationToken token)
			{
				if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

				Infrastructure.Assert(!_dispatcher._objectEvaluators.IsDefault);

				var objectEvaluator = _dispatcher._objectEvaluators[_index];

				return Evaluate(objectEvaluator, executionContext, token);
			}

		#endregion

		#region Interface IValueExpression

			public string Expression { get; }

		#endregion

			private static async ValueTask<DataModelValue> Evaluate(IObjectEvaluator objectEvaluator, IExecutionContext executionContext, CancellationToken token)
			{
				var obj = await objectEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj);
			}
		}

		private class NoActionExecutor : ICustomActionExecutor
		{
			public static readonly ICustomActionExecutor Instance = new NoActionExecutor();

		#region Interface ICustomActionExecutor

			public ValueTask Execute(IExecutionContext executionContext, CancellationToken token) => Infrastructure.Fail<ValueTask>();

		#endregion
		}
	}
}