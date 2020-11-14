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

using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath
{
	internal class XPathResolver
	{
		private readonly IXPathFunctionFactory                     _functionFactory;
		private          Stack<DataModelList>?                     _scopeStack;
		private          Dictionary<string, IXsltContextVariable>? _varDescriptors;

		public XPathResolver(IXPathFunctionFactory functionFactory, IExecutionContext? executionContext = null)
		{
			_functionFactory = functionFactory;
			ExecutionContext = executionContext;
		}

		public IExecutionContext? ExecutionContext { get; }

		public IXsltContextFunction ResolveFunction(string ns, string name) => _functionFactory.ResolveFunction(ns, name);

		public IXsltContextVariable ResolveVariable(string ns, string name)
		{
			if (!string.IsNullOrEmpty(ns))
			{
				throw new XPathDataModelException(Res.Format(Resources.Exception_Unknown_XPath_variable, ns, name));
			}

			_varDescriptors ??= new Dictionary<string, IXsltContextVariable>();

			if (!_varDescriptors.TryGetValue(name, out var descriptor))
			{
				descriptor = new XPathVarDescriptor(name);

				_varDescriptors.Add(name, descriptor);
			}

			return descriptor;
		}

		public void EnterScope()
		{
			_scopeStack ??= new Stack<DataModelList>();
			_scopeStack.Push(new DataModelList());
		}

		public void LeaveScope()
		{
			Infrastructure.NotNull(_scopeStack);

			_scopeStack.Pop();
		}

		public object Evaluate(XPathCompiledExpression compiledExpression)
		{
			Infrastructure.NotNull(ExecutionContext);

			compiledExpression.SetResolver(this);

			return new DataModelXPathNavigator(ExecutionContext.DataModel).Evaluate(compiledExpression.XPathExpression)!;
		}

		public void DeclareVariable(XPathCompiledExpression compiledExpression)
		{
			if (_scopeStack is not null)
			{
				_scopeStack.Peek()[compiledExpression.Expression] = default;
			}
		}

		public string GetName(XPathCompiledExpression compiledExpression) => compiledExpression.Expression;

		public XPathNodeIterator GetVariable(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			if (_scopeStack is not null)
			{
				foreach (var vars in _scopeStack)
				{
					if (vars.ContainsKey(name, caseInsensitive: false))
					{
						return CreateIterator(vars, name);
					}
				}
			}

			if (ExecutionContext is null)
			{
				return XPathEmptyIterator.Instance;
			}

			var global = ExecutionContext.DataModel;

			if (!global.ContainsKey(name, caseInsensitive: false))
			{
				global[name, caseInsensitive: false] = default;
			}

			return CreateIterator(global, name);
		}

		private static XPathNodeIterator CreateIterator(DataModelList list, string key)
		{
			var navigator = new DataModelXPathNavigator(list);

			navigator.MoveToFirstChild();
			while (navigator.Name != key)
			{
				var moved = navigator.MoveToNext();

				Infrastructure.Assert(moved);
			}

			return new XPathSingleElementIterator(navigator);
		}
	}
}