#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class XPathEngine
	{
		public static readonly object Key = new object();

		private readonly XPathResolver _resolver;

		public XPathEngine(IExecutionContext executionContext) => _resolver = new XPathResolver(XPathFunctionFactory.Instance, executionContext);

		public static XPathEngine GetEngine(IExecutionContext executionContext)
		{
			var engine = (XPathEngine?) executionContext.RuntimeItems[Key];

			Infrastructure.Assert(engine != null);

			return engine;
		}

		public XPathObject EvalObject(XPathCompiledExpression compiledExpression) => new XPathObject(_resolver.Evaluate(compiledExpression));

		public void Assign(XPathCompiledExpression compiledLeftExpression, IObject rightValue)
		{
			var result = _resolver.Evaluate(compiledLeftExpression);

			if (!(result is XPathNodeIterator iterator))
			{
				return;
			}

			foreach (DataModelXPathNavigator navigator in iterator)
			{
				switch (rightValue)
				{
					case XPathAssignObject assignObject:
						Assign(navigator, assignObject.AssignType, assignObject.AssignAttributeName, assignObject);
						break;
					default:
						Assign(navigator, XPathAssignType.ReplaceChildren, attributeName: default, rightValue);
						break;
				}
			}
		}

		private static void Assign(DataModelXPathNavigator navigator, XPathAssignType assignType, string? attributeName, IObject valueObject)
		{
			switch (assignType)
			{
				case XPathAssignType.ReplaceChildren:
					navigator.ReplaceChildren(valueObject);
					break;
				case XPathAssignType.FirstChild:
					navigator.FirstChild(valueObject);
					break;
				case XPathAssignType.LastChild:
					navigator.LastChild(valueObject);
					break;
				case XPathAssignType.PreviousSibling:
					navigator.PreviousSibling(valueObject);
					break;
				case XPathAssignType.NextSibling:
					navigator.NextSibling(valueObject);
					break;
				case XPathAssignType.Replace:
					navigator.Replace(valueObject);
					break;
				case XPathAssignType.Delete:
					navigator.DeleteSelf();
					break;
				case XPathAssignType.AddAttribute:
					Infrastructure.Assert(attributeName != null);
					navigator.CreateAttribute(string.Empty, attributeName, string.Empty, valueObject.ToString());
					break;
				default:
					Infrastructure.UnexpectedValue();
					break;
			}
		}

		public string GetName(XPathCompiledExpression compiledExpression) => _resolver.GetName(compiledExpression);

		public void EnterScope() => _resolver.EnterScope();

		public void LeaveScope() => _resolver.LeaveScope();

		public void DeclareVariable(XPathCompiledExpression compiledExpression) => _resolver.DeclareVariable(compiledExpression);
	}
}