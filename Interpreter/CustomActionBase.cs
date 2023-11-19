#region Copyright © 2019-2021 Sergii Artemenko

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
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;
using Xtate.DataModel;

namespace Xtate.CustomAction
{
	public interface ICustomActionActivator
	{
		CustomActionBase Activate(string xml);
	}

	public interface ICustomActionProvider
	{
		ICustomActionActivator? TryGetActivator(string ns, string name);
	}

	public abstract class CustomActionProvider<TCustomAction> : ICustomActionProvider, ICustomActionActivator where TCustomAction : CustomActionBase
	{
		private readonly NameTable _nameTable = default!;
		
		private readonly string    _ns;
		private readonly string    _name;
		
		protected CustomActionProvider(string Namespace, string Name)
		{
			_ns = Namespace;
			_name = Name;
		}

		public required  Func<XmlReader, TCustomAction> CustomActionFactory { private get; init; }

		public required INameTableProvider? NameTableProvider
		{
			init
			{
				Infra.Requires(value);

				_nameTable = value.GetNameTable();

				_ns = _nameTable.Add(_ns);
				_name = _nameTable.Add(_name);
			}
		}

		public virtual ICustomActionActivator? TryGetActivator(string ns, string name) => ns == _ns && name == _name ? this : default;

		public virtual CustomActionBase Activate(string xml)
		{
			using var stringReader = new StringReader(xml);

			var nsManager = new XmlNamespaceManager(_nameTable);
			var context = new XmlParserContext(_nameTable, nsManager, xmlLang: null, xmlSpace: default);

			using var xmlReader = XmlReader.Create(stringReader, settings: null, context);

			xmlReader.MoveToContent();

			Infra.Assert(xmlReader.NamespaceURI == _ns);
			Infra.Assert(xmlReader.LocalName == _name);

			return CustomActionFactory(xmlReader);
		}
	}

	public class CustomActionFactory
	{
		public required IEnumerable<ICustomActionProvider> CustomActionProviders { private get; init; }

		public CustomActionBase GetCustomAction(ICustomAction customAction)
		{
			Infra.Requires(customAction);

			var ns = customAction.XmlNamespace;
			var name = customAction.XmlName;
			var xml = customAction.Xml;

			Infra.NotNull(ns);
			Infra.NotNull(name);
			Infra.NotNull(xml);

			using var enumerator = CustomActionProviders.GetEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.TryGetActivator(ns, name) is not { } activator)
				{
					continue;
				}

				while (enumerator.MoveNext())
				{
					if (enumerator.Current.TryGetActivator(ns, name) is not null)
					{
						throw Infra.Fail<Exception>(Res.Format(Resources.Exception_MoreThenOneCustomActionProviderRegisteredForProcessingCustomActionNode, ns, name));
					}
				}

				return activator.Activate(xml);
			}

			throw Infra.Fail<Exception>(Res.Format(Resources.Exception_ThereIsNoAnyCustomActionProviderRegisteredForProcessingCustomActionNode, ns, name));
		}
	}

	public abstract class CustomActionBase : ICustomActionExecutor //TODO: delete ICustomActionExecutor
	{
		private Dictionary<string, object?>? _arguments;
		private ICustomActionContext?        _customActionContext;
		private ILocationAssigner?           _resultLocationAssigner;

		protected CustomActionBase() { }

		protected CustomActionBase(ICustomActionContext customActionContext) => _customActionContext = customActionContext;

		[Obsolete]
		async ValueTask ICustomActionExecutor.Execute()
		{
			throw new NotSupportedException();
		}

		public ImmutableDictionary<object, IValueExpression> Values    { get; }
		public ImmutableDictionary<object, ILocationExpression> Locations { get; }

		protected void RegisterArgument(string key,
										ExpectedValueType expectedValueType,
										string? expression,
										object? defaultValue = default)
		{
			Infra.NotNull(_customActionContext);

			_arguments ??= new Dictionary<string, object?>();
			_arguments.Add(key, expression is not null ? _customActionContext.RegisterValueExpression(expression, expectedValueType) : defaultValue);
		}

		protected void RegisterResultLocation(string? expression)
		{
			Infra.NotNull(_customActionContext);

			if (expression is not null)
			{
				_resultLocationAssigner = _customActionContext.RegisterLocationExpression(expression);
			}
		}

		protected virtual ValueTask<DataModelValue> EvaluateAsync(IReadOnlyDictionary<string, DataModelValue> arguments) => new(Evaluate(arguments));

		protected virtual DataModelValue Evaluate(IReadOnlyDictionary<string, DataModelValue> arguments) => throw new NotSupportedException(Resources.Exception_CustomActionDoesNotSupported);

		public virtual async ValueTask Execute()
		{
			var arguments = ImmutableDictionary<string, DataModelValue>.Empty;

			if (_arguments is not null)
			{
				var builder = ImmutableDictionary.CreateBuilder<string, DataModelValue>();

				foreach (var pair in _arguments)
				{
					switch (pair.Value)
					{
						case IExpressionEvaluator expressionEvaluator:
							builder.Add(pair.Key, await expressionEvaluator.Evaluate().ConfigureAwait(false));
							break;

						default:
							builder.Add(pair.Key, DataModelValue.FromObject(pair.Value));
							break;
					}
				}

				arguments = builder.ToImmutable();
			}

			var result = await EvaluateAsync(arguments).ConfigureAwait(false);

			if (_resultLocationAssigner is not null)
			{
				await _resultLocationAssigner.Assign(result).ConfigureAwait(false);
			}
		}

		[Obsolete]
		internal void SetContextAndInitialize(ICustomActionContext customActionContext, XmlReader xmlReader)
		{
			if (_customActionContext is null)
			{
				_customActionContext = customActionContext;

				Initialize(xmlReader);
			}
		}

		protected virtual void Initialize(XmlReader xmlReader) { }

		public virtual IEnumerable<Value> GetValues() => Array.Empty<Value>();

		public virtual IEnumerable<Location> GetLocations() => Array.Empty<Location>();

		public class ArrayValue : Value
		{
			private IArrayEvaluator? _arrayEvaluator;

			public ArrayValue(string? expression) : base(expression) { }

			internal override void SetEvaluator(IValueEvaluator valueEvaluator)
			{
				base.SetEvaluator(valueEvaluator);

				_arrayEvaluator = valueEvaluator as IArrayEvaluator;
			}

			public new async ValueTask<object?[]> GetValue()
			{
				var array = _arrayEvaluator is not null
					? await _arrayEvaluator.EvaluateArray().ConfigureAwait(false)
					: (IObject[]) await base.GetValue().ConfigureAwait(false);

				return Array.ConvertAll(array ?? Array.Empty<IObject>(), i => i.ToObject());
			}
		}

		public class StringValue : Value
		{
			private IStringEvaluator? _stringEvaluator;

			public StringValue(string? expression, string? defaultValue = default) : base(expression, defaultValue) { }

			internal override void SetEvaluator(IValueEvaluator valueEvaluator)
			{
				base.SetEvaluator(valueEvaluator);

				_stringEvaluator = valueEvaluator as IStringEvaluator;
			}

			public new async ValueTask<string?> GetValue() => _stringEvaluator is not null
				? await _stringEvaluator.EvaluateString().ConfigureAwait(false)
				: (string?)await base.GetValue().ConfigureAwait(false);
		}

		public class IntegerValue : Value
		{
			private IIntegerEvaluator? _integerEvaluator;

			public IntegerValue(string? expression, int? defaultValue = default) : base(expression, defaultValue) { }

			internal override void SetEvaluator(IValueEvaluator valueEvaluator)
			{
				base.SetEvaluator(valueEvaluator);

				_integerEvaluator = valueEvaluator as IIntegerEvaluator;
			}

			public new async ValueTask<int> GetValue() => _integerEvaluator is not null
				? await _integerEvaluator.EvaluateInteger().ConfigureAwait(false)
				: (int)await base.GetValue().ConfigureAwait(false)!;
		}

		public class BooleanValue : Value
		{
			private IBooleanEvaluator? _booleanEvaluator;

			public BooleanValue(string? expression, bool? defaultValue = default) : base(expression, defaultValue) { }

			internal override void SetEvaluator(IValueEvaluator valueEvaluator)
			{
				base.SetEvaluator(valueEvaluator);

				_booleanEvaluator = valueEvaluator as IBooleanEvaluator;
			}

			public new async ValueTask<bool> GetValue() => _booleanEvaluator is not null
				? await _booleanEvaluator.EvaluateBoolean().ConfigureAwait(false)
				: (bool)await base.GetValue().ConfigureAwait(false)!;
		}
		
		public class Value : IValueExpression
		{
			private readonly string? _expression;
			private readonly IObject _defaultValue;

			private IObjectEvaluator? _objectEvaluator;

			protected Value(string? expression, object? defaultValue = default)
			{
				_expression = expression;

				_defaultValue = expression is null ? new DefaultObject(defaultValue) : DefaultObject.Null;
			}

			string? IValueExpression.Expression => _expression;

			internal virtual void SetEvaluator(IValueEvaluator valueEvaluator) => _objectEvaluator = valueEvaluator as IObjectEvaluator;

			internal ValueTask<IObject> GetObject() => _objectEvaluator?.EvaluateObject() ?? new ValueTask<IObject>(_defaultValue);

			public async ValueTask<object?> GetValue() => (await GetObject().ConfigureAwait(false)).ToObject();
		}

		public class Location : ILocationExpression
		{
			private readonly string? _expression;
			
			private ILocationEvaluator? _locationEvaluator;

			public Location(string? expression) => _expression = expression;

			string? ILocationExpression.Expression => _expression;

			internal void SetEvaluator(ILocationEvaluator locationEvaluator) => _locationEvaluator = locationEvaluator;

			public ValueTask SetValue(object? value) => _locationEvaluator?.SetValue(new DefaultObject(value)) ?? default;

			public async ValueTask CopyFrom(Value value)
			{
				Infra.Requires(value);

				if (_locationEvaluator is not null)
				{
					var val = await value.GetObject().ConfigureAwait(false);
					await _locationEvaluator.SetValue(val).ConfigureAwait(false);
				}
			}
		}
	}
}