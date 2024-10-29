// Copyright © 2019-2024 Sergii Artemenko
// 
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

using System.Xml.XPath;
using System.Xml.Xsl;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath;

public class XPathExpressionContext : XsltContext
{
	private List<XPathFunctionBase>? _functionDescriptors;

	private List<XPathVarDescriptor>? _varDescriptors;

	public XPathExpressionContext(INameTableProvider nameTableProvider, IXmlNamespacesInfo? xmlNamespacesInfo) : base(nameTableProvider.GetNameTable())
	{
		if (xmlNamespacesInfo?.Namespaces is { } namespaces)
		{
			foreach (var prefixNamespace in namespaces)
			{
				base.AddNamespace(prefixNamespace.Prefix, prefixNamespace.Namespace);
			}
		}
	}

	public required IEnumerable<IXPathFunctionProvider> FunctionProviders { private get; [UsedImplicitly] init; }

	public required Func<string, XPathVarDescriptor> XPathVarDescriptorFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<XPathEngine>> XPathEngineFactory { private get; [UsedImplicitly] init; }

	public override bool Whitespace => false;

	public override IXsltContextVariable ResolveVariable(string prefix, string name)
	{
		var ns = LookupNamespace(prefix);

		if (!string.IsNullOrEmpty(ns))
		{
			throw new XPathDataModelException(Res.Format(Resources.Exception_UnknownXPathVariable, ns, name));
		}

		var varDescriptor = XPathVarDescriptorFactory(name);

		_varDescriptors ??= [];
		_varDescriptors.Add(varDescriptor);

		return varDescriptor;
	}

	public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] _)
	{
		var ns = LookupNamespace(prefix);

		foreach (var provider in FunctionProviders)
		{
			if (provider.TryGetFunction(ns, name) is { } function)
			{
				_functionDescriptors ??= [];
				_functionDescriptors.Add(function);

				return function;
			}
		}

		throw new XPathDataModelException(Res.Format(Resources.Exception_UnknownXPathFunction, ns, name));
	}

	public override bool PreserveWhitespace(XPathNavigator node) => false;

	public override int CompareDocument(string baseUri, string nextbaseUri) => string.CompareOrdinal(baseUri, nextbaseUri);

	public override string LookupNamespace(string prefix) => base.LookupNamespace(prefix) ?? throw new XPathDataModelException(Res.Format(Resources.Exception_PrefixCantBeResolved, prefix));

	public ValueTask EnsureInitialized()
	{
		var varDescriptors = _varDescriptors;
		var functionDescriptors = _functionDescriptors;

		if (varDescriptors is null && functionDescriptors is null)
		{
			return default;
		}

		_varDescriptors = null;
		_functionDescriptors = null;

		return Initialize(varDescriptors, functionDescriptors);
	}

	private async ValueTask Initialize(List<XPathVarDescriptor>? varDescriptors, List<XPathFunctionBase>? functionDescriptors)
	{
		if (varDescriptors is not null)
		{
			var engine = await XPathEngineFactory().ConfigureAwait(false);

			foreach (var varDescriptor in varDescriptors)
			{
				await varDescriptor.Initialize(engine).ConfigureAwait(false);
			}
		}

		if (functionDescriptors is not null)
		{
			foreach (var functionDescriptor in functionDescriptors)
			{
				await functionDescriptor.Initialize().ConfigureAwait(false);
			}
		}
	}
}