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
using System.Threading.Tasks;
using System.Xml.XPath;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath
{
	public class XPathCompiledExpression
	{
		private readonly XPathExpression         _xPathExpression;
		private          XPathExpressionContext? _expressionContext;

		public XPathCompiledExpression(string expression,
									   IXmlNamespacesInfo? xmlNamespacesInfo,
									   Func<IXmlNamespacesInfo?, XPathExpressionContext> xPathExpressionContextFactory)
		{
			_expressionContext = xPathExpressionContextFactory(xmlNamespacesInfo);
			_xPathExpression = XPathExpression.Compile(expression, _expressionContext);
		}

		public XPathResultType ReturnType => _xPathExpression.ReturnType;

		public string Expression => _xPathExpression.Expression;

		public async ValueTask<XPathExpression> GetXPathExpression()
		{
			if (_expressionContext is { } context)
			{
				await context.InitResolvers().ConfigureAwait(false);

				_expressionContext = default;
			}

			return _xPathExpression;
		}
	}
}