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
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath;

public class XPathVarDescriptor : IXsltContextVariable, IInitResolver
{
	public required Func<ValueTask<XPathEngine>> XPathEngineFactory { private get; init; }

	private readonly string       _name;
	private          XPathEngine? _engine;

	public XPathVarDescriptor(string name) => _name = name;

#region Interface IXsltContextVariable

	public virtual object Evaluate(XsltContext xsltContext) => _engine?.GetVariable(_name);

	public bool IsLocal => false;
	public bool IsParam => false;

	public XPathResultType VariableType => XPathResultType.NodeSet;

#endregion

	public async ValueTask Initialize() => _engine = await XPathEngineFactory().ConfigureAwait(false);
}