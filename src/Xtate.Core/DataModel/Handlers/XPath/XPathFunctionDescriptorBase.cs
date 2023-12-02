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

using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath;

public abstract class XPathFunctionDescriptorBase : IXsltContextFunction
{
	protected XPathFunctionDescriptorBase(XPathResultType returnType, params XPathResultType[] argTypes)
	{
		ArgTypes = argTypes;
		ReturnType = returnType;
	}

	public virtual ValueTask Initialize() => default;

#region Interface IXsltContextFunction

	public virtual object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext) => Invoke(args)!;

	public virtual XPathResultType[] ArgTypes { get; }

	public virtual XPathResultType ReturnType { get; }

	public virtual int Maxargs => ArgTypes.Length;

	public virtual int Minargs => ArgTypes.Length;

#endregion

	protected abstract object? Invoke(object[] args);
}