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
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Xsl;
using Xtate.Core;

namespace Xtate.DataModel.XPath;

public class XPathEngine
{
	private readonly DataModelList        _root;
	private readonly Stack<DataModelList> _scopeStack = new();

	public XPathEngine(IDataModelController? dataModelController) => _root = dataModelController?.DataModel ?? new DataModelList(false);

	//public required Func<string, XPathVarDescriptorOld>    XPathVarDescriptorFactory { private get; init; }
	/*
	public IXsltContextVariable ResolveVariable(string ns, string name)
	{
		if (!string.IsNullOrEmpty(ns))
		{
			throw new XPathDataModelException(Res.Format(Resources.Exception_UnknownXPathVariable, ns, name));
		}

		return XPathVarDescriptorFactory(name);
	}*/

	public object GetVariable(string name)
	{
		if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

		foreach (var vars in _scopeStack)
		{
			if (vars.ContainsKey(name, caseInsensitive: false))
			{
				return CreateIterator(vars, name);
			}
		}

		if (!_root.ContainsKey(name, caseInsensitive: false))
		{
			_root[name, caseInsensitive: false] = default;
		}

		return CreateIterator(_root, name);
	}

	private static object? GetVariableValue(DataModelList list, string key)
	{
		var value = list[key, caseInsensitive: false];

		switch (value.Type)
		{
			case DataModelValueType.List:     return CreateIterator(list, key);
			case DataModelValueType.String:   return value.AsString();
			case DataModelValueType.Boolean:  return value.AsBoolean();
			case DataModelValueType.Number:   return value.AsNumber();
			case DataModelValueType.DateTime: return value.AsDateTime().ToString(@"O");
			default:                          return default;
		}
	}

	private static XPathNodeIterator CreateIterator(DataModelList list, string key)
	{
		var navigator = new DataModelXPathNavigator(list);

		navigator.MoveToFirstChild();
		while (navigator.Name != key)
		{
			var moved = navigator.MoveToNext();

			Infra.Assert(moved);
		}

		return new XPathSingleElementIterator(navigator);
	}

	public async ValueTask<XPathObject> EvalObject(XPathCompiledExpression compiledExpression, bool stripRoots)
	{
		Infra.Requires(compiledExpression);

		var value = await Evaluate(compiledExpression).ConfigureAwait(false);

		if (stripRoots && value is XPathNodeIterator iterator)
		{
			value = new XPathStripRootsIterator(iterator);
		}

		return new XPathObject(value);
	}

	public async ValueTask Assign1(XPathCompiledExpression compiledLeftExpression,
							XPathAssignType assignType,
							string? attributeName,
							IObject rightValue)
	{
		Infra.Requires(compiledLeftExpression);
		Infra.Requires(rightValue);

		var result = await Evaluate(compiledLeftExpression).ConfigureAwait(false);

		if (result is not XPathNodeIterator iterator)
		{
			return;
		}

		foreach (DataModelXPathNavigator navigator in iterator)
		{
			Assign(navigator, assignType, attributeName, rightValue);
		}
	}

	private async ValueTask<object> Evaluate(XPathCompiledExpression compiledExpression)
	{
		var xPathExpression = await compiledExpression.GetXPathExpression().ConfigureAwait(false);

		return new DataModelXPathNavigator(_root).Evaluate(xPathExpression)!;
	}

	private static void Assign(DataModelXPathNavigator navigator,
							   XPathAssignType assignType,
							   string? attributeName,
							   IObject valueObject)
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
				Infra.NotNull(attributeName);
				var value = Convert.ToString(valueObject.ToObject(), CultureInfo.InvariantCulture);
				navigator.CreateAttribute(string.Empty, attributeName, string.Empty, value ?? string.Empty);
				break;
			default:
				Infra.Unexpected(assignType);
				break;
		}
	}

	public string GetName(XPathCompiledExpression compiledExpression)
	{
		Infra.Requires(compiledExpression);

		return compiledExpression.Expression;
	}

	public void EnterScope()
	{
		_scopeStack.Push(new DataModelList());
	}

	public void LeaveScope()
	{
		_scopeStack.Pop();
	}

	public void DeclareVariable(XPathCompiledExpression compiledExpression)
	{
		Infra.Requires(compiledExpression);

		if (_scopeStack.Count > 0)
		{
			_scopeStack.Peek()[compiledExpression.Expression] = default;
		}
	}
}