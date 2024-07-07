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

namespace Xtate.DataModel.XPath;

public class XPathVarDescriptor : IXsltContextVariable
{
	private static readonly XPathNodeIterator Empty = new EmptyIterator();

	protected XPathEngine? Engine { get; private set; }

	public required string Name { protected get; [UsedImplicitly] init; }

#region Interface IXsltContextVariable

	public virtual object Evaluate(XsltContext xsltContext) => Engine?.GetVariable(Name) ?? Empty;

	public virtual bool IsLocal => false;

	public virtual bool IsParam => false;

	public virtual XPathResultType VariableType => XPathResultType.NodeSet;

#endregion

	public virtual ValueTask Initialize(XPathEngine engine)
	{
		Engine = engine;

		return default;
	}

	private class EmptyIterator : XPathNodeIterator
	{
		public override XPathNavigator Current => default!;

		public override int CurrentPosition => 0;

		public override XPathNodeIterator Clone() => this;

		public override bool MoveNext() => false;
	}
}