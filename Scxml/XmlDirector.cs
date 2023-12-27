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

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Xtate.Scxml;

public abstract class XmlDirector<TDirector> where TDirector : XmlDirector<TDirector>
{
	private readonly string    _xmlnsPrefix;
	private readonly XmlReader _xmlReader;

	private string? _rawContent;

	protected XmlDirector(XmlReader xmlReader)
	{
		_xmlReader = xmlReader;
		var nameTable = xmlReader.NameTable;

		Infra.NotNull(nameTable);

		_xmlnsPrefix = nameTable.Add(@"xmlns");
	}

	protected string AttributeValue => _xmlReader.Value;

	protected string CurrentName => _xmlReader.LocalName;

	protected string CurrentNamespace => _xmlReader.NamespaceURI;

	protected string RawContent => _rawContent ?? string.Empty;

	private bool UseAsync => _xmlReader.Settings?.Async ?? false;

	protected ValueTask Skip()
	{
		if (UseAsync)
		{
			return new ValueTask(_xmlReader.SkipAsync());
		}

		_xmlReader.Skip();

		return default;
	}

	protected ValueTask<string> ReadOuterXml() =>
		UseAsync
			? new ValueTask<string>(_xmlReader.ReadOuterXmlAsync())
			: new ValueTask<string>(_xmlReader.ReadOuterXml());

	private ValueTask<string> ReadInnerXml() =>
		UseAsync
			? new ValueTask<string>(_xmlReader.ReadInnerXmlAsync())
			: new ValueTask<string>(_xmlReader.ReadInnerXml());

	private ValueTask<XmlNodeType> MoveToContent() =>
		UseAsync
			? new ValueTask<XmlNodeType>(_xmlReader.MoveToContentAsync())
			: new ValueTask<XmlNodeType>(_xmlReader.MoveToContent());

	protected async ValueTask<TEntity> Populate<TEntity>(TEntity entity, Policy<TEntity> policy)
	{
		Infra.Requires(policy);

		if (!await IsStartElement().ConfigureAwait(false))
		{
			return entity;
		}

		var validationContext = policy.CreateValidationContext(this);

		PopulateAttributes(entity, policy, validationContext);

		if (_xmlReader.IsEmptyElement)
		{
			await ReadStartElement().ConfigureAwait(false);

			return entity;
		}

		if (policy.RawContentAction is { } policyRawContentAction)
		{
			_rawContent = await ReadInnerXml().ConfigureAwait(false);

			try
			{
				policyRawContentAction((TDirector) this, entity);
			}
			catch (Exception ex)
			{
				OnError(Resources.ErrorMessage_FailureContentProcessing, ex);
			}

			_rawContent = default;

			return entity;
		}

		await PopulateElements(entity, policy, validationContext).ConfigureAwait(false);

		return entity;
	}

	protected static Policy<TEntity> BuildPolicy<TEntity>(Action<IPolicyBuilder<TEntity>> buildPolicy)
	{
		Infra.Requires(buildPolicy);

		var policy = new Policy<TEntity>();

		buildPolicy(new PolicyBuilder<TEntity>(policy));

		return policy;
	}

	private bool ValidXmlReader() => _xmlReader.ReadState == ReadState.Interactive;

	private async ValueTask<bool> IsStartElement()
	{
		if (_xmlReader.NodeType != XmlNodeType.Element)
		{
			await MoveToContent().ConfigureAwait(false);
		}

		return ValidXmlReader() && _xmlReader.IsStartElement();
	}

	private async ValueTask ReadStartElement()
	{
		if (_xmlReader.NodeType != XmlNodeType.Element)
		{
			await MoveToContent().ConfigureAwait(false);
		}

		_xmlReader.ReadStartElement();
	}

	private async ValueTask ReadEndElement()
	{
		if (_xmlReader.NodeType != XmlNodeType.EndElement)
		{
			await MoveToContent().ConfigureAwait(false);
		}

		_xmlReader.ReadEndElement();
	}

	protected virtual void NamespaceAttribute(string prefix) { }

	private void PopulateAttributes<TEntity>(TEntity entity, Policy<TEntity> policy, Policy<TEntity>.ValidationContext validationContext)
	{
		try
		{
			for (var exists = _xmlReader.MoveToFirstAttribute(); exists; exists = _xmlReader.MoveToNextAttribute())
			{
				if (ReferenceEquals(_xmlReader.Prefix, _xmlnsPrefix))
				{
					NamespaceAttribute(_xmlReader.LocalName);
				}

				var ns = _xmlReader.NamespaceURI;
				var name = _xmlReader.LocalName;
				validationContext.ValidateAttribute(ns, name);

				if (policy.AttributeLocated(ns, name) is { } located)
				{
					try
					{
						located((TDirector) this, entity);
					}
					catch (Exception ex)
					{
						OnError(Resources.ErrorMessage_FailureAttributeProcessing, ex);

						if (!ValidXmlReader())
						{
							return;
						}
					}
				}
			}

			_xmlReader.MoveToElement();
		}
		finally
		{
			validationContext.ProcessAttributesCompleted();
		}
	}

	private async ValueTask PopulateElements<TEntity>(TEntity entity, Policy<TEntity> policy, Policy<TEntity>.ValidationContext validationContext)
	{
		try
		{
			await ReadStartElement().ConfigureAwait(false);

			while (await IsStartElement().ConfigureAwait(false))
			{
				var ns = _xmlReader.NamespaceURI;
				var name = _xmlReader.LocalName;
				validationContext.ValidateElement(ns, name);

				if (policy.ElementLocated(ns, name) is { } located)
				{
					try
					{
						await located((TDirector) this, entity).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						OnError(Resources.ErrorMessage_FailureElementProcessing, ex);

						if (!ValidXmlReader())
						{
							return;
						}
					}
				}
				else
				{
					await Skip().ConfigureAwait(false);
				}
			}

			await ReadEndElement().ConfigureAwait(false);
		}
		finally
		{
			validationContext.ProcessElementsCompleted();
		}
	}

	protected abstract void OnError(string message, Exception? exception);

	protected interface IPolicyBuilder<out TEntity>
	{
		IPolicyBuilder<TEntity> IgnoreUnknownElements(bool value);
		IPolicyBuilder<TEntity> ValidateElementName([Localizable(false)] string name);
		IPolicyBuilder<TEntity> RequiredAttribute([Localizable(false)] string name, Action<TDirector, TEntity> located);
		IPolicyBuilder<TEntity> OptionalAttribute([Localizable(false)] string name, Action<TDirector, TEntity> located);
		IPolicyBuilder<TEntity> OptionalElement([Localizable(false)] string name, Func<TDirector, TEntity, ValueTask> located);
		IPolicyBuilder<TEntity> SingleElement([Localizable(false)] string name, Func<TDirector, TEntity, ValueTask> located);
		IPolicyBuilder<TEntity> MultipleElements([Localizable(false)] string name, Func<TDirector, TEntity, ValueTask> located);
		IPolicyBuilder<TEntity> ValidateElementName([Localizable(false)] string ns, [Localizable(false)] string name);
		IPolicyBuilder<TEntity> RequiredAttribute([Localizable(false)] string ns, [Localizable(false)] string name, Action<TDirector, TEntity> located);
		IPolicyBuilder<TEntity> OptionalAttribute([Localizable(false)] string ns, [Localizable(false)] string name, Action<TDirector, TEntity> located);
		IPolicyBuilder<TEntity> OptionalElement([Localizable(false)] string ns, [Localizable(false)] string name, Func<TDirector, TEntity, ValueTask> located);
		IPolicyBuilder<TEntity> SingleElement([Localizable(false)] string ns, [Localizable(false)] string name, Func<TDirector, TEntity, ValueTask> located);
		IPolicyBuilder<TEntity> MultipleElements([Localizable(false)] string ns, [Localizable(false)] string name, Func<TDirector, TEntity, ValueTask> located);
		IPolicyBuilder<TEntity> IgnoreUnknownElements();
		IPolicyBuilder<TEntity> DenyUnknownElements();
		IPolicyBuilder<TEntity> RawContent(Action<TDirector, TEntity> located);
		IPolicyBuilder<TEntity> UnknownElement(Func<TDirector, TEntity, ValueTask> located);
	}

	protected enum AttributeType
	{
		Optional         = 1,
		Required         = 2,
		SysIncrement     = 10,
		SysOptionalFound = 11,
		SysRequiredFound = 12
	}

	protected enum ElementType
	{
		ZeroToMany         = 1,
		ZeroToOne          = 2,
		One                = 3,
		OneToMany          = 4,
		SysIncrement       = 10,
		SysZeroToManyFound = 11,
		SysZeroToOneFound  = 12,
		SysOneFound        = 13,
		SysOneToManyFound  = 14
	}

	protected class Policy<TEntity>
	{
		private readonly Dictionary<QualifiedName, (Action<TDirector, TEntity> located, AttributeType type)> _attributes = [];

		private readonly Dictionary<QualifiedName, (Func<TDirector, TEntity, ValueTask> located, ElementType type)> _elements = [];

		public Action<TDirector, TEntity>? RawContentAction { get; set; }

		public Func<TDirector, TEntity, ValueTask>? UnknownElementAction { get; set; }

		public bool    IgnoreUnknownElements { get; set; }
		public string? ElementNamespace      { get; set; }
		public string? ElementName           { get; set; }

		public void AddAttribute(string ns,
								 string name,
								 Action<TDirector, TEntity> located,
								 AttributeType type)
		{
			_attributes.Add(new QualifiedName(ns, name), (located, type));
		}

		public void AddElement(string ns,
							   string name,
							   Func<TDirector, TEntity, ValueTask> located,
							   ElementType type)
		{
			_elements.Add(new QualifiedName(ns, name), (located, type));
		}

		public Action<TDirector, TEntity>? AttributeLocated(string ns, string name) => _attributes.TryGetValue(new QualifiedName(ns, name), out var value) ? value.located : null;

		public Func<TDirector, TEntity, ValueTask>? ElementLocated(string ns, string name) => _elements.TryGetValue(new QualifiedName(ns, name), out var value) ? value.located : UnknownElementAction;

		public ValidationContext CreateValidationContext(XmlDirector<TDirector> xmlDirector) => new(this, xmlDirector);

		public void FillNameTable(XmlNameTable nameTable)
		{
			Infra.Requires(nameTable);

			FillFromQualifiedName(_elements);
			FillFromQualifiedName(_attributes);

			void FillFromQualifiedName<T>(Dictionary<QualifiedName, T> dictionary)
			{
				foreach (var pair in dictionary)
				{
					var ns = nameTable.Add(pair.Key.Namespace);
					Debug.Assert(ns == pair.Key.Namespace);

					var name = nameTable.Add(pair.Key.Name);
					Debug.Assert(name == pair.Key.Name);
				}
			}
		}

		public class ValidationContext
		{
			private readonly Dictionary<QualifiedName, AttributeType>? _attributes;
			private readonly Dictionary<QualifiedName, ElementType>?   _elements;
			private readonly bool                                      _ignoreUnknownElements;
			private readonly XmlDirector<TDirector>                    _xmlDirector;

			public ValidationContext(Policy<TEntity> policy, XmlDirector<TDirector> xmlDirector)
			{
				Infra.Requires(policy);
				Infra.Requires(xmlDirector);

				_xmlDirector = xmlDirector;

				if (policy._attributes.Count > 0)
				{
					_attributes = new Dictionary<QualifiedName, AttributeType>(policy._attributes.Count);
					foreach (var pair in policy._attributes)
					{
						_attributes.Add(pair.Key, pair.Value.type);
					}
				}

				if (policy._elements.Count > 0)
				{
					_elements = new Dictionary<QualifiedName, ElementType>(policy._elements.Count);
					foreach (var pair in policy._elements)
					{
						_elements.Add(pair.Key, pair.Value.type);
					}
				}

				_ignoreUnknownElements = policy.IgnoreUnknownElements || policy.UnknownElementAction is not null;

				if (policy.ElementName is not null && policy.ElementNamespace is not null)
				{
					if (!_xmlDirector._xmlReader.IsStartElement(policy.ElementName, policy.ElementNamespace))
					{
						OnError(CreateMessage(Resources.ErrorMessage_ExpectedElementNotFound, policy.ElementNamespace, policy.ElementName));
					}
				}
			}

			public void ValidateAttribute(string ns, string name)
			{
				if (_attributes is not null && _attributes.TryGetValue(new QualifiedName(ns, name), out var type))
				{
					if (type == AttributeType.SysOptionalFound || type == AttributeType.SysRequiredFound)
					{
						OnError(CreateMessage(Resources.ErrorMessage_FoundDuplicateAttribute, ns, name));
					}

					_attributes[new QualifiedName(ns, name)] = type + (int) AttributeType.SysIncrement;
				}
			}

			public void ProcessAttributesCompleted()
			{
				if (_attributes is not null && _attributes.Any(pair => pair.Value == AttributeType.Required))
				{
					var query = _attributes.Where(pair => pair.Value == AttributeType.Required).Select(p => p.Key);
					OnError(CreateMessage(Resources.ErrorMessage_MissedRequiredAttributes, delimiter: @"', '", query));
				}
			}

			public void ValidateElement(string ns, string name)
			{
				if (_elements is not null && _elements.TryGetValue(new QualifiedName(ns, name), out var type))
				{
					if (type is ElementType.SysOneFound or ElementType.SysZeroToOneFound)
					{
						OnError(CreateMessage(Resources.ErrorMessage_OnlyOneElementAllowed, ns, name));
					}

					_elements[new QualifiedName(ns, name)] = type + (int) ElementType.SysIncrement;
				}
				else if (!_ignoreUnknownElements)
				{
					OnError(CreateMessage(Resources.ErrorMessage_DetectedUnknownElement, ns, name));
				}
			}

			public void ProcessElementsCompleted()
			{
				if (_elements is not null && _elements.Any(pair => pair.Value is ElementType.One or ElementType.OneToMany))
				{
					var query = _elements.Where(pair => pair.Value is ElementType.One or ElementType.OneToMany).Select(p => p.Key);
					OnError(CreateMessage(Resources.ErrorMessage_MissedRequiredElements, delimiter: @">, <", query));
				}
			}

			private static string CreateMessage(string format, string ns, string name) => string.Format(CultureInfo.InvariantCulture, format, string.IsNullOrEmpty(ns) ? name : ns + @":" + name);

			private static string CreateMessage(string format, string delimiter, IEnumerable<QualifiedName> names)
			{
				var query = names.Select(qualifiedName => string.IsNullOrEmpty(qualifiedName.Namespace) ? qualifiedName.Name : qualifiedName.Namespace + @":" + qualifiedName.Name);
				return string.Format(CultureInfo.InvariantCulture, format, string.Join(delimiter, query));
			}

			private void OnError(string message) => _xmlDirector.OnError(message, exception: default);
		}
	}

	private class PolicyBuilder<TEntity>(Policy<TEntity> policy) : IPolicyBuilder<TEntity>
	{
		private bool? _rawContent;

		#region Interface XmlDirector<TDirector>.IPolicyBuilder<TEntity>

		public IPolicyBuilder<TEntity> IgnoreUnknownElements(bool value)
		{
			policy.IgnoreUnknownElements = value;

			return this;
		}

		public IPolicyBuilder<TEntity> ValidateElementName(string name) => ValidateElementName(string.Empty, name);

		public IPolicyBuilder<TEntity> RequiredAttribute(string name, Action<TDirector, TEntity> located) => RequiredAttribute(string.Empty, name, located);

		public IPolicyBuilder<TEntity> OptionalAttribute(string name, Action<TDirector, TEntity> located) => OptionalAttribute(string.Empty, name, located);

		public IPolicyBuilder<TEntity> OptionalElement(string name, Func<TDirector, TEntity, ValueTask> located) => OptionalElement(string.Empty, name, located);

		public IPolicyBuilder<TEntity> SingleElement(string name, Func<TDirector, TEntity, ValueTask> located) => SingleElement(string.Empty, name, located);

		public IPolicyBuilder<TEntity> MultipleElements(string name, Func<TDirector, TEntity, ValueTask> located) => MultipleElements(string.Empty, name, located);

		public IPolicyBuilder<TEntity> ValidateElementName(string ns, string name)
		{
			Infra.Requires(ns);
			Infra.RequiresNonEmptyString(name);

			policy.ElementNamespace = ns;
			policy.ElementName = name;

			return this;
		}

		public IPolicyBuilder<TEntity> RequiredAttribute(string ns, string name, Action<TDirector, TEntity> located)
		{
			Infra.Requires(ns);
			Infra.RequiresNonEmptyString(name);
			Infra.Requires(located);

			policy.AddAttribute(ns, name, located, AttributeType.Required);

			return this;
		}

		public IPolicyBuilder<TEntity> OptionalAttribute(string ns, string name, Action<TDirector, TEntity> located)
		{
			Infra.Requires(ns);
			Infra.RequiresNonEmptyString(name);
			Infra.Requires(located);

			policy.AddAttribute(ns, name, located, AttributeType.Optional);

			return this;
		}

		public IPolicyBuilder<TEntity> OptionalElement(string ns, string name, Func<TDirector, TEntity, ValueTask> located)
		{
			Infra.Requires(ns);
			Infra.RequiresNonEmptyString(name);
			Infra.Requires(located);

			policy.AddElement(ns, name, located, ElementType.ZeroToOne);

			return this;
		}

		public IPolicyBuilder<TEntity> SingleElement(string ns, string name, Func<TDirector, TEntity, ValueTask> located)
		{
			Infra.Requires(ns);
			Infra.RequiresNonEmptyString(name);
			Infra.Requires(located);

			UseRawContent(value: false);
			policy.AddElement(ns, name, located, ElementType.One);

			return this;
		}

		public IPolicyBuilder<TEntity> MultipleElements(string ns, string name, Func<TDirector, TEntity, ValueTask> located)
		{
			Infra.Requires(ns);
			Infra.RequiresNonEmptyString(name);
			Infra.Requires(located);

			UseRawContent(value: false);
			policy.AddElement(ns, name, located, ElementType.ZeroToMany);

			return this;
		}

		public IPolicyBuilder<TEntity> UnknownElement(Func<TDirector, TEntity, ValueTask> located)
		{
			Infra.Requires(located);

			policy.UnknownElementAction = located;

			return this;
		}

		public IPolicyBuilder<TEntity> IgnoreUnknownElements()
		{
			policy.IgnoreUnknownElements = true;

			return this;
		}

		public IPolicyBuilder<TEntity> DenyUnknownElements()
		{
			policy.IgnoreUnknownElements = false;

			return this;
		}

		public IPolicyBuilder<TEntity> RawContent(Action<TDirector, TEntity> action)
		{
			UseRawContent(value: true);
			policy.RawContentAction = action;

			return this;
		}

#endregion

		private void UseRawContent(bool value)
		{
			if (_rawContent == value)
			{
				if (value)
				{
					throw new ArgumentException(Resources.Exception_CanNotRegisterRawContentMoreThanOneTime);
				}

				return;
			}

			if (!_rawContent.HasValue)
			{
				_rawContent = value;

				return;
			}

			if (value)
			{
				throw new ArgumentException(Resources.Exception_CanNotReadRawContentDueToRegisteredElements);
			}

			throw new ArgumentException(Resources.Exception_CanNotRegisterComponentDueToRegisteredRawContent);
		}
	}

	private readonly struct QualifiedName(string ns, string name) : IEquatable<QualifiedName>
	{
		public readonly string Name = name;
		public readonly string Namespace = ns;

		#region Interface IEquatable<XmlDirector<TDirector>.QualifiedName>

		public bool Equals(QualifiedName other) => ReferenceEquals(Namespace, other.Namespace) && ReferenceEquals(Name, other.Name);

#endregion

		public override bool Equals(object? obj) => obj is QualifiedName other && Equals(other);

		public override int GetHashCode() => unchecked((Namespace.Length << 16) + Name.Length);
	}
}