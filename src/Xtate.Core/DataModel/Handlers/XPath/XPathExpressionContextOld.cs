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
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Xsl;
using Xtate.Core;
using Xtate.IoC;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath;

public class XPathExpressionContextOld : XPathExpressionContext, IAsyncInitialization
{
	//public required Func<string, ValueTask<XPathVarDescriptorOld>> XPathVarDescriptorFactory { private get; init; }

	public XPathExpressionContextOld(INameTableProvider nameTableProvider, Func<ValueTask<XPathEngine>> engineFactory, IXmlNamespacesInfo? xmlNamespacesInfo) : base(nameTableProvider, xmlNamespacesInfo)
	{
		_engineAsyncInit = AsyncInit.RunNow(engineFactory);
	}

	private readonly AsyncInit<XPathEngine> _engineAsyncInit;

	public Task Initialization => _engineAsyncInit.Task;

	//public override IXsltContextVariable ResolveVariable(string prefix, string name) => _engineAsyncInit.Value.ResolveVariable(LookupNamespace(prefix), name);
}