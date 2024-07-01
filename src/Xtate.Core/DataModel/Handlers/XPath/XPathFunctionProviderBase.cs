<<<<<<< Updated upstream
﻿using System;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath;

public abstract class XPathFunctionProviderBase<TXPathFunction> : IXPathFunctionProvider where TXPathFunction : class, IXsltContextFunction
{
	public required Func<TXPathFunction> XPathFunctionFactory { private get; init; }

#region Interface IXPathFunctionProvider

	public IXsltContextFunction? TryGetFunction(string ns, string name) => CanHandle(ns, name) ? XPathFunctionFactory() : default;
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.DataModel.XPath;

public abstract class XPathFunctionProviderBase<TXPathFunction> : IXPathFunctionProvider where TXPathFunction : XPathFunctionDescriptorBase
{
	public required Func<TXPathFunction> XPathFunctionFactory { private get; [UsedImplicitly] init; }

#region Interface IXPathFunctionProvider

	public XPathFunctionDescriptorBase? TryGetFunction(string ns, string name) => CanHandle(ns, name) ? XPathFunctionFactory() : default;
>>>>>>> Stashed changes

#endregion

	protected abstract bool CanHandle(string ns, string name);
}