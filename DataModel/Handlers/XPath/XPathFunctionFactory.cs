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

using System.Collections.Generic;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath
{
	internal class XPathFunctionFactory : IXPathFunctionFactory
	{
		private readonly Dictionary<(string Namespace, string Name), IXsltContextFunction> _functionDescriptors = new();

		private XPathFunctionFactory()
		{
			RegisterFunction(new InFunction());
		}

		public static IXPathFunctionFactory Instance { get; } = new XPathFunctionFactory();

	#region Interface IXPathFunctionFactory

		public IXsltContextFunction ResolveFunction(string ns, string name)
		{
			if (_functionDescriptors.TryGetValue((ns, name), out var descriptor))
			{
				return descriptor;
			}

			throw new XPathDataModelException(Res.Format(Resources.Exception_UnknownXPathFunction, ns, name));
		}

	#endregion

		private void RegisterFunction(XPathFunctionDescriptorBase descriptor)
		{
			_functionDescriptors.Add((descriptor.Namespace, descriptor.Name), descriptor);
		}
	}
}