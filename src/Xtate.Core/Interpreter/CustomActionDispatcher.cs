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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.CustomAction;
using Xtate.DataModel;

namespace Xtate.Core;

public interface ICustomActionService
{
	ICustomActionExecutor CreateCustomActionExecutor(string ns, string name, string xml);
}

public class GenericCustomActionEvaluator : ICustomAction, IExecEvaluator, IAncestorProvider
{
	private readonly ICustomAction         _customAction;
	private readonly ICustomActionExecutor _customActionExecutor;

	public GenericCustomActionEvaluator(ICustomAction customAction, ICustomActionService customActionService)
	{
		Infra.Requires(customAction);

		Infra.NotNull(customAction.XmlNamespace);
		Infra.NotNull(customAction.XmlName);
		Infra.NotNull(customAction.Xml);

		_customAction = customAction;

		//_customActionExecutor = customActionService(customAction.XmlNamespace, customAction.XmlName)
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _customAction;

#endregion

#region Interface ICustomAction

	public ImmutableArray<ILocationExpression> Locations { get; }

	public ImmutableArray<IValueExpression> Values { get; }

	public string XmlNamespace => _customAction.XmlNamespace!;

	public string XmlName => _customAction.XmlName!;

	public string Xml => _customAction.Xml!;

#endregion

#region Interface IExecEvaluator

	public async ValueTask Execute() => throw new NotImplementedException();

#endregion
}

[Obsolete]
internal sealed class CustomActionDispatcher : ICustomAction, ICustomActionDispatcher, ICustomActionContext
{
	private readonly ICustomAction           _customAction;
	private readonly IErrorProcessorService1 _errorProcessorService;
	private readonly ServiceLocator          _serviceLocator;

	private ICustomActionExecutor?                       _executor;
	private ImmutableArray<ILocationEvaluator>           _locationEvaluators;
	private ImmutableArray<ILocationExpression>.Builder? _locations;
	private ImmutableArray<IValueEvaluator>              _valueEvaluators;
	private ImmutableArray<IValueExpression>.Builder?    _values;

	public CustomActionDispatcher(IErrorProcessorService<CustomActionDispatcher> errorProcessorService, ICustomAction customAction, ServiceLocator serviceLocator)
	{
		_errorProcessorService = errorProcessorService;
		_customAction = customAction;
		_serviceLocator = serviceLocator;

		Infra.NotNull(customAction.XmlNamespace);
		Infra.NotNull(customAction.XmlName);
		Infra.NotNull(customAction.Xml);
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

		Infra.NotNull(_executor, Resources.Exception_RegistrationShouldNoOccurAfterInitialization);

		_locations ??= ImmutableArray.CreateBuilder<ILocationExpression>();

		var locationAssigner = new LocationAssigner(this, _locations.Count, expression);
		_locations.Add(locationAssigner);

		return locationAssigner;
	}

	IExpressionEvaluator ICustomActionContext.RegisterValueExpression(string expression, ExpectedValueType expectedValueType)
	{
		if (expression is null) throw new ArgumentNullException(nameof(expression));

		Infra.NotNull(_executor, Resources.Exception_RegistrationShouldNoOccurAfterInitialization);

		_values ??= ImmutableArray.CreateBuilder<IValueExpression>();

		var expressionEvaluator = new ExpressionEvaluator(this, _values.Count, expression, expectedValueType);
		_values.Add(expressionEvaluator);

		return expressionEvaluator;
	}

	void ICustomActionContext.AddValidationError<T>(string message, Exception? exception) => throw new NotImplementedException(); // _errorProcessorService.AddError<T>(this, message, exception);

	string ICustomActionContext.Xml => _customAction.Xml!;

#endregion

#region Interface ICustomActionDispatcher

	public void SetEvaluators(ImmutableArray<ILocationEvaluator> locationEvaluators, ImmutableArray<IValueEvaluator> objectEvaluators)
	{
		_locationEvaluators = locationEvaluators;
		_valueEvaluators = objectEvaluators;
	}

	public ValueTask Execute()
	{
		Infra.NotNull(_executor);

		return _executor.Execute();
	}

#endregion

	public async ValueTask SetupExecutor(ImmutableArray<ICustomActionFactory> customActionFactories, CancellationToken token)
	{
		try
		{
			var executor = await GetExecutor(customActionFactories, token).ConfigureAwait(false);

			if (executor is null)
			{
				_errorProcessorService.AddError(this, Resources.ErrorMessage_CustomActionExecutorCantBeFound);
			}

			Locations = _locations?.ToImmutable() ?? default;
			Values = _values?.ToImmutable() ?? default;

			_executor = executor;
		}
		catch (Exception ex)
		{
			_errorProcessorService.AddError(this, Resources.ErrorMessage_ErrorOnCreationCustomActionExecutor, ex);
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
			var activator = await factory.TryGetActivator(_serviceLocator, XmlNamespace, XmlName, token).ConfigureAwait(false);

			if (activator is not null)
			{
				return await activator.CreateExecutor(_serviceLocator, this, token).ConfigureAwait(false);
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

		public ValueTask Assign(DataModelValue value)
		{
			Infra.Assert(!_dispatcher._locationEvaluators.IsDefault);

			var locationEvaluator = _dispatcher._locationEvaluators[_index];

			return locationEvaluator.SetValue(value);
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

		public ExpressionEvaluator(CustomActionDispatcher dispatcher,
								   int index,
								   string expression,
								   ExpectedValueType expectedValueType)
		{
			_dispatcher = dispatcher;
			_index = index;
			_expectedValueType = expectedValueType;
			Expression = expression;
		}

	#region Interface IExpressionEvaluator

		public ValueTask<DataModelValue> Evaluate()
		{
			Infra.Assert(!_dispatcher._valueEvaluators.IsDefault);

			var valueEvaluator = _dispatcher._valueEvaluators[_index];

			return Evaluate(valueEvaluator);
		}

	#endregion

	#region Interface IValueExpression

		public string Expression { get; }

	#endregion

		private async ValueTask<DataModelValue> Evaluate(IValueEvaluator objectEvaluator)
		{
			switch (_expectedValueType)
			{
				case ExpectedValueType.String when objectEvaluator is IStringEvaluator evaluator:
					return await evaluator.EvaluateString().ConfigureAwait(false);

				case ExpectedValueType.Integer when objectEvaluator is IIntegerEvaluator evaluator:
					return await evaluator.EvaluateInteger().ConfigureAwait(false);

				case ExpectedValueType.Boolean when objectEvaluator is IBooleanEvaluator evaluator:
					return await evaluator.EvaluateBoolean().ConfigureAwait(false);
			}

			var obj = await ((IObjectEvaluator) objectEvaluator).EvaluateObject().ConfigureAwait(false);

			return DataModelValue.FromObject(obj);
		}
	}

	private class NoActionExecutor : ICustomActionExecutor
	{
		public static ICustomActionExecutor Instance { get; } = new NoActionExecutor();

	#region Interface ICustomActionExecutor

		public ValueTask Execute() => Infra.Fail<ValueTask>();

	#endregion
	}
}