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

<<<<<<< Updated upstream
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Xsl;
using Xtate.Core;
=======
using System.Xml.XPath;
using System.Xml.Xsl;
>>>>>>> Stashed changes
using Xtate.Scxml;

namespace Xtate.DataModel.XPath;

<<<<<<< Updated upstream
public interface IInitResolver
{
	ValueTask Initialize();
}

public class XPathExpressionContext : XsltContext
{
	private List<IInitResolver>? _initResolvers;

	public required IEnumerable<IXPathFunctionProvider> FunctionProviders         { private get; init; }
	public required Func<string, XPathVarDescriptor>    XPathVarDescriptorFactory { private get; init; }
	

	public XPathExpressionContext(INameTableProvider nameTableProvider, IXmlNamespacesInfo? xmlNamespacesInfo) : base(nameTableProvider.GetNameTable())
	{
		if (xmlNamespacesInfo is not null)
		{
			foreach (var prefixNamespace in xmlNamespacesInfo.Namespaces)
			{
				AddNamespace(prefixNamespace.Prefix, prefixNamespace.Namespace);
=======
public class XPathExpressionContext : XsltContext
{
	private List<XPathFunctionDescriptorBase>? _functionDescriptors;
	private List<XPathVarDescriptor>? _varDescriptors;

	public XPathExpressionContext(INameTableProvider nameTableProvider, IXmlNamespacesInfo? xmlNamespacesInfo) : base(nameTableProvider.GetNameTable())
	{
		if (xmlNamespacesInfo?.Namespaces is { } namespaces)
		{
			foreach (var prefixNamespace in namespaces)
			{
				base.AddNamespace(prefixNamespace.Prefix, prefixNamespace.Namespace);
>>>>>>> Stashed changes
			}
		}
	}

<<<<<<< Updated upstream
	public override bool Whitespace => false;

	public override IXsltContextVariable ResolveVariable(string prefix, string name)
	{
		var ns = LookupNamespace(prefix);

		if (!string.IsNullOrEmpty(ns))
		{
			throw new XPathDataModelException(Res.Format(Resources.Exception_UnknownXPathVariable, ns, name));
		}

		var varDescriptor = XPathVarDescriptorFactory(name);

		RegisterInitResolver(varDescriptor);

		return varDescriptor;
	}

	public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] _)
	{
		var ns = LookupNamespace(prefix);

=======
	public required IEnumerable<IXPathFunctionProvider> FunctionProviders         { private get; [UsedImplicitly] init; }
	public required Func<string, XPathVarDescriptor>    XPathVarDescriptorFactory { private get; [UsedImplicitly] init; }
	public required Func<ValueTask<XPathEngine>>        XPathEngineFactory        { private get; [UsedImplicitly] init; }

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

>>>>>>> Stashed changes
		foreach (var provider in FunctionProviders)
		{
			if (provider.TryGetFunction(ns, name) is { } function)
			{
<<<<<<< Updated upstream
				RegisterInitResolver(function);
=======
				_functionDescriptors ??= [];
				_functionDescriptors.Add(function);
>>>>>>> Stashed changes

				return function;
			}
		}

		throw new XPathDataModelException(Res.Format(Resources.Exception_UnknownXPathFunction, ns, name));
	}

<<<<<<< Updated upstream
	private void RegisterInitResolver(object resolver)
	{
		if (resolver is IInitResolver initResolver)
		{
			_initResolvers ??= new List<IInitResolver>();

			_initResolvers.Add(initResolver);
		}
	}

=======
>>>>>>> Stashed changes
	public override bool PreserveWhitespace(XPathNavigator node) => false;

	public override int CompareDocument(string baseUri, string nextbaseUri) => string.CompareOrdinal(baseUri, nextbaseUri);

	public override string LookupNamespace(string prefix) => base.LookupNamespace(prefix) ?? throw new XPathDataModelException(Res.Format(Resources.Exception_PrefixCantBeResolved, prefix));

<<<<<<< Updated upstream
	public async ValueTask InitResolvers()
	{
		var initResolvers = _initResolvers;
		_initResolvers = default;

		if (initResolvers is not null)
		{
			foreach (var initResolver in initResolvers)
			{
				await initResolver.Initialize().ConfigureAwait(false);
=======
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

	private async ValueTask Initialize(List<XPathVarDescriptor>? varDescriptors, List<XPathFunctionDescriptorBase>? functionDescriptors)
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
>>>>>>> Stashed changes
			}
		}
	}
}