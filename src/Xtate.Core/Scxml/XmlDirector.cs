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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;

namespace Xtate.Scxml
{
	[PublicAPI]
	public abstract class XmlDirector<TDirector> : IXmlLineInfo where TDirector : XmlDirector<TDirector>
	{
		private readonly IErrorProcessor _errorProcessor;
		private readonly bool            _useAsync;
		private readonly IXmlLineInfo?   _xmlLineInfo;
		private readonly XmlReader       _xmlReader;
		private          string?         _rawContent;

		protected XmlDirector(XmlReader xmlReader, IErrorProcessor errorProcessor, bool useAsync)
		{
			_xmlReader = xmlReader ?? throw new ArgumentNullException(nameof(xmlReader));
			_errorProcessor = errorProcessor ?? throw new ArgumentNullException(nameof(errorProcessor));
			_useAsync = useAsync;

			_xmlLineInfo = _xmlReader as IXmlLineInfo;
		}

		protected string AttributeValue => _xmlReader.Value;

		protected string CurrentName => _xmlReader.LocalName;

		protected string CurrentNamespace => _xmlReader.NamespaceURI;

		protected string RawContent => _rawContent ?? string.Empty;

	#region Interface IXmlLineInfo

		public bool HasLineInfo() => _xmlLineInfo?.HasLineInfo() ?? false;

		public int LineNumber => _xmlLineInfo?.LineNumber ?? 0;

		public int LinePosition => _xmlLineInfo?.LinePosition ?? 0;

	#endregion

		protected ValueTask Skip()
		{
			if (_useAsync)
			{
				return new ValueTask(_xmlReader.SkipAsync());
			}

			_xmlReader.Skip();

			return default;
		}

		protected ValueTask<string> ReadOuterXml() =>
				_useAsync
						? new ValueTask<string>(_xmlReader.ReadOuterXmlAsync())
						: new ValueTask<string>(_xmlReader.ReadOuterXml());

		private ValueTask<string> ReadInnerXml() =>
				_useAsync
						? new ValueTask<string>(_xmlReader.ReadInnerXmlAsync())
						: new ValueTask<string>(_xmlReader.ReadInnerXml());

		private ValueTask<XmlNodeType> MoveToContent() =>
				_useAsync
						? new ValueTask<XmlNodeType>(_xmlReader.MoveToContentAsync())
						: new ValueTask<XmlNodeType>(_xmlReader.MoveToContent());

		protected async ValueTask<TEntity> Populate<TEntity>(TEntity entity, Policy<TEntity> policy)
		{
			if (policy is null) throw new ArgumentNullException(nameof(policy));

			if (!await IsStartElement().ConfigureAwait(false))
			{
				return entity;
			}

			var validationContext = policy.CreateValidationContext(_xmlReader, _errorProcessor);

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
					AddError(Resources.ErrorMessage_FailureContentProcessing, ex);
				}

				_rawContent = default;

				return entity;
			}

			await PopulateElements(entity, policy, validationContext).ConfigureAwait(false);

			return entity;
		}

		protected static Policy<TEntity> BuildPolicy<TEntity>(Action<IPolicyBuilder<TEntity>> buildPolicy)
		{
			if (buildPolicy is null) throw new ArgumentNullException(nameof(buildPolicy));

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

		private void PopulateAttributes<TEntity>(TEntity entity, Policy<TEntity> policy, Policy<TEntity>.ValidationContext validationContext)
		{
			try
			{
				for (var exists = _xmlReader.MoveToFirstAttribute(); exists; exists = _xmlReader.MoveToNextAttribute())
				{
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
							AddError(Resources.ErrorMessage_FailureAttributeProcessing, ex);

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
							AddError(Resources.ErrorMessage_FailureElementProcessing, ex);

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

		private void AddError(string message, Exception exception) => _errorProcessor.AddError<XmlDirector<TDirector>>(_xmlReader, message, exception);

		[PublicAPI]
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

		[PublicAPI]
		protected enum AttributeType
		{
			Optional         = 1,
			Required         = 2,
			SysIncrement     = 10,
			SysOptionalFound = 11,
			SysRequiredFound = 12
		}

		[PublicAPI]
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
			private readonly Dictionary<QualifiedName, (Action<TDirector, TEntity> located, AttributeType type)> _attributes = new();

			private readonly Dictionary<QualifiedName, (Func<TDirector, TEntity, ValueTask> located, ElementType type)> _elements = new();

			public Action<TDirector, TEntity>? RawContentAction { get; set; }

			public Func<TDirector, TEntity, ValueTask>? UnknownElementAction { get; set; }

			public bool    IgnoreUnknownElements { get; set; }
			public string? ElementNamespace      { get; set; }
			public string? ElementName           { get; set; }

			public void AddAttribute(string ns, string name, Action<TDirector, TEntity> located, AttributeType type)
			{
				_attributes.Add(new QualifiedName(ns, name), (located, type));
			}

			public void AddElement(string ns, string name, Func<TDirector, TEntity, ValueTask> located, ElementType type)
			{
				_elements.Add(new QualifiedName(ns, name), (located, type));
			}

			public Action<TDirector, TEntity>? AttributeLocated(string ns, string name) => _attributes.TryGetValue(new QualifiedName(ns, name), out var value) ? value.located : null;

			public Func<TDirector, TEntity, ValueTask>? ElementLocated(string ns, string name) =>
					_elements.TryGetValue(new QualifiedName(ns, name), out var value) ? value.located : UnknownElementAction;

			public ValidationContext CreateValidationContext(XmlReader xmlReader, IErrorProcessor errorProcessor) => new(this, xmlReader, errorProcessor);

			public void FillNameTable(XmlNameTable nameTable)
			{
				if (nameTable is null) throw new ArgumentNullException(nameof(nameTable));

				FillFromQualifiedName(_elements);
				FillFromQualifiedName(_attributes);

				void FillFromQualifiedName<T>(Dictionary<QualifiedName, T> dictionary)
				{
					foreach (var pair in dictionary)
					{
						var ns = nameTable.Add(pair.Key.Namespace);
						Infrastructure.Assert(ns == pair.Key.Namespace);

						var name = nameTable.Add(pair.Key.Name);
						Infrastructure.Assert(name == pair.Key.Name);
					}
				}
			}

			private readonly struct QualifiedName : IEquatable<QualifiedName>
			{
				public readonly string Name;
				public readonly string Namespace;

				public QualifiedName(string ns, string name)
				{
					Namespace = ns;
					Name = name;
				}

			#region Interface IEquatable<XmlDirector<TDirector>.Policy<TEntity>.QualifiedName>

				public bool Equals(QualifiedName other) => ReferenceEquals(Namespace, other.Namespace) && ReferenceEquals(Name, other.Name);

			#endregion

				public override bool Equals(object? obj) => obj is QualifiedName other && Equals(other);

				public override int GetHashCode() => unchecked((Namespace.Length << 16) + Name.Length);
			}

			public class ValidationContext
			{
				private readonly Dictionary<QualifiedName, AttributeType>? _attributes;
				private readonly Dictionary<QualifiedName, ElementType>?   _elements;
				private readonly IErrorProcessor                           _errorProcessor;
				private readonly bool                                      _ignoreUnknownElements;
				private readonly XmlReader                                 _xmlReader;

				public ValidationContext(Policy<TEntity> policy, XmlReader xmlReader, IErrorProcessor errorProcessor)
				{
					if (policy is null) throw new ArgumentNullException(nameof(policy));
					_xmlReader = xmlReader ?? throw new ArgumentNullException(nameof(xmlReader));
					_errorProcessor = errorProcessor;

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
						if (!xmlReader.IsStartElement(policy.ElementName, policy.ElementNamespace))
						{
							AddError(CreateMessage(Resources.ErrorMessage_ExpectedElementNotFound, policy.ElementNamespace, policy.ElementName));
						}
					}
				}

				public void ValidateAttribute(string ns, string name)
				{
					if (_attributes is not null && _attributes.TryGetValue(new QualifiedName(ns, name), out var type))
					{
						if (type == AttributeType.SysOptionalFound || type == AttributeType.SysRequiredFound)
						{
							AddError(CreateMessage(Resources.ErrorMessage_FoundDuplicateAttribute, ns, name));
						}

						_attributes[new QualifiedName(ns, name)] = type + (int) AttributeType.SysIncrement;
					}
				}

				public void ProcessAttributesCompleted()
				{
					if (_attributes is not null && _attributes.Any(pair => pair.Value == AttributeType.Required))
					{
						var query = _attributes.Where(pair => pair.Value == AttributeType.Required).Select(p => p.Key);
						AddError(CreateMessage(Resources.ErrorMessage_MissedRequiredAttributes, delimiter: @"', '", query));
					}
				}

				public void ValidateElement(string ns, string name)
				{
					if (_elements is not null && _elements.TryGetValue(new QualifiedName(ns, name), out var type))
					{
						if (type == ElementType.SysOneFound || type == ElementType.SysZeroToOneFound)
						{
							AddError(CreateMessage(Resources.ErrorMessage_OnlyOneElementAllowed, ns, name));
						}

						_elements[new QualifiedName(ns, name)] = type + (int) ElementType.SysIncrement;
					}
					else if (!_ignoreUnknownElements)
					{
						AddError(CreateMessage(Resources.ErrorMessage_DetectedUnknownElement, ns, name));
					}
				}

				public void ProcessElementsCompleted()
				{
					if (_elements is not null && _elements.Any(pair => pair.Value == ElementType.One || pair.Value == ElementType.OneToMany))
					{
						var query = _elements.Where(pair => pair.Value == ElementType.One || pair.Value == ElementType.OneToMany).Select(p => p.Key);
						AddError(CreateMessage(Resources.ErrorMessage_MissedRequiredElements, delimiter: @">, <", query));
					}
				}

				private static string CreateMessage(string format, string ns, string name) => string.Format(CultureInfo.InvariantCulture, format, string.IsNullOrEmpty(ns) ? name : ns + @":" + name);

				private static string CreateMessage(string format, string delimiter, IEnumerable<QualifiedName> names)
				{
					var query = names.Select(qualifiedName => string.IsNullOrEmpty(qualifiedName.Namespace) ? qualifiedName.Name : qualifiedName.Namespace + @":" + qualifiedName.Name);
					return string.Format(CultureInfo.InvariantCulture, format, string.Join(delimiter, query));
				}

				private void AddError(string message) => _errorProcessor.AddError<XmlDirector<TDirector>>(_xmlReader, message);
			}
		}

		private class PolicyBuilder<TEntity> : IPolicyBuilder<TEntity>
		{
			private readonly Policy<TEntity> _policy;

			private bool? _rawContent;

			public PolicyBuilder(Policy<TEntity> policy) => _policy = policy;

		#region Interface XmlDirector<TDirector>.IPolicyBuilder<TEntity>

			public IPolicyBuilder<TEntity> IgnoreUnknownElements(bool value)
			{
				_policy.IgnoreUnknownElements = value;

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
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

				_policy.ElementNamespace = ns ?? throw new ArgumentNullException(nameof(ns));
				_policy.ElementName = name;

				return this;
			}

			public IPolicyBuilder<TEntity> RequiredAttribute(string ns, string name, Action<TDirector, TEntity> located)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (located is null) throw new ArgumentNullException(nameof(located));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

				_policy.AddAttribute(ns, name, located, AttributeType.Required);

				return this;
			}

			public IPolicyBuilder<TEntity> OptionalAttribute(string ns, string name, Action<TDirector, TEntity> located)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (located is null) throw new ArgumentNullException(nameof(located));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

				_policy.AddAttribute(ns, name, located, AttributeType.Optional);

				return this;
			}

			public IPolicyBuilder<TEntity> OptionalElement(string ns, string name, Func<TDirector, TEntity, ValueTask> located)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (located is null) throw new ArgumentNullException(nameof(located));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

				_policy.AddElement(ns, name, located, ElementType.ZeroToOne);

				return this;
			}

			public IPolicyBuilder<TEntity> SingleElement(string ns, string name, Func<TDirector, TEntity, ValueTask> located)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (located is null) throw new ArgumentNullException(nameof(located));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

				UseRawContent(value: false);
				_policy.AddElement(ns, name, located, ElementType.One);

				return this;
			}

			public IPolicyBuilder<TEntity> MultipleElements(string ns, string name, Func<TDirector, TEntity, ValueTask> located)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (located is null) throw new ArgumentNullException(nameof(located));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

				UseRawContent(value: false);
				_policy.AddElement(ns, name, located, ElementType.ZeroToMany);

				return this;
			}

			public IPolicyBuilder<TEntity> UnknownElement(Func<TDirector, TEntity, ValueTask> located)
			{
				_policy.UnknownElementAction = located ?? throw new ArgumentNullException(nameof(located));

				return this;
			}

			public IPolicyBuilder<TEntity> IgnoreUnknownElements()
			{
				_policy.IgnoreUnknownElements = true;

				return this;
			}

			public IPolicyBuilder<TEntity> DenyUnknownElements()
			{
				_policy.IgnoreUnknownElements = false;

				return this;
			}

			public IPolicyBuilder<TEntity> RawContent(Action<TDirector, TEntity> action)
			{
				UseRawContent(value: true);
				_policy.RawContentAction = action;

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
	}
}