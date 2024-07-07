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

namespace Xtate.DataModel.XPath;

public class XPathLocationExpression(ILocationExpression locationExpression, XPathAssignType assignType, string? attribute) : ILocationExpression, IAncestorProvider
{
	public XPathAssignType AssignType { get; } = assignType;

	public string? Attribute { get; } = attribute;

#region Interface IAncestorProvider

	public object Ancestor => locationExpression;

#endregion

#region Interface ILocationExpression

	public string? Expression => locationExpression.Expression;

#endregion

	public static bool TryParseAssignType(string? value, out XPathAssignType assignType)
	{
		switch (value)
		{
			case null:
			case "":
			case "replacechildren":
				assignType = XPathAssignType.ReplaceChildren;

				return true;

			case "firstchild":
				assignType = XPathAssignType.FirstChild;

				return true;

			case "lastchild":
				assignType = XPathAssignType.LastChild;

				return true;

			case "previoussibling":
				assignType = XPathAssignType.PreviousSibling;

				return true;

			case "nextsibling":
				assignType = XPathAssignType.NextSibling;

				return true;

			case "replace":
				assignType = XPathAssignType.Replace;

				return true;

			case "delete":
				assignType = XPathAssignType.Delete;

				return true;

			case "addattribute":
				assignType = XPathAssignType.AddAttribute;

				return true;

			default:
				assignType = default;

				return false;
		}
	}
}