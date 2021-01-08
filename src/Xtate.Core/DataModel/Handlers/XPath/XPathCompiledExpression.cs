#region Copyright © 2019-2020 Sergii Artemenko

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

using System.Xml;
using System.Xml.XPath;
using Xtate.Core;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath
{
	internal class XPathCompiledExpression
	{
		private static readonly XPathResolver ParseExpressionResolver = new(XPathFunctionFactory.Instance);

		private readonly XPathExpressionContext _context;

		public XPathCompiledExpression(string expression, object? entity)
		{
			entity.Is<XmlNameTable>(out var nameTable);
			nameTable ??= new NameTable();

			_context = new XPathExpressionContext(ParseExpressionResolver, nameTable);

			if (entity.Is<IXmlNamespacesInfo>(out var xmlNamespacesInfo))
			{
				foreach (var prefixUri in xmlNamespacesInfo.Namespaces)
				{
					_context.AddNamespace(prefixUri.Prefix, prefixUri.Namespace);
				}
			}

			XPathExpression = XPathExpression.Compile(expression, _context);
		}

		public XPathResultType ReturnType => XPathExpression.ReturnType;

		public string Expression => XPathExpression.Expression;

		public XPathExpression XPathExpression { get; }

		public void SetResolver(XPathResolver resolver) => _context.SetResolver(resolver);
	}
}