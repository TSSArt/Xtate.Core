#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System.Collections.Generic;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath
{
	internal class XPathFunctionFactory
	{
		public static readonly XPathFunctionFactory Instance = new XPathFunctionFactory();

		private readonly Dictionary<(string Namespace, string Name), IXsltContextFunction> _functionDescriptors = new Dictionary<(string Namespace, string Name), IXsltContextFunction>();

		private XPathFunctionFactory()
		{
			RegisterFunction<InFunction>();
		}

		private void RegisterFunction<T>() where T : XPathFunctionDescriptorBase, new()
		{
			var descriptor = new T();

			_functionDescriptors.Add((descriptor.Namespace, descriptor.Name), descriptor);
		}

		public IXsltContextFunction ResolveFunction(string ns, string name)
		{
			if (_functionDescriptors.TryGetValue((ns, name), out var descriptor))
			{
				return descriptor;
			}

			throw new XPathDataModelException(Res.Format(Resources.Exception_Unknown_XPath_function, ns, name));
		}
	}
}